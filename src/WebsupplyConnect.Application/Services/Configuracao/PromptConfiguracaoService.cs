using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.Configuracao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Entities.Configuracao;
using WebsupplyConnect.Domain.Interfaces.Configuracao;

namespace WebsupplyConnect.Application.Services.Configuracao;

/// <summary>
/// Serviço de resolução de prompts com suporte a cache Redis e fallback para arquivo.
/// Implementa estratégia Cache-Aside:
/// 1. Tentar obter do Redis
/// 2. Se miss, obter do banco (última versão publicada)
/// 3. Se não encontrar no banco, tentar arquivo .txt
/// 4. Cachear no Redis (exceto fallback de arquivo)
/// </summary>
public class PromptConfiguracaoService(
    ILogger<PromptConfiguracaoService> logger,
    IPromptConfiguracaoRepository promptRepository,
    IRedisCacheService redisCacheService,
    IOptions<RedisConfiguration> redisConfig,
    IOptions<ConversaClassificacaoConfig> conversaConfig)
    : IPromptConfiguracaoService
{
    private readonly ILogger<PromptConfiguracaoService> _logger = logger;
    private readonly IPromptConfiguracaoRepository _promptRepository = promptRepository;
    private readonly IRedisCacheService _redisCacheService = redisCacheService;
    private readonly RedisConfiguration _redisConfig = redisConfig.Value;
    private readonly ConversaClassificacaoConfig _conversaConfig = conversaConfig.Value;

    private const string CACHE_KEY_PREFIX = "prompt-config";
    private const string LOG_PREFIX = "[PROMPT-CONFIG]";

    public async Task<string?> ObterConteudoAsync(
        string codigo,
        int? empresaId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            _logger.LogWarning("{LogPrefix} Código de prompt vazio fornecido", LOG_PREFIX);
            return null;
        }

        var chaveCache = MontarChaveCache(codigo, empresaId);

        try
        {
            // 1. Tentar obter do Redis (Cache HIT)
            var conteudoCache = await _redisCacheService.GetStringAsync(chaveCache);
            if (!string.IsNullOrEmpty(conteudoCache))
            {
                _logger.LogInformation(
                    "{LogPrefix} Prompt '{Codigo}' carregado do cache Redis.",
                    LOG_PREFIX, codigo);
                return conteudoCache;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "{LogPrefix} Erro ao obter prompt '{Codigo}' do Redis: {Mensagem}. Continuando...",
                LOG_PREFIX, codigo, ex.Message);
            // Continua para banco de dados
        }

        try
        {
            // 2. Obter do banco (última versão publicada)
            var config = await _promptRepository.ObterPorCodigoAsync(codigo);
            if (config == null)
            {
                _logger.LogInformation(
                    "{LogPrefix} Configuração '{Codigo}' não encontrada no banco. Tentando fallback de arquivo.",
                    LOG_PREFIX, codigo);
                return await ObterDoArquivoAsync(codigo);
            }

            var versao = await _promptRepository.ObterUltimaVersaoPublicadaAsync(config.Id);
            if (versao == null)
            {
                _logger.LogWarning(
                    "{LogPrefix} Nenhuma versão publicada de '{Codigo}' encontrada no banco. Tentando fallback de arquivo.",
                    LOG_PREFIX, codigo);
                return await ObterDoArquivoAsync(codigo);
            }

            // 3. Registrar uso
            versao.RegistrarUso();

            // 4. Cachear no Redis
            var ttl = TimeSpan.FromDays(_redisConfig.CacheExpirationInDays);
            try
            {
                await _redisCacheService.SetStringAsync(chaveCache, versao.ConteudoPrompt, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "{LogPrefix} Erro ao cachear prompt '{Codigo}' no Redis: {Mensagem}",
                    LOG_PREFIX, codigo, ex.Message);
                // Continua mesmo se o cache falhar
            }

            _logger.LogInformation(
                "{LogPrefix} Prompt '{Codigo}' carregado do banco (versão {NumeroVersao}).",
                LOG_PREFIX, codigo, versao.NumeroVersao);

            return versao.ConteudoPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogPrefix} Erro ao obter prompt '{Codigo}' do banco: {Mensagem}. Tentando fallback de arquivo.",
                LOG_PREFIX, codigo, ex.Message);
            return await ObterDoArquivoAsync(codigo);
        }
    }

    public async Task<PromptConfiguracaoVersao?> ObterVersaoExecutavelAsync(
        string codigo,
        int? empresaId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        try
        {
            var config = await _promptRepository.ObterPorCodigoAsync(codigo);
            if (config == null)
                return null;

            var versao = await _promptRepository.ObterUltimaVersaoPublicadaAsync(config.Id);
            return versao;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogPrefix} Erro ao obter versão executável de '{Codigo}': {Mensagem}",
                LOG_PREFIX, codigo, ex.Message);
            return null;
        }
    }

    public async Task InvalidarCacheAsync(string codigo, int? empresaId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return;

        var chaveCache = MontarChaveCache(codigo, empresaId);

        try
        {
            await _redisCacheService.RemoveAsync(chaveCache);
            _logger.LogInformation(
                "{LogPrefix} Cache do prompt '{Codigo}' invalidado.",
                LOG_PREFIX, codigo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "{LogPrefix} Erro ao invalidar cache do prompt '{Codigo}': {Mensagem}",
                LOG_PREFIX, codigo, ex.Message);
        }
    }

    /// <summary>
    /// Monta a chave de cache: "prompt-config:{codigo}" ou "prompt-config:{codigo}:{empresaId}"
    /// </summary>
    private static string MontarChaveCache(string codigo, int? empresaId)
    {
        return empresaId.HasValue
            ? $"{CACHE_KEY_PREFIX}:{codigo}:{empresaId}"
            : $"{CACHE_KEY_PREFIX}:{codigo}";
    }

    /// <summary>
    /// Fallback: tenta ler o prompt de um arquivo .txt
    /// </summary>
    private async Task<string?> ObterDoArquivoAsync(string codigo)
    {
        try
        {
            var basePath = Path.IsPathRooted(_conversaConfig.PromptsPath)
                ? _conversaConfig.PromptsPath
                : Path.Combine(AppContext.BaseDirectory, _conversaConfig.PromptsPath);

            var nomeArquivo = $"{codigo.ToLower()}.txt";
            var caminhoArquivo = Path.Combine(basePath, nomeArquivo);

            if (!File.Exists(caminhoArquivo))
            {
                _logger.LogError(
                    "{LogPrefix} Arquivo de prompt '{NomeArquivo}' não encontrado em '{Caminho}'. Retornando null.",
                    LOG_PREFIX, nomeArquivo, caminhoArquivo);
                return null;
            }

            var conteudo = await File.ReadAllTextAsync(caminhoArquivo);
            _logger.LogWarning(
                "{LogPrefix} Prompt '{Codigo}' não encontrado no banco. Usando fallback de arquivo.",
                LOG_PREFIX, codigo);

            return conteudo;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogPrefix} Erro ao ler arquivo de fallback para prompt '{Codigo}': {Mensagem}",
                LOG_PREFIX, codigo, ex.Message);
            return null;
        }
    }
}
