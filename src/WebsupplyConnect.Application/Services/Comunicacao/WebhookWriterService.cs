using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class WebhookWriterService(IUnitOfWork unitOfWork, IBusPublisherService busPublisher, IWebhookMetaRepository webhookRepository, ILogger<WebhookWriterService> logger) : IWebhookWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IWebhookMetaRepository _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
        private readonly ILogger<WebhookWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IBusPublisherService _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));

        /// <summary>
        /// Registra um webhook da Meta
        /// </summary>
        /// <param name="dto">Dados do webhookDTO a serem registrado</param>
        /// <returns>ID do webhook registrado ou existente</returns>
        public async Task<int> RegisterWebhookAsync(WebhookMetaInboundDTO dto)
        {
            try
            {
                var existente = await _webhookRepository.GetWebhookMetaByIdExternoAsync(dto.IdExterno);
                if (existente != null)
                {
                    //ja existe
                    return existente.Id;
                }

                var webhook = new WebhookMeta(dto.IdExterno, dto.Payload, dto.AssinaturaHMAC);

                var webhookCriada = await _webhookRepository.CreateAsync(webhook);

                await _unitOfWork.SaveChangesAsync();
                //await _unitOfWork.CommitAsync();

                return webhookCriada.Id;
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackAsync();

                _logger.LogError(ex, "Erro inesperado ao registrar webhook {IdExterno}", dto.IdExterno);
                // Log the exception
                throw new AppException("Error registering webhook", ex);
            }

        }

        public async Task<bool> UpdateWebhookAsync(int webhookID, int conversaID)
        {
            try
            {
                var webhook = await _webhookRepository.GetWebhookMetaByIdAsync(webhookID);
                if (webhook != null)
                {
                    webhook.MarcarProcessado(conversaID);

                    _webhookRepository.UpdateWebhookMeta(webhook);

                    await _unitOfWork.SaveChangesAsync();
                    //await _unitOfWork.CommitAsync();

                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackAsync();

                _logger.LogError(ex, "Erro inesperado ao realizar update, webhook:{webhookID}", webhookID);
                // Log the exception
                throw new AppException("Error registering webhook", ex);
            }
        }

        public async Task ProcessWebhookAsync(string payload, string signature)
        {
            try
            {
                var dto = new WebhookMetaInboundDTO(
                    Guid.NewGuid().ToString(),
                    payload,
                    signature
                );
                await _busPublisher.PublishAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook: {payload}", payload);
                throw new AppException("Error ao processar webhook", ex);
            }
        }
    }
}
