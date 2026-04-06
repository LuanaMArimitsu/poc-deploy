using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class MidiaWriterService(
        ILogger<MidiaWriterService> logger,
        IBlobStorageService blobStorageService,
        IMidiaRepository midiaRepository,
        IUnitOfWork unitOfWork,
        IOptions<AzureBlobStorageConfig> config, IWhatsAppMediaClient whatsAppMediaClient) : IMidiaWriterService
    {
        private readonly ILogger<MidiaWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        private readonly IMidiaRepository _midiaRepository = midiaRepository ?? throw new ArgumentNullException(nameof(midiaRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly string _connectionString = config?.Value.AzureBlobStorageConnectionString ?? throw new ArgumentNullException(nameof(config));
        private readonly string _containerName = config.Value.ContainerNameMidiasMeta ?? throw new ArgumentNullException(nameof(config));
        private readonly IWhatsAppMediaClient _whatsAppMediaClient = whatsAppMediaClient ?? throw new ArgumentNullException(nameof(whatsAppMediaClient));

        //TODO: Criar teste unitário para método ProcessarMidiaAsync, classe MidiaOrquestradorService.
        public async Task<Midia> ProcessarMidiaOutboundAsync(MensagemRequestDTO dto, Mensagem mensagem)
        {
            try
            {
                ValidarMidia(dto);

                var tipo = mensagem.Tipo.Codigo.ToLowerInvariant();
                var nomeMidia = ObterNomeMidia(tipo, dto.File?.FileName) ?? throw new AppException("Erro ao obter nome da midia do usuário.");

                using var stream = dto.File!.OpenReadStream() ?? throw new AppException("Erro ao transformar o File em Stream.");
                string contentType = dto.File.ContentType ?? throw new AppException("Erro ao obter o tipo de File.");
                var midia = await CriarMidiaAsync(mensagem.Id, mensagem.Conteudo, nomeMidia, contentType) ?? throw new AppException("Erro ao criar midia.");
                var midiaBanco = await CreateMidiaAsync(midia);
                var blobUrl = await SalvarNoBlobAsync(stream, midia);

                string? thumbnail = "";
                if (tipo == "image")
                {
                    thumbnail = await GerarThumbnailSeForImagem(tipo, stream, midia);
                }
                await UpdateMidiaStorageAsync(midiaBanco.Id, blobUrl, thumbnail ?? string.Empty);

                return midiaBanco;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Midia> ProcessarMidiaInboundAsync(MidiaInboundDTO dto)
        {
            try
            {
                var midiaInfo = await ObterInformacoesMidia(dto.MediaId, dto.MetaConfig.WhatsAppAcessToken);
                var midia = await CriarMidiaAsync(dto.MensagemId, dto.Caption ?? string.Empty, dto.FileName, dto.MimeType ?? dto.MediaType, midiaInfo.File_size, midiaInfo.Id);
                var midiaBanco = await CreateMidiaAsync(midia)
                    ?? throw new AppException("Erro ao persistir a mídia no banco de dados.");

                var stream = await BaixarStreamMidia(midiaInfo, dto.MetaConfig.WhatsAppAcessToken);
                var blobUrl = await SalvarNoBlobAsync(stream, midia);

                string? thumbnail = "";
                if (dto.MediaType == "image")
                {
                    thumbnail = await GerarThumbnailSeForImagem(dto.MediaType, stream, midia);

                }
                await UpdateMidiaStorageAsync(midiaBanco.Id, blobUrl, thumbnail ?? string.Empty);

                var statusProcessado = await _midiaRepository.GetMidiaStatusProcessamentoAsync("PROCESSADO")
                    ?? throw new AppException("Código PROCESSADO não foi encontrado.");

                await UpdateStatusProcessamentoMidiaAsync(midiaBanco.Id, statusProcessado.Id);

                return midiaBanco;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task UpdateMidiaStorageAsync(int midiaId, string urlStorage, string? thumbnail)
        {
            try
            {
                var midia = await _midiaRepository.GetByIdAsync<Midia>(midiaId) ?? throw new AppException($"Mídia a ser atualizada não foi encontrada: {midiaId}");

                midia.AtualizarUrlsStorage(urlStorage, thumbnail ?? string.Empty);
                _midiaRepository.Update(midia);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Erro inesperado ao realizar update, Midia:{midiaId}", midiaId);
                throw new AppException("Error updating midia", ex);
            }
        }

        public async Task UpdateStatusProcessamentoMidiaAsync(int midiaId, int novoStatusId)
        {
            try
            {
                var midia = await _midiaRepository.GetByIdAsync<Midia>(midiaId);

                if (midia == null)
                {
                    _logger.LogWarning("Mídia com ID {MidiaId} não encontrada para atualização de status.", midiaId);
                    throw new AppException($"Mídia com ID {midiaId} não encontrada.");
                }

                midia.AtualizarStatusProcessamento(novoStatusId);

                _midiaRepository.Update(midia);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar o status de processamento da mídia ID: {MidiaId}", midiaId);
                throw new AppException("Erro ao atualizar o status de processamento da mídia.", ex);
            }
        }

        public async Task UpdateMidiaDadosMetaAsync(int midiaId, long size, string mediaIdMeta)
        {
            try
            {
                var midia = await _midiaRepository.GetByIdAsync<Midia>(midiaId);

                if (midia == null)
                {
                    _logger.LogWarning("Mídia com ID {MidiaId} não encontrada para atualização de status.", midiaId);
                    throw new AppException($"Mídia com ID {midiaId} não encontrada.");
                }

                midia.AtualizarMidiaDadosMeta(size, mediaIdMeta);

                _midiaRepository.Update(midia);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar o status de processamento da mídia ID: {MidiaId}", midiaId);
                throw new AppException("Erro ao atualizar o status de processamento da mídia.", ex);
            }
        }

        public async Task<Midia> CreateMidiaAsync(Midia midia)
        {
            try
            {
                if (midia == null)
                {
                    throw new AppException("Objeto mídia não pode ser nulo para ser criada.");
                }
                ;

                var midiaBanco = await _midiaRepository.CreateAsync(midia);
                await _unitOfWork.SaveChangesAsync();
                return midiaBanco;
            }
            catch (Exception ex)
            {
                throw new AppException($"Erro ao criar a mídia no banco. Erro: {ex}");
            }
        }

        private async Task<MidiaMetaDTO> ObterInformacoesMidia(string mediaMetaId, string whatsAppAcessToken)
        {
            return await _whatsAppMediaClient.GetMediaInfoAsync(mediaMetaId, whatsAppAcessToken)
                ?? throw new AppException("As informações da mídia retornaram nulas.");
        }
        private async Task<Stream> BaixarStreamMidia(MidiaMetaDTO midiaInfo, string whatsAppAcessToken)
        {
            return await _whatsAppMediaClient.DownloadMediaAsync(midiaInfo, whatsAppAcessToken)
                ?? throw new AppException("Erro ao fazer download da mídia.");
        }
        private static void ValidarMidia(MensagemRequestDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                throw new AppException("Para mensagem do tipo mídia, o File não pode ser nulo.");
        }

        private static string ObterNomeMidia(string tipo, string? fileName)
        {
            return tipo switch
            {
                "document" => fileName ?? "documento",
                "audio" or "image" or "video" => tipo,
                _ => "arquivo"
            };
        }

        private async Task<Midia> CriarMidiaAsync(int mensagemId, string conteudo, string filename, string formatoMidia, long? tamanhoBytes = null, string? idMeta = null)
        {
            var status = await _midiaRepository.GetMidiaStatusProcessamentoAsync("PROCESSANDO")
                ?? throw new AppException("Código PROCESSANDO não foi encontrado.");

            return new Midia(
                nome: filename,
                blobId: Guid.NewGuid().ToString(),
                containerName: _containerName,
                midiaStatusProcessamentoId: status.Id,
                mensagemId: mensagemId,
                formato: formatoMidia,
                caption: conteudo ?? string.Empty,
                tamanhoBytes: tamanhoBytes,
                urlStorage: string.Empty,
                thumbnailUrlStorage: string.Empty,
                idExternoMeta: idMeta
            );
        }

        private async Task<string> SalvarNoBlobAsync(Stream mediaStream, Midia midia)
        {
            var blobUrl = await _blobStorageService.UploadAsync(
                mediaStream,
                midia.BlobId,
                midia.Formato,
                _connectionString,
                _containerName
            );

            if (string.IsNullOrWhiteSpace(blobUrl))
                throw new AppException("Erro ao salvar a mídia no Blob Storage.");

            return blobUrl;
        }

        private async Task<string?> GerarThumbnailSeForImagem(string tipo, Stream stream, Midia midia)
        {
            stream.Position = 0;
            return await _blobStorageService.CreateThumbnailAsync(
                stream,
                midia.BlobId,
                _connectionString,
                _containerName
            );
        }


        public async Task SalvarTranscricaoAsync(int midiaId, string transcricao)
        {
            try
            {
                var midia = await _midiaRepository.GetByIdAsync<Midia>(midiaId);

                if (midia == null)
                {
                    _logger.LogWarning("Mídia com ID {MidiaId} não encontrada para salvar transcrição.", midiaId);
                    throw new AppException($"Mídia com ID {midiaId} não encontrada.");
                }

                midia.RegistrarTranscricao(transcricao);

                _midiaRepository.Update(midia);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar transcrição da mídia ID: {MidiaId}", midiaId);
                throw new AppException("Erro ao salvar transcrição da mídia.", ex);
            }
        }
    }
}
