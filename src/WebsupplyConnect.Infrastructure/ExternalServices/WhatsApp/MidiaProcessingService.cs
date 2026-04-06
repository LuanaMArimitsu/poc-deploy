using Azure.Messaging.ServiceBus;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Services.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Infrastructure.Exceptions;
namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class MidiaProcessingService(
        ILogger<MidiaProcessingService> logger,
        ICanalReaderService canalReaderService,
        IMidiaReaderService midiaReaderService,
        IMidiaWriterService midiaWriterService,
        IMensagemReaderService mensagemReaderService,
        IMensagemWriterService mensagemWriterService,
        IMensagemEnvioFilaFactory mensagemEnvioFilaFactory,
        IWhatsAppMediaClient whatsAppMediaClient,
        IBusPublisherService busPublisherService,
        IOptions<AzureBlobStorageConfig> config,
        IBlobStorageService blobStorageService,
        IUnitOfWork unitOfWork) : IMidiaProcessingService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly IMidiaReaderService _midiaReaderService = midiaReaderService ?? throw new ArgumentNullException(nameof(midiaReaderService));
        private readonly IMidiaWriterService _midiaWriterService = midiaWriterService ?? throw new ArgumentNullException(nameof(midiaWriterService));
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService ?? throw new ArgumentNullException(nameof(mensagemWriterService));
        private readonly IMensagemEnvioFilaFactory _mensagemEnvioFilaFactory = mensagemEnvioFilaFactory ?? throw new ArgumentNullException(nameof(mensagemEnvioFilaFactory));
        private readonly IBusPublisherService _busPublisherService = busPublisherService ?? throw new ArgumentNullException(nameof(busPublisherService));
        private readonly string _containerName = config.Value.ContainerNameMidiasMeta ?? throw new ArgumentNullException(nameof(config));
        private readonly string _connectionString = config.Value.AzureBlobStorageConnectionString ?? throw new ArgumentNullException(nameof(config));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        private readonly IWhatsAppMediaClient _whatsAppMediaClient = whatsAppMediaClient ?? throw new ArgumentNullException(nameof(whatsAppMediaClient));

        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        // TODO: Criar teste unitário para método ProcessMidiaCliente, classe MidiaProcessingService.

        public async Task ProcessMidiaUsuario(ServiceBusReceivedMessage media)
        {
            await _unitOfWork.BeginTransactionAsync();
            var payload = DeserializarMidiaUsuario(media);
            try
            {
                var canal = await _canalReaderService.GetCanalByIdAsync(payload.CanalId)
                        ?? throw new AppException($"Canal com id {payload.CanalId} não foi encontrado.");

                var config = JsonSerializer.Deserialize<CanalConfigDTO>(
                    canal.ConfiguracaoIntegracao
                    ?? throw new InfraException($"Configuração de integração com WhatsApp não foram encontrados no canal com id {payload.CanalId}."))
                    ?? throw new AppException("Não foi possível recuperar as configurações de integração do canal.");

                using var fileStream = await _blobStorageService.DownloadAsync(payload.BlobId, _connectionString, _containerName);

                var midiaBanco = await _midiaReaderService.GetMidiaByIdAsync(payload.MidiaId);

                var mensagem = await _mensagemReaderService.GetMensagemByIdAsync(payload.MensagemId);

                var mensagemTipo = await _mensagemReaderService.GetMensagemTipoAsync(mensagem.TipoId)
                    ?? throw new InfraException("Tipo de mensagem não encontrado.");

                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var tipoMensagem = mensagemTipo.Codigo?.ToUpperInvariant();
                var midiaFormato = midiaBanco.Formato;
                _logger.LogWarning("Iniciando processamento da mídia. MidiaId: {MidiaId}, TipoMensagem: {TipoMensagem}, FormatoOriginal: {FormatoOriginal}", payload.MidiaId, tipoMensagem, midiaFormato);
                if (mensagemTipo.Codigo.Equals("AUDIO", StringComparison.OrdinalIgnoreCase) && midiaFormato == "audio/ogg")
                {
                    fileBytes = await ConverterAudioParaAAC(fileBytes);
                    midiaFormato = "audio/aac";
                }
                else if (mensagemTipo.Codigo.Equals("VIDEO", StringComparison.OrdinalIgnoreCase) && midiaFormato == "video/mp4" || midiaFormato == "video/quicktime")
                {
                    fileBytes = await ConverterVideo(fileBytes, midiaBanco.Formato);
                    midiaFormato = "video/mp4"; // Atualiza o formato para mp4 após a conversão
                }

                if (tipoMensagem == "VIDEO" || tipoMensagem == "AUDIO")
                {
                    const long LIMITE_META_BYTES = 100L * 1024 * 1024;

                    if (fileBytes.LongLength > LIMITE_META_BYTES)
                    {
                        _logger.LogWarning(
                            "Mídia excede limite da Meta após processamento. MidiaId: {MidiaId}, Tipo: {Tipo}, Tamanho: {Tamanho}",
                            payload.MidiaId,
                            tipoMensagem,
                            fileBytes.LongLength);

                        throw new InfraException("Mídia excede limite permitido pela Meta.");
                    }
                }

                var midiaMetaId = await EnviarMidiaParaMeta(fileBytes, midiaBanco.Nome, midiaFormato, config, mensagemTipo.Codigo.ToLowerInvariant());
                _logger.LogWarning("Midia id gerada com sucesso.{id}", midiaMetaId);
                var midiaInfo = await ObterInformacoesMidia(midiaMetaId, config.WhatsAppAcessToken);
                _logger.LogWarning("Informações da mídia obtidas com sucesso. MidiaId: {MidiaId}, Tamanho: {Tamanho}, Formato: {Formato}", payload.MidiaId, midiaInfo.File_size, midiaInfo.Mime_type);

                var statusProcessado = await _midiaReaderService.GetMidiaStatusProcessamentoAsync("PROCESSADO")
                    ?? throw new InfraException("Código PROCESSADO não foi encontrado.");

                await _midiaWriterService.UpdateStatusProcessamentoMidiaAsync(midiaBanco.Id, statusProcessado.Id);
                await _midiaWriterService.UpdateMidiaDadosMetaAsync(midiaBanco.Id, midiaInfo.File_size, midiaMetaId);

                var mensagemEnvio = _mensagemEnvioFilaFactory.CriarMensagemOutbound(mensagem, null);

                await _busPublisherService.PublishAsync(mensagemEnvio);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar mídia {midiaID}.", payload.MidiaId);

                if (payload != null)
                    await AtualizarStatusComErro(payload.MensagemId, payload.MidiaId);

                throw;
            }
        }

        private async Task AtualizarStatusComErro(int mensagemId, int midiaId)
        {
            try
            {
                var mensagemBanco = await _mensagemReaderService.GetMensagemByIdAsync(mensagemId);
                var statusErroMsg = await _mensagemReaderService.GetMensagemStatusByCodigo("FAILED");
                await _mensagemWriterService.UpdateStatusMensagensAsync(mensagemId, statusErroMsg.Id);
                mensagemBanco.AtualizarStatus(statusErroMsg.Id);

                var midiaBanco = await _midiaReaderService.GetMidiaByIdAsync(midiaId);
                var statusErroMidia = await _midiaReaderService.GetMidiaStatusProcessamentoAsync("ERRO_PROCESSAMENTO");
                await _midiaWriterService.UpdateStatusProcessamentoMidiaAsync(midiaId, statusErroMidia.Id);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogCritical(fallbackEx, "Falha ao atualizar status de erro da mídia {midiaID}.", midiaId);
            }
        }

        public static async Task<byte[]> ConverterAudioParaAAC(byte[] inputBytes)
        {
            using var inputStream = new MemoryStream(inputBytes);
            using var outputStream = new MemoryStream();

            var inputPipe = new StreamPipeSource(inputStream);
            var outputPipe = new StreamPipeSink(outputStream);

            await FFMpegArguments
                 .FromPipeInput(inputPipe)
                 .OutputToPipe(outputPipe, options => options
                     .WithAudioCodec("aac")
                     .WithAudioBitrate(128) // 128 kbps - boa qualidade para voz
                     .WithAudioSamplingRate(44100) // 44.1 kHz - padrão iOS
                     .WithCustomArgument("-ac 1") // Mono - reduz tamanho para áudio de voz
                     .ForceFormat("adts")) // Container MP4/M4A
                 .ProcessAsynchronously();

            return outputStream.ToArray();
        }

        public async static Task<byte[]> ConverterVideo(byte[] videoBytes, string inputExtension)
        {
            var sanitizedExtension = inputExtension
            .Replace("video/", "")
            .Replace("/", "")
            .Replace("\\", "");


            var tempFolder = Path.Combine(Path.GetTempPath(), "midia-whatsapp");
            Directory.CreateDirectory(tempFolder); // garante que a pasta exista

            var inputPath = Path.Combine(tempFolder, $"{Guid.NewGuid()}.{sanitizedExtension}");
            var outputPath = Path.Combine(tempFolder, $"{Guid.NewGuid()}.mp4");

            try
            {
                await File.WriteAllBytesAsync(inputPath, videoBytes);

                await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(outputPath, overwrite: true, options => options
                    .WithVideoCodec("libx264")
                    .WithAudioCodec("aac")
                    .WithAudioBitrate(96000)
                    .WithCustomArgument("-preset veryfast")
                    .WithCustomArgument("-crf 28")
                    .WithCustomArgument("-vf \"scale='min(1280,iw)':-2\"")
                    .WithCustomArgument("-maxrate 1500k")
                    .WithCustomArgument("-bufsize 3000k")
                    .ForceFormat("mp4"))
                .ProcessAsynchronously();

                return await File.ReadAllBytesAsync(outputPath);
            }
            finally
            {
                if (File.Exists(inputPath)) File.Delete(inputPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        private static MidiaOutboundDTO DeserializarMidiaUsuario(ServiceBusReceivedMessage message)
        {
            var corpo = Encoding.UTF8.GetString(message.Body);
            return JsonSerializer.Deserialize<MidiaOutboundDTO>(corpo, _jsonOptions)
                ?? throw new JsonException("Payload da mídia do usuário é inválido ou está ausente.");
        }

        private async Task<string> EnviarMidiaParaMeta(byte[] fileBytes, string fileName, string contentType, CanalConfigDTO config, string tipoMensagem)
        {
            return await _whatsAppMediaClient.EnviarMidiaParaMetaAsync(
                fileBytes,
                contentType,
                fileName,
                config.WhatsAppPhoneID,
                config.WhatsAppAcessToken,
                tipoMensagem
            ) ?? throw new AppException("O midiaMetaId não pode ser nulo.");
        }

        private async Task<MidiaMetaDTO> ObterInformacoesMidia(string mediaMetaId, string whatsAppAcessToken)
        {
            return await _whatsAppMediaClient.GetMediaInfoAsync(mediaMetaId, whatsAppAcessToken)
                ?? throw new InfraException("As informações da mídia retornaram nulas.");
        }

    }
}
