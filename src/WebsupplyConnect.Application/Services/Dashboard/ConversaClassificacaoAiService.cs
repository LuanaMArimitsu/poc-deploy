using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.Interfaces.Configuracao;
using WebsupplyConnect.Application.Interfaces.Dashboard;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Dashboard;

public class ConversaClassificacaoAiService(
    ILogger<ConversaClassificacaoAiService> logger,
    IConversaRepository conversaRepository,
    IEmpresaReaderService empresaReaderService,
    IOpenAiService openAiService,
    IUnitOfWork unitOfWork,
    IPromptConfiguracaoService promptConfiguracaoService) : IConversaClassificacaoAiService
{
    private static readonly JsonSerializerOptions EmpresaConfigJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    private readonly ILogger<ConversaClassificacaoAiService> _logger = logger;
    private readonly IConversaRepository _conversaRepository = conversaRepository;
    private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService;
    private readonly IOpenAiService _openAiService = openAiService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPromptConfiguracaoService _promptConfiguracaoService = promptConfiguracaoService;

    public async Task<ConversaClassificacaoSobDemandaResultado> ProcessarConversaSobDemandaAsync(
        int conversaId,
        bool executarExtracaoContexto,
        bool executarDeteccaoContato,
        bool executarClassificacaoConversa,
        CancellationToken cancellationToken = default)
    {
        var conversa = await _conversaRepository.GetConversaParaClassificacaoPorIdAsync(conversaId);
        if (conversa == null)
            return new ConversaClassificacaoSobDemandaResultado(false, conversaId, false, false, false);

        var extracaoProcessada = false;
        var deteccaoProcessada = false;
        var classificacaoProcessada = false;

        if (executarExtracaoContexto && !cancellationToken.IsCancellationRequested)
            extracaoProcessada = await ProcessarExtracaoContextoConversaAsync(conversa, cancellationToken);

        if (executarDeteccaoContato && !cancellationToken.IsCancellationRequested)
            deteccaoProcessada = await ProcessarDeteccaoContatoConversaAsync(conversa, cancellationToken);

        if (executarClassificacaoConversa && !cancellationToken.IsCancellationRequested)
            classificacaoProcessada = await ProcessarClassificacaoConversaAsync(conversa, cancellationToken);

        return new ConversaClassificacaoSobDemandaResultado(
            true,
            conversaId,
            extracaoProcessada,
            deteccaoProcessada,
            classificacaoProcessada);
    }

    private async Task<bool> ProcessarExtracaoContextoConversaAsync(Conversa conversa, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return false;

        var mensagensElegiveis = ObterMensagensElegiveis(conversa, excluirMensagensBot: true);

        if (mensagensElegiveis.Count == 0) return false;

        var empresaId = conversa.Lead?.EmpresaId ?? 0;
        if (empresaId == 0) return false;

        var configOpenAi = await ObterConfiguracaoOpenAiAsync(empresaId);
        if (configOpenAi == null) return false;

        var prompt = await _promptConfiguracaoService.ObterConteudoAsync("EXTRACAO_CONTEXTO", empresaId, cancellationToken);
        if (string.IsNullOrEmpty(prompt)) return false;

        var ultimaMensagem = mensagensElegiveis[^1];
        var ultimaMensagemTexto = string.IsNullOrWhiteSpace(ultimaMensagem.Conteudo) ? "[mensagem sem conteúdo textual]" : ultimaMensagem.Conteudo.Trim();
        prompt = prompt.Replace("{ULTIMA_MENSAGEM}", ultimaMensagemTexto);

        var contextoMensagens = BuildConversationContext(mensagensElegiveis);
        var promptUsuario = $"{prompt}\n\nContexto:\n{contextoMensagens}";

        _openAiService.GetConfig(configOpenAi);
        var resumo = await _openAiService.GenerateClassificacaoAsync(
            configOpenAi,
            "Responda APENAS JSON válido: {\"contextoResumo\":\"...\",\"pendencia\":\"...\",\"acaoVendedor\":\"...\"}",
            promptUsuario);

        var jsonContexto = ParseContextoJson(resumo);
        if (string.IsNullOrEmpty(jsonContexto)) return false;

        var agora = TimeHelper.GetBrasiliaTime();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _conversaRepository.AtualizarContextoAsync(conversa.Id, jsonContexto, agora);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("[EXTRACAO CONTEXTO] Conversa {ConversaId} processada com sucesso", conversa.Id);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task<bool> ProcessarDeteccaoContatoConversaAsync(Conversa conversa, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return false;

        var mensagensElegiveis = ObterMensagensElegiveis(conversa, excluirMensagensBot: false);

        if (mensagensElegiveis.Count == 0) return false;

        var empresaId = conversa.Lead?.EmpresaId ?? 0;
        if (empresaId == 0) return false;

        var configOpenAi = await ObterConfiguracaoOpenAiAsync(empresaId);
        if (configOpenAi == null) return false;

        var promptSistema = await _promptConfiguracaoService.ObterConteudoAsync("DETECCAO_CONTATO", empresaId, cancellationToken);
        if (string.IsNullOrEmpty(promptSistema)) return false;

        var payload = MontarPayloadClassificacao(conversa, mensagensElegiveis);

        _openAiService.GetConfig(configOpenAi);
        var resposta = await _openAiService.GenerateClassificacaoAsync(configOpenAi, promptSistema, payload);

        var trocaDeContato = ParseTrocaDeContato(resposta);
        var agora = TimeHelper.GetBrasiliaTime();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _conversaRepository.AtualizarClassificacaoAsync(conversa.Id, trocaDeContato, null, agora);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("[DETECCAO CONTATO] Conversa {ConversaId} processada com sucesso", conversa.Id);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task<bool> ProcessarClassificacaoConversaAsync(Conversa conversa, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return false;

        var mensagensElegiveis = ObterMensagensElegiveis(conversa, excluirMensagensBot: false);

        if (mensagensElegiveis.Count == 0) return false;

        var empresaId = conversa.Lead?.EmpresaId ?? 0;
        if (empresaId == 0) return false;

        var configOpenAi = await ObterConfiguracaoOpenAiAsync(empresaId);
        if (configOpenAi == null) return false;

        var promptSistema = await _promptConfiguracaoService.ObterConteudoAsync("CLASSIFICACAO_CONVERSA", empresaId, cancellationToken);
        if (string.IsNullOrEmpty(promptSistema)) return false;

        var payload = MontarPayloadClassificacao(conversa, mensagensElegiveis);

        _openAiService.GetConfig(configOpenAi);
        var resposta = await _openAiService.GenerateClassificacaoAsync(configOpenAi, promptSistema, payload);

        var (trocaDeContato, classificacaoIA) = ParseClassificacaoResposta(resposta);

        var agora = TimeHelper.GetBrasiliaTime();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _conversaRepository.AtualizarClassificacaoAsync(conversa.Id, trocaDeContato, classificacaoIA, agora);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("[CLASSIFICACAO CONVERSA] Conversa {ConversaId} processada com sucesso", conversa.Id);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static List<Mensagem> ObterMensagensElegiveis(Conversa conversa, bool excluirMensagensBot)
    {
        var query = (conversa.Mensagens ?? [])
            .Where(m => !m.Excluido);

        if (excluirMensagensBot)
            query = query.Where(m => m.Usuario?.IsBot != true);

        return query
            .OrderBy(m => m.DataEnvio ?? m.DataCriacao)
            .ToList();
    }

    private static string MontarPayloadClassificacao(Conversa conversa, List<Mensagem> mensagens)
    {
        var currentDatetime = TimeHelper.GetBrasiliaTime();
        var currentDatetimeStr = currentDatetime.ToString("yyyy-MM-ddTHH:mm:sszzz");

        var messages = mensagens.Select(m =>
        {
            var dataMsg = m.DataEnvio ?? m.DataCriacao;
            var senderRole = m.Sentido == 'R' ? "customer" : (m.Usuario?.IsBot == true ? "bot" : "seller");
            return new
            {
                message_id = m.Id.ToString(),
                sender_role = senderRole,
                message_text = m.Conteudo ?? "",
                message_datetime = dataMsg.ToString("yyyy-MM-ddTHH:mm:sszzz")
            };
        });

        var payload = new
        {
            conversation_id = conversa.Id.ToString(),
            current_datetime = currentDatetimeStr,
            messages
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private static string BuildConversationContext(List<Mensagem> mensagens)
    {
        var sb = new StringBuilder();
        foreach (var m in mensagens)
        {
            var autor = m.Sentido == 'R' ? "CLIENTE" : (m.Usuario?.IsBot == true ? "BOT" : "VENDEDOR");
            var texto = !string.IsNullOrWhiteSpace(m.Conteudo) ? m.Conteudo : "[mídia ou template]";
            sb.AppendLine($"{autor}: {texto}");
        }
        return sb.ToString();
    }

    private static string? ParseContextoJson(string resposta)
    {
        try
        {
            var inicio = resposta.IndexOf('{');
            var fim = resposta.LastIndexOf('}');
            if (inicio < 0 || fim <= inicio) return null;

            var json = resposta[inicio..(fim + 1)];
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? GetStr(string name)
            {
                if (root.TryGetProperty(name, out var p))
                    return p.GetString();
                var camel = char.ToLowerInvariant(name[0]) + name[1..];
                if (root.TryGetProperty(camel, out var p2))
                    return p2.GetString();
                return null;
            }

            var resumo = GetStr("contextoResumo") ?? GetStr("contexto_resumo") ?? "";
            var pendencia = GetStr("pendencia") ?? "";
            var acao = GetStr("acaoVendedor") ?? GetStr("acao_vendedor") ?? "";

            var result = new Dictionary<string, string>
            {
                ["contextoResumo"] = resumo,
                ["pendencia"] = pendencia,
                ["acaoVendedor"] = acao
            };
            var serialized = JsonSerializer.Serialize(result);
            return serialized.Length <= 500 ? serialized : serialized[..500];
        }
        catch
        {
            return null;
        }
    }

    private static bool ParseTrocaDeContato(string resposta)
    {
        try
        {
            var inicio = resposta.IndexOf('{');
            var fim = resposta.LastIndexOf('}');
            if (inicio < 0 || fim <= inicio) return false;

            using var doc = JsonDocument.Parse(resposta[inicio..(fim + 1)]);
            var root = doc.RootElement;
            if (root.TryGetProperty("troca_de_contato", out var prop))
                return prop.GetBoolean();
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static (bool TrocaDeContato, string? ClassificacaoIA) ParseClassificacaoResposta(string resposta)
    {
        try
        {
            var inicio = resposta.IndexOf('{');
            var fim = resposta.LastIndexOf('}');
            if (inicio < 0 || fim <= inicio) return (false, null);

            var json = resposta[inicio..(fim + 1)];
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var trocaDeContato = false;
            if (root.TryGetProperty("troca_de_contato", out var prop))
                trocaDeContato = prop.ValueKind == JsonValueKind.True;

            return (trocaDeContato, json);
        }
        catch
        {
            return (false, null);
        }
    }

    private async Task<Openai?> ObterConfiguracaoOpenAiAsync(int empresaId)
    {
        var json = await _empresaReaderService.GetConfiguracaoIntegracao(empresaId);
        if (string.IsNullOrWhiteSpace(json)) return null;

        var config = JsonSerializer.Deserialize<EmpresaConfigIntegracaoDTO>(json, EmpresaConfigJsonOptions);
        return config?.OpenAI;
    }
}
