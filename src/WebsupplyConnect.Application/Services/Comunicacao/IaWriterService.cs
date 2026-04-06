using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class IaWriterService(ILogger<IaWriterService> logger,
        IEmpresaReaderService empresaReaderService,
        IMensagemReaderService mensagemReaderService,
        IPromptEmpresasReaderService promptEmpresasReaderService,
        IMidiaReaderService midiaReaderService,
        IMidiaWriterService midiaWriterService,
        IOpenAiService openAIService, IBlobStorageService blobStorageService, IOptions<AzureBlobStorageConfig> config) : IIaWriterService
    {
        private readonly ILogger<IaWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly IPromptEmpresasReaderService _promptEmpresasReaderService = promptEmpresasReaderService ?? throw new ArgumentNullException(nameof(promptEmpresasReaderService));
        private readonly IMidiaReaderService _midiaReaderService = midiaReaderService ?? throw new ArgumentNullException(nameof(midiaReaderService));
        private readonly IMidiaWriterService _midiaWriterService = midiaWriterService ?? throw new ArgumentNullException(nameof(midiaWriterService));
        private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        private readonly IOpenAiService _openAIService = openAIService ?? throw new ArgumentNullException(nameof(openAIService));
        private readonly string _containerName = config.Value.ContainerNameMidiasMeta ?? throw new ArgumentNullException(nameof(config));
        private readonly string _connectionString = config.Value.AzureBlobStorageConnectionString ?? throw new ArgumentNullException(nameof(config));

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public async Task<List<MensagemSugestaoDTO>> GerarSugestoesPorEmpresa(SuggestionRequestDTO dto)
        {
            try
            {
                var empresaConfig = await _empresaReaderService.GetConfiguracaoIntegracao(dto.EmpresaId);
                if (empresaConfig == null)
                {
                    _logger.LogError("Configuração de integração da empresa com id {empresaId} não encontrada", dto.EmpresaId);
                    return [];
                }

                var config = System.Text.Json.JsonSerializer.Deserialize<EmpresaConfigIntegracaoDTO>(empresaConfig, options);
                if (config == null)
                {
                    _logger.LogError("Configuração OpenAI não encontrada para a empresa {EmpresaId}", dto.EmpresaId);
                    return [];
                }

                var mensagens = await _mensagemReaderService.GetMensagensRecentesAsync(dto.ConversaId, 0, config.OpenAI?.QuantidadeMensagens);
                if (mensagens == null || mensagens.Count == 0)
                {
                    _logger.LogWarning("Nenhuma mensagem encontrada para a conversa {ConversaId}", dto.ConversaId);
                    return [];
                }

                if (config.OpenAI == null)
                {
                    _logger.LogError("Configuração OpenAI não encontrada para a empresa {EmpresaId}", dto.EmpresaId);
                    return [];
                }

                _openAIService.GetConfig(config.OpenAI);

                foreach (var mensagem in mensagens.Where(m => m.TipoMensagem.Equals("AUDIO", StringComparison.OrdinalIgnoreCase)))
                {
                    var transcricao = await _midiaReaderService.GetTranscricaoByMensagemIdAsync(mensagem.MensagemId);

                    if (transcricao == null)
                    {
                        var resultado = await GerarTranscricaoAudio(new TranscricaoAudioRequestDTO
                        {
                            EmpresaId = dto.EmpresaId,
                            MidiaId = mensagem.MidiaId!.Value
                        });

                        if (resultado == null)
                        {
                            _logger.LogWarning("Não foi possível obter transcrição para a mensagem {MensagemId}, será ignorada nas sugestões.", mensagem.MensagemId);
                            continue;
                        }

                        transcricao = resultado.Transcricao;
                    }

                    mensagem.Conteudo = transcricao;
                }

                string? prompt = null;
                string? promptSistema = null;

                if (!string.IsNullOrWhiteSpace(dto.Rascunho))
                {
                    prompt = await _promptEmpresasReaderService.GetPromptAsync(dto.EmpresaId, false, "SUGESTAO");
                    promptSistema = await _promptEmpresasReaderService.GetPromptAsync(dto.EmpresaId, true, "SUGESTAO");
                }
                else
                {
                    prompt = await _promptEmpresasReaderService.GetPromptAsync(dto.EmpresaId, false, "SUGESTAO");
                    promptSistema = await _promptEmpresasReaderService.GetPromptAsync(dto.EmpresaId, true, "SUGESTAO");
                }

                if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrEmpty(promptSistema))
                {
                    _logger.LogError("Prompt não encontrado para a empresa {EmpresaId}", dto.EmpresaId);
                    return [];
                }

                var lista = await _openAIService.GenerateSuggestionsAsync(mensagens, config.OpenAI, prompt, promptSistema, dto.Rascunho);

                if (lista == null || lista.Count == 0)
                {
                    _logger.LogWarning("Nenhuma sugestão gerada para a conversa {ConversaId}", dto.ConversaId);
                    return [];
                }

                List<MensagemSugestaoDTO> sugestoes = [];
                foreach (var sugestao in lista)
                {
                    sugestoes.Add(new MensagemSugestaoDTO
                    {
                        Sugestao = sugestao,
                    });
                }
                return sugestoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar sugestões para a conversa {ConversaId}", dto.ConversaId);
                throw;
            }
        }

        public async Task<ResumoIaResponseDTO?> GerarResumoPorEmpresa(ResumoIaRequestDTO dto)
        {
            try
            {
                if(dto.ConversaId <= 0)
                {
                    _logger.LogError("Conversa id não pode ser nulo.");
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                if(dto.EmpresaId <= 0)
                {
                    _logger.LogError("Empresa id não pode ser nulo.");
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                var empresaConfig = await _empresaReaderService.GetConfiguracaoIntegracao(dto.EmpresaId);
                if (empresaConfig == null)
                {
                    _logger.LogError("Configuração de integração da empresa com id {empresaId} não encontrada", dto.EmpresaId);
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                var config = System.Text.Json.JsonSerializer.Deserialize<EmpresaConfigIntegracaoDTO>(empresaConfig, options);
                if (config == null)
                {
                    _logger.LogError("Configuração OpenAI não encontrada para a empresa {EmpresaId}", dto.EmpresaId);
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                var mensagens = await _mensagemReaderService.GetTodasMensagens(dto.ConversaId);
                if (mensagens == null || mensagens.Count == 0)
                {
                    _logger.LogWarning("Nenhuma mensagem encontrada para a conversa {ConversaId}", dto.ConversaId);
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                if (config.OpenAI == null)
                {
                    _logger.LogError("Configuração OpenAI não encontrada para a empresa {EmpresaId}", dto.EmpresaId);
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                _openAIService.GetConfig(config.OpenAI);

                foreach (var mensagem in mensagens.Where(m => m.TipoMensagem.Equals("AUDIO", StringComparison.OrdinalIgnoreCase)))
                {
                    var transcricao = await _midiaReaderService.GetTranscricaoByMensagemIdAsync(mensagem.MensagemId);

                    if (transcricao == null)
                    {
                        var resultado = await GerarTranscricaoAudio(new TranscricaoAudioRequestDTO
                        {
                            EmpresaId = dto.EmpresaId,
                            MidiaId = mensagem.MidiaId!.Value
                        });

                        if (resultado == null)
                        {
                            _logger.LogWarning("Não foi possível obter transcrição para a mensagem {MensagemId}, será ignorada no resumo.", mensagem.MensagemId);
                            continue;
                        }

                        transcricao = resultado.Transcricao;
                    }

                    mensagem.Conteudo = transcricao;
                }

                var prompt = await _promptEmpresasReaderService.GetPromptAsync(dto.EmpresaId, false, "RESUMO");
                var promptSistema = await _promptEmpresasReaderService.GetPromptAsync(dto.EmpresaId, true, "RESUMO");


                if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrEmpty(promptSistema))
                {
                    _logger.LogError("Prompt não encontrado para a empresa {EmpresaId}", dto.EmpresaId);
                    throw new AppException("Essa empresa não possui nenhuma instrução de resumo, por favor avise o suporte.");
                }

                var resumo = await _openAIService.GenerateResumoAsync(mensagens, config.OpenAI, prompt, promptSistema);

                if (resumo == null)
                {
                    _logger.LogError("Nenhum resumo foi gerado para a conversa {ConversaId}", dto.ConversaId);
                    throw new AppException("Ocorreu um erro ao gerar a resumo.");
                }

                ResumoIaResponseDTO resumoIaResponseDTO = new()
                {
                    Resumo = resumo
                };

                return resumoIaResponseDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar resumo para a conversa {ConversaId}", dto.ConversaId);
                throw;
            }
        }

        public async Task<TranscricaoResponseDTO?> GerarTranscricaoAudio(TranscricaoAudioRequestDTO dto)
        {
            try
            {
                if(dto.EmpresaId <= 0)
                {
                    _logger.LogError("A empresa id não pode ser nula.");
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                if (dto.MidiaId <= 0)
                {
                    _logger.LogError("A Midia id não pode ser nula.");
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                var empresaConfig = await _empresaReaderService.GetConfiguracaoIntegracao(dto.EmpresaId);
                if (empresaConfig == null)
                {
                    _logger.LogError("Configuração de integração da empresa com id {empresaId} não encontrada", dto.EmpresaId);
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                var config = System.Text.Json.JsonSerializer.Deserialize<EmpresaConfigIntegracaoDTO>(empresaConfig, options);
                if (config == null)
                {
                    _logger.LogError("Configuração OpenAI não encontrada para a empresa {EmpresaId}", dto.EmpresaId);
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                if (config.OpenAI == null)
                {
                    _logger.LogError("Configuração OpenAI não encontrada para a empresa {EmpresaId}", dto.EmpresaId);
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                var midia = await _midiaReaderService.GetMidiaByIdAsync(dto.MidiaId);
                if (midia == null)
                {
                    _logger.LogError("Mídia com id {MidiaId} não encontrada.", dto.MidiaId);
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                if (!string.IsNullOrWhiteSpace(midia.Transcricao))
                {
                    _logger.LogInformation("Transcrição já existente reutilizada para a mídia {MidiaId}.", dto.MidiaId);
                    return new TranscricaoResponseDTO { Transcricao = midia.Transcricao };
                }

                _openAIService.GetConfig(config.OpenAI);

                using var fileStream = await _blobStorageService.DownloadAsync(midia.BlobId, _connectionString, _containerName);
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var (bytesParaTranscricao, formatoParaTranscricao) = midia.Formato switch
                {
                    "audio/ogg" or "audio/aac" or "audio/mp4" => (await ConverterParaMp3(fileBytes), "audio/mpeg"),
                    _ => (fileBytes, midia.Formato)
                };

                var transcricao = await _openAIService.GenerateTranscricaoAsync(bytesParaTranscricao, formatoParaTranscricao, config.OpenAI);
                if (transcricao == null)
                {
                    _logger.LogError("Não foi possível transcrever o audio com id {id}", dto.MidiaId);
                    throw new AppException("Ocorreu um erro ao gerar a transcrição.");
                }

                await _midiaWriterService.SalvarTranscricaoAsync(dto.MidiaId, transcricao);

                TranscricaoResponseDTO transcricaoResponseDTO = new()
                {
                    Transcricao = transcricao
                };

                return transcricaoResponseDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar transcrição de áudio");
                throw;
            }
        }

        public static async Task<byte[]> ConverterParaMp3(byte[] inputBytes)
        {
            using var inputStream = new MemoryStream(inputBytes);
            using var outputStream = new MemoryStream();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToPipe(new StreamPipeSink(outputStream), options => options
                    .WithAudioCodec("libmp3lame")
                    .WithAudioBitrate(128)
                    .WithAudioSamplingRate(16000) // 16kHz é suficiente para voz e reduz tamanho
                    .WithCustomArgument("-ac 1") // Mono
                    .ForceFormat("mp3"))
                .ProcessAsynchronously();
            return outputStream.ToArray();
        }

    }
}
