using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class MessageProcessingInboundService(ILogger<MessageProcessingInboundService> logger, IWebhookWriterService webhookWriterService, IMensagemWriterService mensagemWriterService, ICanalReaderService canalReaderService, IConversaWriterService conversaWriterService, ILeadResponsavelWriterService leadResponsavelWriterService, IUnitOfWork unitOfWork, INotificacaoClient notificacaoClient, IMidiaWriterService midiaWriterService, IChatBotWriterService chatBotWriterService, IEmpresaReaderService empresaReaderService, IChatBotClient chatBotClient, IMensagemReaderService mensagemReaderService, IMembroEquipeReaderService membroEquipeReaderService) : IMessageProcessingInboundService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILogger<MessageProcessingInboundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IWebhookWriterService _webhookWriterService = webhookWriterService ?? throw new ArgumentNullException(nameof(webhookWriterService));
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService ?? throw new ArgumentNullException(nameof(conversaWriterService));
        private readonly ILeadResponsavelWriterService _leadResponsavelWriterService = leadResponsavelWriterService ?? throw new ArgumentNullException(nameof(leadResponsavelWriterService));
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService ?? throw new ArgumentNullException(nameof(mensagemWriterService));
        private readonly INotificacaoClient _notificacaoClient = notificacaoClient ?? throw new ArgumentNullException(nameof(notificacaoClient));
        private readonly IMidiaWriterService _midiaWriterService = midiaWriterService ?? throw new ArgumentNullException(nameof(midiaWriterService));
        private readonly IChatBotWriterService _chatBotWriterService = chatBotWriterService ?? throw new ArgumentNullException(nameof(chatBotWriterService));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
        private readonly IChatBotClient _chatBotClient = chatBotClient ?? throw new ArgumentNullException(nameof(chatBotClient));
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly HashSet<string> MediaTypes =
        [
            "image", "video", "audio", "document", "sticker"
        ];

        // TODO: Criar teste unitário para método ProcessMessageCliente, classe MessageProcessingService.
        public async Task ProcessMessageCliente(ServiceBusReceivedMessage message)
        {
            await _unitOfWork.BeginTransactionAsync();

            var payloadJson = Encoding.UTF8.GetString(message.Body);

            try
            {
                var webhookRoot = await DesserializarMensagemWebhook(message);

                var canais = await ObterCanal(webhookRoot);

                if (canais.Count <= 0)
                {
                    return;
                }

                if (IsApenasStatus(webhookRoot))
                {
                    await ProcessarApenasStatus(webhookRoot);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                var (usuarioPhone, usuarioApelido) = ExtrairTelefoneUsuario(webhookRoot);
                var lead = await _leadResponsavelWriterService.VerificarOuCriarLeadComResponsavelAsync(usuarioPhone, canais, usuarioApelido);

                if (lead == null)
                {
                    _logger.LogError("Lead não encontrado para o número {Phone} no canal {CanalId}. Payload: {@WebhookRoot}", usuarioPhone, lead.CanalId, webhookRoot);
                    throw new InfraException("Lead não encontrado.");
                }

                var conversaId = await _conversaWriterService.GetConversaByLeadAndCanalAsync(lead.LeadId, lead.ResponsavelId, lead.CanalId, "ATIVA", lead.EquipeId, lead.LeadNovo);
                if (conversaId <= 0)
                {
                    _logger.LogError("Conversa inválida para LeadId: {LeadId}, CanalId: {CanalId}. Payload: {@WebhookRoot}",
                        lead.LeadId, lead.CanalId, webhookRoot);
                    throw new InfraException("Conversa inválida.");
                }

                var canalConfigJson = await _canalReaderService.GetCanalByIdAsync(lead.CanalId) ?? throw new InfraException("Canal não possui configuração de integração.");

                var configIntegracao = JsonSerializer.Deserialize<CanalConfigDTO>(canalConfigJson.ConfiguracaoIntegracao ?? throw new InfraException("Configuração de integração de um canal WhatsApp não pode ser nulo."))
                    ?? throw new InfraException("Erro ao desserializar configuração de integração.");

                var (mensagem, midia) = await ProcessarWebhook(webhookRoot.Entry, lead.ResponsavelId, conversaId, configIntegracao, lead.Nome);

                await _unitOfWork.CommitAsync();

                if (lead.IsBot)
                {
                    var botCache = await _chatBotWriterService.GetHistoryToBot(conversaId);
                    if (botCache == null)
                    {
                        var grupoEmpresa = await _empresaReaderService.GetGrupoEmpresaByEmpresaId(lead.EmpresaId);
                        List<MensagemDTO> listaMensagens = await _mensagemReaderService.GetMensagensRecentesSemAviso(conversaId, 0, 10);

                        var filiais = await _empresaReaderService.GetFiliasAsync(grupoEmpresa.Id);
                        var objBot = new CreateHistoryBotObjectDTO
                        {
                            ChatBotId = lead.ResponsavelId,
                            LeadId = lead.LeadId,
                            ConversaId = conversaId,
                            EmpresaId = lead.EmpresaId,
                            GrupoEmpresa = grupoEmpresa.Nome,
                            Mensagem = mensagem.Conteudo,
                            MensagensAntigas = listaMensagens,
                            Filiais = filiais
                        };
                        var respostaBot = await _chatBotWriterService.CreateHistoryToBot(objBot);
                    }
                    else
                    {
                        var novaMensagemCliente = new MessageRedisDTO() { Message = mensagem.Conteudo, Sender = "Customer", SenderIsBot = false, SentOn = TimeHelper.GetBrasiliaTime() };
                        await _chatBotWriterService.UpdateHistoryBot(novaMensagemCliente, conversaId);
                    }

                    var responseChatBot = new ChatMessageRequestDTO
                    {
                        Message = mensagem.Conteudo,
                        ConversationId = mensagem.ConversaId,
                    };

                    await _chatBotClient.SendToChatBot(responseChatBot, configIntegracao.UrlBaseChatBot ?? throw new InfraException("A url do chat bot não existe."));
                    return;
                }

                if (lead.LeadNovo)
                {
                    NotificarNovoLeadDTO novoLead = new()
                    {
                        LeadId = lead.LeadId,
                        UsuarioId = lead.ResponsavelId
                    };

                    await _notificacaoClient.NovoLead(novoLead);

                    var membroEquipe = await _membroEquipeReaderService.GetByIdAsync(lead.MembroId);
                    var lider = await _membroEquipeReaderService.ObterLiderDaEquipeAsync(membroEquipe!.EquipeId);

                    NotificarNovoLeadVendedorDTO notificarNovoLeadVendedorDTO = new()
                    {
                        LeadId = lead.LeadId,
                        UsuarioId = lider.UsuarioId,
                        NomeVendedor = lead.NomeResponsavel
                    };

                    await _notificacaoClient.NovoLeadVendedor(notificarNovoLeadVendedorDTO);
                }

                NotificarNovaMensagemDTO novaMensagem;
                if (mensagem.Midia == null)
                {
                    novaMensagem = new()
                    {
                        MensagemId = mensagem.Id,
                        UsuarioId = lead.ResponsavelId,
                        Titulo = lead.Nome,
                        MensagemSincronizacao = new MensagemDTO
                        {
                            MensagemId = mensagem.Id,
                            Template = mensagem.TemplateId != 0,
                            TemplateId = mensagem.TemplateId ?? null,
                            TipoMensagem = mensagem.Tipo.Codigo,
                            Conteudo = mensagem.Conteudo,
                            DataEnvio = mensagem.DataEnvio!.Value,
                            TipoRemetente = mensagem.Sentido,
                            LeadId = lead.LeadId,
                            UsuarioId = lead.ResponsavelId
                        }
                    };
                }
                else
                {
                    if (midia == null)
                    {
                        _logger.LogError("Midia não encontrada para a mensagem ID: {MensagemId}", mensagem.Id);
                        throw new InfraException("Midia não encontrada.");
                    }

                    novaMensagem = new()
                    {
                        MensagemId = mensagem.Id,
                        UsuarioId = lead.ResponsavelId,
                        Titulo = lead.Nome,
                        MensagemSincronizacao = new MensagemDTO
                        {
                            MensagemId = mensagem.Id,
                            Midia = true,
                            File = midia.UrlStorage,
                            MidiaId = midia.Id,
                            Conteudo = midia.Caption,
                            TipoMensagem = mensagem.Tipo.Codigo,
                            DataEnvio = mensagem.DataEnvio.Value,
                            TipoRemetente = mensagem.Sentido,
                            LeadId = lead.LeadId,
                            UsuarioId = lead.ResponsavelId
                        }
                    };
                }

                await _notificacaoClient.NovaMensagem(novaMensagem);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar mensagem do Azure Bus. MessageId: {MessageId}, Body: {Body}", message.MessageId, payloadJson);
                throw;
            }
        }

        private static bool IsApenasStatus(MetaWebhookRootDTO webhookRoot)
        {
            return webhookRoot.Entry
                .SelectMany(e => e.Changes)
                .All(c => c.Value?.Statuses != null &&
                          c.Value.Statuses.Count > 0 &&
                          (c.Value.Messages == null || c.Value.Messages.Count == 0));
        }

        private async Task ProcessarApenasStatus(MetaWebhookRootDTO webhookRoot)
        {
            foreach (var entry in webhookRoot.Entry)
            {
                if (entry == null || entry.Changes == null) continue;
                foreach (var change in entry.Changes)
                {
                    var changeValue = change.Value;
                    if (changeValue == null) continue;
                    if (changeValue.Statuses != null)
                    {
                        foreach (var status in changeValue.Statuses)
                        {
                            var mensagem = await ProcessarStatusComRetryAsync(status.Id, status.Status, long.Parse(status.Timestamp), status.Conversation?.Id);

                            if (mensagem == null)
                            {
                                _logger.LogWarning("Mensagem com wamid {Wamid} não encontrada após retries. Status {Status} ignorado.", status.Id, status.Status);
                                continue;
                            }

                            var codigoStatus = status.Status?.ToLowerInvariant() switch
                            {
                                "sent" => "STATUS_MENSAGEM_ENVIADA",
                                "delivered" => "STATUS_MENSAGEM_ENTREGUE",
                                "read" => "STATUS_MENSAGEM_LIDA",
                                _ => null
                            };

                            if (mensagem.UsuarioId.HasValue && !string.IsNullOrWhiteSpace(codigoStatus))
                            {
                                NotificarStatusMensagemAtualizadoDTO novoStatus = new()
                                {
                                    MensagemId = mensagem.Id,
                                    Status = codigoStatus,
                                    UsuarioId = mensagem.UsuarioId.Value
                                };

                                await _notificacaoClient.AtualizarMensagemStatus(novoStatus);
                            }
                        }
                    }
                }
            }
        }

        private async Task<Mensagem?> ProcessarStatusComRetryAsync(string wamid, string status, long timestamp, string? conversaMetaId)
        {
            const int maxTentativas = 5;
            var delay = TimeSpan.FromMilliseconds(300);

            for (int i = 0; i < maxTentativas; i++)
            {
                var mensagem = await _mensagemWriterService.ProcessarStatusAsync(wamid, status, timestamp, conversaMetaId);

                if (mensagem != null)
                    return mensagem;

                _logger.LogWarning("Tentativa {Tentativa}/{Max}: mensagem com wamid {Wamid} ainda não encontrada. Aguardando {Delay}ms...",
                    i + 1, maxTentativas, wamid, delay.TotalMilliseconds);

                await Task.Delay(delay);
                delay *= 2; // 300ms → 600ms → 1.2s → 2.4s → 4.8s
            }

            return null;
        }

        private async Task<MetaWebhookRootDTO> DesserializarMensagemWebhook(ServiceBusReceivedMessage message)
        {
            var corpo = Encoding.UTF8.GetString(message.Body);

            try
            {
                var payloadBody = JsonSerializer.Deserialize<WebhookMetaInboundDTO>(corpo, _jsonOptions);

                if (payloadBody == null)
                {
                    _logger.LogError("Payload inválido ao desserializar WebhookMetaDTO. Body recebido: {Body}", corpo);
                    throw new InfraException("Payload inválido.");
                }

                await _webhookWriterService.RegisterWebhookAsync(payloadBody);

                var webhookRoot = JsonSerializer.Deserialize<MetaWebhookRootDTO>(payloadBody.Payload!, _jsonOptions);

                if (webhookRoot == null)
                {
                    _logger.LogError("Webhook root inválido ao desserializar MetaWebhookRootDTO. Payload recebido: {Payload}", payloadBody.Payload);
                    throw new InfraException("Webhook root inválido.");
                }

                return webhookRoot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar ou deserializar mensagem recebida do Azure Bus. Body: {Body}", corpo);
                throw new InfraException("Erro ao deserializar payload.", ex);
            }
        }

        private async Task<List<CanalDTO>> ObterCanal(MetaWebhookRootDTO webhookRoot)
        {
            var phone = webhookRoot.Entry?
                .SelectMany(e => e.Changes)
                .FirstOrDefault()?.Value?.Metadata?.Display_Phone_Number;

            if (string.IsNullOrWhiteSpace(phone))
            {
                _logger.LogWarning("Número de telefone do canal não encontrado no payload: {@WebhookRoot}", webhookRoot);
                throw new InfraException("Número de telefone do canal ausente.");
            }

            var listaCanais = await _canalReaderService.GetListCanaisByWhatsAppNumber(phone);
            var canalInfo = listaCanais.Select(
                    e => new CanalDTO
                    {
                        CanalId = e.Id,
                        EmpresaId = e.EmpresaId
                    }
            ).ToList();
            return canalInfo;
        }

        private static (string usuarioPhone, string usuarioApelido) ExtrairTelefoneUsuario(MetaWebhookRootDTO webhookRoot)
        {
            var usuarioPhone = webhookRoot.Entry
               .SelectMany(e => e.Changes)
               .Select(c => c.Value?.Contacts?.FirstOrDefault()?.Wa_Id ?? c.Value?.Statuses?.FirstOrDefault()?.Recipient_Id)
               .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p))
               ?? throw new InfraException("Telefone do usuário não encontrado.");

            var usuarioApelido = webhookRoot.Entry
                .SelectMany(e => e.Changes)
                .Select(c => c.Value?.Contacts?.FirstOrDefault()?.Profile?.Name)
                .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p))
                ?? "Cliente WhatsApp";

            return ((usuarioPhone, usuarioApelido));
        }

        private async Task<(Mensagem obj, Midia? midiaObj)> ProcessarWebhook(IEnumerable<Entry> entries, int usuarioResponsavelId, int conversaId, CanalConfigDTO configuracaoIntegracao, string leadNome)
        {
            foreach (var entry in entries)
            {
                if (entry == null || entry.Changes == null) continue;

                foreach (var change in entry.Changes)
                {
                    var changeValue = change.Value;
                    if (changeValue == null) continue;

                    if (changeValue.Messages?.Count > 0)
                    {
                        foreach (var mensagemMeta in changeValue.Messages)
                        {
                            var (mensagem, midia) = await ProcessarMensagem(mensagemMeta, usuarioResponsavelId, conversaId, entry.Id, configuracaoIntegracao, leadNome);
                            return (mensagem, midia ?? null);
                        }
                    }
                }
            }

            _logger.LogError(
                "Webhook do WhatsApp não reconhecido. ConversaId: {ConversaId}, Lead: {LeadNome}",
                conversaId,
                leadNome
            );

            throw new InvalidOperationException(
                    "Webhook recebido não contém mensagens nem status para processar. " +
                    $"Payload: {JsonSerializer.Serialize(entries)}"
                );
        }

        private async Task<(Mensagem, Midia?)> ProcessarMensagem(WebhookMetaTypesDTO messageMeta, int usuarioResponsavelId, int conversaId, string conversaMetaId, CanalConfigDTO configuracaoIntegracao, string leadNome)
        {
            try
            {
                if (messageMeta is null)
                {
                    _logger.LogWarning("Mensagem recebida é nula.");
                    throw new InfraException("Mensagem recebida é nula.");
                }

                if (string.IsNullOrWhiteSpace(messageMeta.Type))
                {
                    _logger.LogWarning("Tipo da mensagem é nulo ou vazio.");
                    throw new InfraException("Tipo da mensagem é nulo ou vazio.");
                }

                if (string.IsNullOrWhiteSpace(messageMeta.Id))
                {
                    _logger.LogWarning("ID da mensagem é nulo ou vazio.");
                    throw new InfraException("ID da mensagem é nulo ou vazio.");
                }

                if (conversaId <= 0)
                {
                    _logger.LogWarning("conversaId inválido: {ConversaId}", conversaId);
                    throw new InfraException($"conversaId inválido: {conversaId}.");
                }

                switch (messageMeta.Type.ToLowerInvariant())
                {
                    case "text":

                        if (string.IsNullOrWhiteSpace(messageMeta.Text?.Body))
                        {
                            _logger.LogWarning("Mensagem de texto recebida sem conteúdo.");
                            throw new InfraException("Mensagem de texto não possui conteúdo.");
                        }

                        var mensagemTextoCriada = await _mensagemWriterService.ProcessarMensagemTextoAsync(messageMeta.Text.Body, messageMeta.Type, conversaId, messageMeta.Id);
                        await _conversaWriterService.UpdateInfoMensagemAsync(conversaId, mensagemTextoCriada.DataCriacao);
                        return (mensagemTextoCriada, null);

                    case var mediaType when MediaTypes.Contains(mediaType):

                        var mediaId = ExtractMediaId(messageMeta);
                        if (string.IsNullOrWhiteSpace(mediaId))
                        {
                            _logger.LogWarning("MediaId não encontrado para tipo de mídia: {MediaType}", mediaType);
                            throw new InfraException($"MediaId não encontrado para tipo de mídia: {mediaType}.");
                        }
                        
                        var mensagemMidiaCriada = await _mensagemWriterService.ProcessarMensagemMidiaAsync(mediaType, conversaId, messageMeta.Id, mediaId);
                        var dto = BuildMidiaProcessingDTO(messageMeta, usuarioResponsavelId, conversaMetaId, mensagemMidiaCriada.Id, leadNome, configuracaoIntegracao);
                        var midia = await _midiaWriterService.ProcessarMidiaInboundAsync(dto);
                        await _conversaWriterService.UpdateInfoMensagemAsync(conversaId, mensagemMidiaCriada.DataCriacao);
                        return (mensagemMidiaCriada, midia);

                    default:
                        var mensagemIncompativel = await _mensagemWriterService.ProcessarMensagemTextoAsync("Mensagem não suportada.", "UNKNOWN", conversaId, messageMeta.Id);
                        await _conversaWriterService.UpdateInfoMensagemAsync(conversaId, mensagemIncompativel.DataCriacao);
                        return (mensagemIncompativel, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem ID: {MessageId}, Tipo: {MessageType}",
                    messageMeta.Id, messageMeta.Type);
                throw;
            }
        }

        private static MidiaInboundDTO BuildMidiaProcessingDTO(
            WebhookMetaTypesDTO messageMeta,
            int usuarioResponsavelId,
            string conversaMetaId,
            int mensagemId,
            string leadNome,
            CanalConfigDTO configIntegracao)
        {
            return messageMeta.Type?.ToLowerInvariant() switch
            {
                "image" => new MidiaInboundDTO(
                    conversaMetaId,
                    usuarioResponsavelId,
                    leadNome,
                    messageMeta.Id,
                    mensagemId,
                    messageMeta.Image!.Id,
                    "image",
                    messageMeta.Image.Mime_Type,
                    messageMeta.Image.Caption,
                    "image",
                    null,
                    null,
                    configIntegracao
                ),

                "video" => new MidiaInboundDTO(
                    conversaMetaId,
                    usuarioResponsavelId,
                    leadNome,
                    messageMeta.Id,
                    mensagemId,
                    messageMeta.Video!.Id,
                    "video",
                    messageMeta.Video.Mime_Type,
                    messageMeta.Video.Caption,
                    "video",
                    null,
                    null,
                    configIntegracao
                ),

                "audio" => new MidiaInboundDTO(
                    conversaMetaId,
                    usuarioResponsavelId,
                    leadNome,
                    messageMeta.Id,
                    mensagemId,
                    messageMeta.Audio!.Id,
                    "audio",
                    messageMeta.Audio.Mime_Type.Split(';')[0].Trim(),
                    null,
                    "audio",
                    messageMeta.Audio.Voice,
                    null,
                    configIntegracao
                ),

                "document" => new MidiaInboundDTO(
                    conversaMetaId,
                    usuarioResponsavelId,
                    leadNome,
                    messageMeta.Id,
                    mensagemId,
                    messageMeta.Document!.Id,
                    "document",
                    messageMeta.Document.Mime_Type,
                    messageMeta.Document.Caption,
                    messageMeta.Document.Filename,
                    null,
                    null,
                    configIntegracao
                ),

                "sticker" => new MidiaInboundDTO(
                    conversaMetaId,
                    usuarioResponsavelId,
                    leadNome,
                    messageMeta.Id,
                    mensagemId,
                    messageMeta.Sticker!.Id,
                    "sticker",
                    messageMeta.Sticker.Mime_Type,
                    null,
                    "sticker",
                    null,
                    messageMeta.Sticker.Animated,
                    configIntegracao
                ),

                _ => throw new InfraException($"Tipo de mídia desconhecido: {messageMeta.Type}")
            };
        }

        private static string? ExtractMediaId(WebhookMetaTypesDTO message)
        {
            return message.Type?.ToLowerInvariant() switch
            {
                "image" => message.Image?.Id,
                "video" => message.Video?.Id,
                "audio" => message.Audio?.Id,
                "document" => message.Document?.Id,
                "sticker" => message.Sticker?.Id,
                _ => null
            };
        }
    }
}
