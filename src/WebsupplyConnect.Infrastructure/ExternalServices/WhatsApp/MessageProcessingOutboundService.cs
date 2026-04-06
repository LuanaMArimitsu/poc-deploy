using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class MessageProcessingOutboundService(ILogger<MessageProcessingOutboundService> logger, ICanalReaderService canalReaderService, ILeadReaderService leadReader, IMensagemReaderService mensagemReaderService, IMensagemWriterService mensagemWriterService,IConversaReaderService conversaReaderService, IWhatsAppClient whatsAppClient, ITemplateReaderService templateReaderService,ITemplateWriterService templateWriterService, IMidiaReaderService midiaReaderService, IUnitOfWork unitOfWork) : IMessageProcessingOutboundService
    {
        private readonly ILogger<MessageProcessingOutboundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly ILeadReaderService _leadReader = leadReader ?? throw new ArgumentNullException(nameof(leadReader));
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService ?? throw new ArgumentNullException(nameof(mensagemWriterService));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IWhatsAppClient _whatsAppClient = whatsAppClient ?? throw new ArgumentNullException(nameof(whatsAppClient));
        private readonly ITemplateReaderService _templateReaderService = templateReaderService ?? throw new ArgumentNullException(nameof(templateReaderService));
        private readonly ITemplateWriterService _templateWriterService = templateWriterService ?? throw new ArgumentNullException(nameof(templateWriterService));
        private readonly IMidiaReaderService _midiaReaderService = midiaReaderService ?? throw new ArgumentNullException(nameof(midiaReaderService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private static readonly HashSet<string> MediaTypes =
        [
           "image", "video", "audio", "document", "sticker"
        ];

        // TODO: Criar teste unitário para método ProcessMessageEnvio, classe MessageProcessingEnvioService.
        public async Task ProcessMessageEnvio(ServiceBusReceivedMessage message)
        {
            await _unitOfWork.BeginTransactionAsync();
            var payload = ObterPayload(message);
            try
            {
                var (lead, canal, config) = await ObterContextoEnvioAsync(payload);
                var messageMetaId = await EnviarMensagemAsync(payload, lead, config);
                _logger.LogWarning("Mensagem enviada com sucesso. MessageId: {MessageId}, Payload: {PayloadJson}, MetaMessageId: {MetaMessageId}", message.MessageId, payload, messageMetaId);
                await FinalizarEnvioAsync(payload.Id, messageMetaId);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar envio da mensagem. MessageId: {MessageId}, Payload: {PayloadJson}", message.MessageId, payload);

                if (payload != null)
                    await AtualizarStatusComErro(payload.Id);
                throw;
            }
        }

        private async Task AtualizarStatusComErro(int mensagemId)
        {
            try
            {
                var statusErro = await _mensagemReaderService.GetMensagemStatusByCodigo("FAILED");
                await _mensagemWriterService.UpdateStatusMensagensAsync(mensagemId, statusErro.Id);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogCritical(fallbackEx, "Falha ao atualizar status de erro da mensagem {mensagemId}.", mensagemId);
            }
        }
        private static MensagemOutboundDTO ObterPayload(ServiceBusReceivedMessage message)
        {
            var messageBody = message.Body.ToString();
            return JsonSerializer.Deserialize<MensagemOutboundDTO>(messageBody, _jsonOptions)
                ?? throw new InfraException("Não foi possível desserializar o payload de envio.");
        }

        private async Task<(Lead lead, Canal canal, CanalConfigDTO config)> ObterContextoEnvioAsync(MensagemOutboundDTO payload)
        {
            var conversa = await _conversaReaderService.GetConversaByIdAsync(payload.ConversaId);
            var canal = await _canalReaderService.GetCanalByIdAsync(conversa.CanalId)
                ?? throw new InfraException("Não foi possível encontrar o canal de envio.");

            var lead = await _leadReader.GetLeadByIdAsync(conversa.LeadId)
                ?? throw new InfraException("Não foi possível encontrar o lead alvo.");

            var configuracaoCanal = canal.ConfiguracaoIntegracao
                ?? throw new InfraException($"Canal encontrado não possui configuração de integração: {canal.Id}");

            var config = JsonSerializer.Deserialize<CanalConfigDTO>(configuracaoCanal)
                ?? throw new InfraException("Não foi possível recuperar as configurações de integração do canal.");

            return (lead, canal, config);
        }

        private async Task<string> EnviarMensagemAsync(MensagemOutboundDTO payload, Lead lead, CanalConfigDTO config)
        {
            var response = await SendMessage(payload, lead.WhatsappNumero ?? throw new InfraException("Número do lead não pode ser vazio."), config);

            if (!response.IsSuccessStatusCode)
                throw new InfraException("Erro ao enviar mensagem ou mídia para a API Meta.");

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseMeta = JsonSerializer.Deserialize<ResponseEnvioMetaDTO>(responseContent, _jsonOptions)
                ?? throw new InfraException("Erro ao deserializar retorno da API Meta.");

            return responseMeta.messages?.FirstOrDefault()?.id
                ?? throw new InfraException("ID da mensagem Meta não encontrado no retorno.");
        }

        private async Task FinalizarEnvioAsync(int mensagemId, string messageMetaId)
        {
            await _mensagemWriterService.UpdateIdMensagemMetaAsync(mensagemId, messageMetaId);
        }

        private async Task<HttpResponseMessage> SendMessage(MensagemOutboundDTO mensagemEnvio, string leadWhatsApp, CanalConfigDTO config)
        {
            HttpResponseMessage response;
            if (mensagemEnvio.TemplateId.HasValue && mensagemEnvio.TemplateId > 0)
            {
                var template = await _templateReaderService.GetTemplateByIdAsync(mensagemEnvio.TemplateId.Value);
                var templateObject = _templateWriterService.MontarJsonTemplateMeta(template.Nome, leadWhatsApp);

                response = await _whatsAppClient.EnviarTemplateMontadoAsync(templateObject, config.WhatsAppAcessToken, config.WhatsAppPhoneID);

                return response;
            }

            var tipoMensagem = await _mensagemReaderService.GetMensagemTipoAsync(mensagemEnvio.TipoId);

            switch (tipoMensagem.Codigo.ToLowerInvariant())
            {
                case var mediaType when MediaTypes.Contains(mediaType):
                    if (!mensagemEnvio.MidiaId.HasValue || mensagemEnvio.MidiaId <= 0)
                    {
                        throw new InfraException("Midia id não pode ser nulo ou menor que zero.");
                    }
                    var midiaMensagem = await _midiaReaderService.GetMidiaByIdAsync(mensagemEnvio.MidiaId.Value);
                    response = await _whatsAppClient.EnviarMidiaPorIdAsync(leadWhatsApp, mediaType, midiaMensagem.IdExternoMeta ?? throw new InfraException("Id Meta da midia não pode ser nulo."), config.WhatsAppAcessToken, config.WhatsAppPhoneID, midiaMensagem.Nome, midiaMensagem.Caption);
                    break;

                case "text":
                    response = await _whatsAppClient.EnviarMensagemTextoAsync(leadWhatsApp, mensagemEnvio.Conteudo, config.WhatsAppAcessToken, config.WhatsAppPhoneID);
                    break;

                // Para mensagem que não processamos, somente ignoramos. 
                default:
                    _logger.LogError("O tipo da mensagem que deseja ser enviada não é suportado. Tipo: {tipo}.", tipoMensagem);
                    throw new InfraException("Tipo de mensagem não é suportado.");
            }

            return response;
        }
    }
}
