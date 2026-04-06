using System.Text.Json;
using Azure;
using FluentValidation;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens.Experimental;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class MensagemWriterService(
        ILogger<MensagemWriterService> logger,
        IConversaReaderService conversaReaderService,
        IConversaWriterService conversaWriterService,
        IMensagemRepository mensagemRepository,
        ITemplateReaderService templateReaderService,
        IUnitOfWork unitOfWork,
        IUsuarioReaderService usuarioReaderService,
        ICanalReaderService canalReaderService,
        ILeadReaderService leadReader,
        IBusPublisherService busPublisher,
        IMensagemEnvioFilaFactory mensagemEnvioFactory,
        IMidiaWriterService midiaWriterService,
        IValidator<MensagemRequestDTO> createMensagemRequestValidator,
        IWhatsAppClient whatsAppClient, INotificacaoClient notificacaoClient, IUsuarioEmpresaReaderService usuarioEmpresaService, ITemplateWriterService templateWriterService, IMidiaReaderService midiaReaderService) : IMensagemWriterService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService ?? throw new ArgumentNullException(nameof(conversaWriterService));
        private readonly IMensagemRepository _mensagemRepository = mensagemRepository ?? throw new ArgumentNullException(nameof(mensagemRepository));
        private readonly ITemplateReaderService _templateReaderService = templateReaderService ?? throw new ArgumentNullException(nameof(templateReaderService));
        private readonly ILeadReaderService _leadReader = leadReader ?? throw new ArgumentNullException(nameof(leadReader));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly IBusPublisherService _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));
        private readonly IWhatsAppClient _whatsAppClient = whatsAppClient ?? throw new ArgumentNullException(nameof(whatsAppClient));
        private readonly INotificacaoClient _notificacaoClient = notificacaoClient ?? throw new ArgumentNullException(nameof(notificacaoClient));
        private readonly IMensagemEnvioFilaFactory _mensagemEnvioFactory = mensagemEnvioFactory ?? throw new ArgumentNullException(nameof(mensagemEnvioFactory));
        private readonly IMidiaWriterService _midiaWriterService = midiaWriterService ?? throw new ArgumentNullException(nameof(midiaWriterService));
        private readonly IValidator<MensagemRequestDTO> _createMensagemRequestValidator = createMensagemRequestValidator ?? throw new ArgumentNullException(nameof(createMensagemRequestValidator));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaService = usuarioEmpresaService ?? throw new ArgumentNullException(nameof(usuarioEmpresaService));
        private readonly ITemplateWriterService _templateWriterService = templateWriterService ?? throw new ArgumentNullException(nameof(templateWriterService));
        private readonly IMidiaReaderService _midiaReaderService = midiaReaderService ?? throw new ArgumentNullException(nameof(midiaReaderService));

        // TODO: Criar teste unitário para método ProcessarMensagemAsync, classe MensagemOrquestradorService.
        public async Task ProcessarMensagemAsync(MensagemRequestDTO dto)
        {
            var validationResult = await _createMensagemRequestValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                throw new AppException($"Dados inválidos para criação e envio de mensagem para a fila: {errors}");
            }

            try
            {
                var tipoMensagem = await _mensagemRepository.GetByPredicateAsync<MensagemTipo>(e => e.Codigo == dto.TipoMensagem)
                    ?? throw new AppException($"Tipo de mensagem não aceito. Tipo: {dto.TipoMensagem}.");

                var usuario = await _usuarioReaderService.ObterUsuarioPorIdAsync(dto.UsuarioId)
                    ?? throw new AppException($"Usuário com o id {dto.UsuarioId} não foi encontrado.");

                var lead = await _leadReader.GetLeadByIdAsync(dto.LeadId);

                if (string.IsNullOrEmpty(lead.WhatsappNumero))
                {
                    throw new AppException("Lead não pode iniciar conversa sem ter um número de WhatsApp");
                }

                var canal = await _usuarioEmpresaService.GetCanalPadraoByUsuarioEmpresaAsync(usuario.Id, lead.EmpresaId) ?? throw new AppException($"Não foi não possível encontrar o canal relacionado ao usuário com id {usuario.Id}.");

                if (!lead.EquipeId.HasValue)
                {
                    throw new AppException("Lead sem equipe atribuída. Não é possível criar conversa para o lead.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var conversaId = await _conversaWriterService.GetConversaByLeadAndCanalAsync(dto.LeadId, usuario.Id, canal.CanalPadraoId, "ATIVA", lead.EquipeId.Value, true);

                var mensagem = await CreateMessageObject(dto, conversaId, tipoMensagem);

                if (dto.Midia)
                {
                    var midiaValida = _midiaReaderService.ValidarArquivo(dto.File!);
                    if (!midiaValida.Valido)
                    {
                        throw new AppException(midiaValida.Erro);
                    }
                    var midia = await _midiaWriterService.ProcessarMidiaOutboundAsync(dto, mensagem);
                    await _conversaWriterService.UpdateDataUltimaMensagemAsync(conversaId, mensagem.DataCriacao);

                    var midiaEnvio = _mensagemEnvioFactory.CriarMidiaOutbound(midia.BlobId!, midia.MensagemId, usuario.Id, midia.Id, canal.CanalPadraoId);

                    await _busPublisher.PublishAsync(midiaEnvio);
                    await _unitOfWork.CommitAsync();

                    NotificarNovaMensagemDTO novaMidia = new()
                    {
                        MensagemId = mensagem.Id,
                        UsuarioId = usuario.Id,
                        Titulo = lead.Nome,
                        MensagemSincronizacao = new MensagemDTO
                        {
                            MensagemId = mensagem.Id,
                            Midia = true,
                            File = midia.UrlStorage,
                            MidiaId = midia.Id,
                            Template = false,
                            TipoMensagem = mensagem.Tipo.Codigo,
                            DataEnvio = mensagem.DataEnvio!.Value,
                            TipoRemetente = mensagem.Sentido,
                            LeadId = lead.Id,
                            UsuarioId = usuario.Id
                        }
                    };

                    await _notificacaoClient.NovaMensagem(novaMidia);

                    return;
                }

                if (dto.EhAviso == false)
                {
                    await _conversaWriterService.UpdateDataUltimaMensagemAsync(conversaId, mensagem.DataCriacao);
                }

                var mensagemEnvio = _mensagemEnvioFactory.CriarMensagemOutbound(mensagem, dto);

                await _unitOfWork.CommitAsync();

                await _busPublisher.PublishAsync(mensagemEnvio);

                NotificarNovaMensagemDTO novaMensagem = new()
                {
                    MensagemId = mensagem.Id,
                    UsuarioId = usuario.Id,
                    Titulo = lead.Nome,
                    MensagemSincronizacao = new MensagemDTO
                    {
                        MensagemId = mensagem.Id,
                        Conteudo = mensagem.Conteudo,
                        TipoMensagem = mensagem.Tipo.Codigo,
                        DataEnvio = mensagem.DataEnvio!.Value,
                        TipoRemetente = mensagem.Sentido,
                        LeadId = lead.Id,
                        UsuarioId = usuario.Id
                    }
                };

                var response = await _notificacaoClient.NovaMensagem(novaMensagem);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro de processamento ao salvar a mensagem do usuário no banco de dados. DTO: {dto}", dto);
                throw;
            }
        }

        public async Task<Mensagem> ProcessarStatusAsync(string messageMetaId, string statusMeta, long horaStatus, string? idConversaMeta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statusMeta))
                    throw new AppException("O Status da Meta não pode ser nulo.");

                var mensagem = await _mensagemRepository.GetByPredicateAsync<Mensagem>(
                                 w => w.IdExternoMeta == messageMetaId) ?? throw new AppException($"Mensagem com IdExternoMeta '{messageMetaId}' não encontrada.");

                if (!string.IsNullOrEmpty(idConversaMeta))
                {
                    var conversa = await _conversaReaderService.GetConversaByIdAsync(mensagem.ConversaId)
                    ?? throw new AppException($"Conversa com Id '{mensagem.ConversaId}' não encontrada.");

                    if (string.IsNullOrEmpty(conversa.IdExternoMeta))
                    {
                        await _conversaWriterService.UpdateConversaMetaIdAsync(conversa.Id, idConversaMeta);
                    }
                }

                statusMeta = statusMeta.ToUpperInvariant();
                var status = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(e => e.Codigo == statusMeta) ?? throw new AppException($"Status '{statusMeta}' não encontrado.");

                var dataAtual = TimeHelper.GetTimestampParaHorarioBrasilia(horaStatus);

                switch (status.Codigo.ToLowerInvariant())
                {
                    case "sent":
                        mensagem.AtualizarStatus(status.Id);
                        if (mensagem.DataEnvio == null)
                            mensagem.RegistrarEnvio(dataAtual);
                        break;

                    case "delivered":
                        mensagem.AtualizarStatus(status.Id);
                        if (mensagem.DataRecebimento == null)
                            mensagem.RegistrarRecebimento(dataAtual);
                        break;

                    case "read":
                        mensagem.AtualizarStatus(status.Id);
                        if (mensagem.DataLeitura == null)
                            mensagem.RegistrarLeitura(dataAtual);
                        break;

                    default:
                        var statusErro = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(e => e.Codigo == "FAILED") ?? throw new AppException("Erro ao encontrar código FAILED.");
                        mensagem.AtualizarStatus(statusErro.Id);
                        await _unitOfWork.CommitAsync();
                        _logger.LogError("Erro ao realizar o envio da mensagem do vendedor para o cliente. Id meta da mensagem: {id}. ", messageMetaId);
                        throw new AppException("Erro ao enviar a mensagem do Vendedor para o cliente.");
                }

                await _unitOfWork.SaveChangesAsync();
                return mensagem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar status '{statusMeta}' da mensagem '{messageMetaId}'.", statusMeta, messageMetaId);
                throw;
            }
        }

        public async Task<string> MarcarMensagemComoLidaAsync(int conversaId)
        {
            try
            {
                var conversa = await _conversaReaderService.GetConversaByIdAsync(conversaId);
                await _conversaWriterService.UpdatePossuiMensagensNaoLidas(conversaId, false);
                // Recupera o status "READ"
                var statusLida = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(e => e.Codigo == "READ") ?? throw new AppException("Erro ao encontrar código READ");

                // Atualiza o status de todas as mensagens do cliente 
                var mensagensAtualizadas = await UpdateStatusMensagensClienteAsync(conversaId, statusLida.Id);

                if (mensagensAtualizadas == 0)
                {
                    return "Não foi necessário atualizar nenhuma mensagem.";
                }

                // Busca apenas a última mensagem 
                var ultimaMensagem = await _mensagemRepository.GetUltimaMensagemByConversaIdAsync(conversaId) ?? throw new AppException($"Não foi possível encontrar mensagens na conversa com o Id: {conversaId}");

                var canal = await _canalReaderService.GetCanalByIdAsync(conversa.CanalId)
                            ?? throw new AppException($"Canal da conversa {conversaId} não encontrado.");

                if (string.IsNullOrWhiteSpace(canal.ConfiguracaoIntegracao))
                    throw new AppException($"Configuração de integração ausente no canal {canal.Id}.");

                var config = _canalReaderService.ObterConfiguracaoMeta(canal)
                    ?? throw new AppException($"Configuração inválida do canal {canal.Id}");

                // Chamada externa para API da Meta
                var responseMeta = await _whatsAppClient.MarcarMensagemComoLidaAsync(
                    ultimaMensagem.IdExternoMeta!,
                    config.WhatsAppAcessToken,
                    config.WhatsAppPhoneID
                );

                if (!responseMeta.IsSuccessStatusCode)
                {
                    var responseBody = await responseMeta.Content.ReadAsStringAsync();
                    var statusCode = (int)responseMeta.StatusCode;

                    _logger.LogError("Um erro ocorreu durante o uso da API Meta. StatusCode: {statusCode}. Detalhes: {detalhes}", statusCode, responseMeta.Content);
                    throw new AppException(
                       "Um erro ocorreu durante o uso da API Meta."
                    );
                }

                await _unitOfWork.CommitAsync();
                return $"{mensagensAtualizadas} mensagem(ns) foram(foi) atualizada(s).";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao marcar mensagens da conversa {conversaId} como lidas. Detalhes: {detalhes}", conversaId, ex.Message);
                throw;
            }
        }

        public async Task<Mensagem> CreateMessageObject(MensagemRequestDTO dto, int conversaId, MensagemTipo tipoMensagem)
        {
            try
            {

                if (dto.Midia)
                {
                    return await ProcessarMensagemEnvioMidiaAsync(
                        dto.Conteudo ?? string.Empty,
                        tipoMensagem.Codigo,
                        conversaId,
                        dto.UsuarioId
                    );
                }

                if (dto.Template)
                {
                    if (dto.TemplateId is null or <= 0)
                        throw new AppException("Uma mensagem de template não pode ter o template id menor ou igual a zero.");

                    return await ProcessarMensagemEnvioTemplateAsync(
                        tipoMensagem.Codigo,
                        conversaId,
                        dto.UsuarioId,
                        dto.TemplateId.Value,
                        null);
                }

                if (string.IsNullOrWhiteSpace(dto.Conteudo))
                    throw new AppException("Uma mensagem do tipo texto não pode estar com o conteúdo vazio.");

                return await ProcessarMensagemEnvioTextoAsync(
                    dto.Conteudo,
                    tipoMensagem.Codigo,
                    conversaId,
                    dto.UsuarioId,
                    dto.UsouAssistenteAi,
                    dto.EhAviso ?? false
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o objeto da mensagem para realizar o envio para a meta.");
                throw;
            }
        }

        public async Task<Mensagem> ProcessarMensagemMidiaAsync(string tipoMensagem, int conversaId, string messageMetaId, string midiaID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoMensagem))
                    throw new AppException("Tipo da mensagem não pode ser vazio.");

                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                if (string.IsNullOrWhiteSpace(messageMetaId))
                    throw new AppException("ID da mensagem Meta não pode ser vazio.");

                if (string.IsNullOrWhiteSpace(midiaID))
                    throw new AppException("ID da mídia não pode ser vazio.");


                var mensagemTipoId = await _mensagemRepository.GetMensagemTipo(tipoMensagem.ToUpper());
                var mensagemStatus = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(e => e.Codigo == "DELIVERED") ?? throw new AppException("Erro em buscar status da mensagem com o código: DELIVERED");

                var mensagemObj = Mensagem.CriarMensagemRecebida(conversaId, mensagemTipoId, messageMetaId, mensagemStatus.Id);

                var novaMensagem = await _mensagemRepository.CreateAsync<Mensagem>(mensagemObj);

                await _unitOfWork.SaveChangesAsync();

                return novaMensagem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar mensagem {IdExterno} do tipo {tipoMensagem}", messageMetaId, tipoMensagem);
                throw;
            }
        }

        public async Task<Mensagem> ProcessarMensagemTextoAsync(string conteudoMensagem, string tipoMensagem, int conversaId, string messageMetaId)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(conteudoMensagem))
                    throw new AppException("Corpo da mensagem de texto não pode ser nulo ou vazio.");

                if (string.IsNullOrWhiteSpace(tipoMensagem))
                    throw new AppException("Tipo da mensagem não pode ser vazio.");

                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                if (string.IsNullOrWhiteSpace(messageMetaId))
                    throw new AppException("ID da mensagem Meta não pode ser vazio.");

                var mensagemTipo = await _mensagemRepository.GetByPredicateAsync<MensagemTipo>(e => e.Codigo == tipoMensagem) ?? throw new AppException($"Erro em buscar tipo da mensagem com o código:{tipoMensagem}");
                var mensagemStatus = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(e => e.Codigo == "DELIVERED") ?? throw new AppException("Erro em buscar status da mensagem com o código: DELIVERED");

                var mensagemObj = Mensagem.CriarMensagemRecebida(conversaId, mensagemTipo.Id, messageMetaId, mensagemStatus.Id, conteudoMensagem);

                var novaMensagem = await _mensagemRepository.CreateAsync<Mensagem>(mensagemObj);

                await _unitOfWork.SaveChangesAsync();

                return novaMensagem;
            }
            catch (Exception ex)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogError(ex, "Erro inesperado ao registrar mensagem {IdExterno} do tipo {tipoMensagem}", messageMetaId, tipoMensagem);
                throw;
            }
        }

        public async Task<Mensagem> ProcessarMensagemEnvioTextoAsync(string conteudoMensagem, string tipoMensagem, int conversaId, int usuarioResponsavelId, bool usouAssistenteIA, bool ehAviso)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(conteudoMensagem) && tipoMensagem?.ToLowerInvariant() == "text")
                    throw new AppException("Corpo da mensagem de texto não pode ser nulo ou vazio.");

                if (string.IsNullOrWhiteSpace(tipoMensagem))
                    throw new AppException("Tipo da mensagem não pode ser vazio.");

                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                var mensagemStatusId = await _mensagemRepository.GetMensagemStatus("PENDENTE");

                if (mensagemStatusId <= 0)
                    throw new AppException("ID do status conversa deve ser maior que zero.");

                var mensagemTipoId = await _mensagemRepository.GetMensagemTipo(tipoMensagem.ToUpper());


                var mensagemObj = new Mensagem
                (
                    conversaId,
                    mensagemTipoId,
                    usuarioResponsavelId,
                    usouAssistenteIA: usouAssistenteIA,
                    ehAviso,
                    null,
                    conteudoMensagem ?? string.Empty,
                    null,
                    mensagemStatusId
                );

                var novaMensagem = await _mensagemRepository.CreateAsync<Mensagem>(mensagemObj);

                await _unitOfWork.SaveChangesAsync();

                return novaMensagem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar mensagem da conversa {conversa} do tipo {tipoMensagem}", conversaId, tipoMensagem);
                throw;
            }
        }
        public async Task<Mensagem> ProcessarMensagemEnvioMidiaAsync(string conteudoMensagem, string tipoMensagem, int conversaId, int usuarioResponsavelId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoMensagem))
                    throw new AppException("Tipo da mensagem não pode ser vazio.");

                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                var mensagemStatusId = await _mensagemRepository.GetMensagemStatus("PENDENTE");

                var mensagemTipoId = await _mensagemRepository.GetMensagemTipo(tipoMensagem.ToUpper());

                var mensagemObj = new Mensagem
                 (conversaId,
                   mensagemTipoId,
                   usuarioResponsavelId,
                   false,
                   false,
                   null,
                   conteudoMensagem,
                   null,
                   mensagemStatusId,
                   null
                 );

                var novaMensagem = await _mensagemRepository.CreateAsync<Mensagem>(mensagemObj);

                await _unitOfWork.SaveChangesAsync();

                return novaMensagem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar mensagem da conversa {conversa} do tipo {tipoMensagem}", conversaId, tipoMensagem);
                throw;
            }
        }

        public async Task<Mensagem> ProcessarMensagemEnvioTemplateAsync(string tipoMensagem, int conversaId, int usuarioResponsavelId, int templateId, string? idExternoMeta, List<string>? parametros = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoMensagem))
                    throw new AppException("Tipo da mensagem não pode ser vazio.");

                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                var template = await _templateReaderService.GetTemplateByIdAsync(templateId) ?? throw new AppException($"Template com ID {templateId} não encontrado.");

                // Pegamos o conteúdo original do template
                var conteudoTemplate = template.Conteudo;

                // Se houver parâmetros, usamos string.Format para preencher os {}
                if (parametros != null && parametros.Count > 0)
                {
                    try
                    {
                        conteudoTemplate = string.Format(conteudoTemplate, parametros.ToArray());
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError(ex, "Erro ao preencher parâmetros no template: {ConteudoTemplate} com {Parametros}", conteudoTemplate, parametros);
                        throw new AppException("Erro ao preencher parâmetros no template. Verifique se o número de parâmetros está correto.", ex);
                    }
                }

                var mensagemStatusId = await _mensagemRepository.GetMensagemStatus("PENDENTE");

                var mensagemTipoId = await _mensagemRepository.GetMensagemTipo(tipoMensagem.ToUpper());

                var mensagemObj = new Mensagem
                 (conversaId,
                   mensagemTipoId,
                   usuarioResponsavelId,
                   false,
                   false,
                   idExternoMeta ?? null,
                   conteudoTemplate,
                   null,
                   mensagemStatusId,
                   templateId
                 );

                var novaMensagem = await _mensagemRepository.CreateAsync<Mensagem>(mensagemObj);

                await _unitOfWork.SaveChangesAsync();

                return novaMensagem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar mensagem da conversa {conversaId} do tipo {tipoMensagem}", conversaId, tipoMensagem);
                throw;
            }
        }

        public async Task<Mensagem> ProcessarMensagemEnvioTemplateIntegracaoAsync(string tipoMensagem, int conversaId, int usuarioResponsavelId, Domain.Entities.Lead.Lead lead, Canal canal, int templateId, string? idExternoMeta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoMensagem))
                    throw new AppException("Tipo da mensagem não pode ser vazio.");

                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                var template = await _templateReaderService.GetTemplateByIdAsync(templateId) ?? throw new AppException($"Template com ID {templateId} não encontrado.");

                var primeiroNome = lead.Responsavel.Usuario.Nome?
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

                if (!string.IsNullOrEmpty(primeiroNome))
                {
                    primeiroNome = char.ToUpper(primeiroNome[0]) + primeiroNome[1..].ToLower();
                }

                List<TemplateParamIntegracao> parametros =
                [
                    new() { Tipo = "text", Parametro = primeiroNome }
                ];

                var templateEnvio = _templateWriterService.MontarJsonTemplateIntegracao(
                    template.Nome,
                    lead.WhatsappNumero,
                    parametros
                );

                var conteudo = _templateWriterService.MontarPreviewTemplate(template.Conteudo, parametros);
                var configuracaoCanal = canal.ConfiguracaoIntegracao
                    ?? throw new AppException($"Canal encontrado não possui configuração de integração: {canal.Id}");

                var config = JsonSerializer.Deserialize<CanalConfigDTO>(configuracaoCanal)
                    ?? throw new AppException("Não foi possível recuperar as configurações de integração do canal.");

                var mensagemStatusId = await _mensagemRepository.GetMensagemStatus("PENDENTE");

                var mensagemTipoId = await _mensagemRepository.GetMensagemTipo(tipoMensagem.ToUpper());

                var mensagemObj = new Mensagem
                 (conversaId,
                   mensagemTipoId,
                   usuarioResponsavelId,
                   false,
                   false,
                   idExternoMeta ?? null,
                   conteudo,
                   null,
                   mensagemStatusId,
                   templateId
                 );

                var novaMensagem = await _mensagemRepository.CreateAsync<Mensagem>(mensagemObj);

                await _unitOfWork.CommitAsync();

                var messageMetaId = await _templateWriterService.EnviarTemplateAsync(
                    template.Nome,
                    lead.WhatsappNumero,
                    config.WhatsAppAcessToken,
                    config.WhatsAppPhoneID,
                    templateEnvio
                );

                await UpdateIdMensagemMetaAsync(novaMensagem.Id, messageMetaId);
                return novaMensagem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar mensagem da conversa {conversaId} do tipo {tipoMensagem}", conversaId, tipoMensagem);
                throw;
            }
        }

        public async Task<int> UpdateStatusMensagensClienteAsync(int conversaId, int statusId)
        {
            try
            {
                return await _mensagemRepository.UpdateStatusMensagensClienteAsync(conversaId, statusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar mensagens pela conversa id. ConversaId: {ConversaId}", conversaId);
                throw;
            }
        }

        public async Task UpdateStatusMensagensAsync(int mensagemId, int statusId)
        {
            try
            {
                var mensagem = await _mensagemRepository.GetByIdAsync<Mensagem>(mensagemId);

                if (mensagem == null)
                {
                    _logger.LogWarning("Mensagem com ID {id} não encontrada para atualização de status.", mensagemId);
                    throw new AppException($"Mensagem com ID {mensagemId} não encontrada.");
                }

                mensagem.AtualizarStatus(statusId);

                _mensagemRepository.Update(mensagem);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar mensagem pelo id {id}.", mensagemId);
                throw;
            }
        }

        public async Task UpdateIdMensagemMetaAsync(int mensagemId, string idMeta)
        {
            try
            {
                var mensagem = await _mensagemRepository.GetByIdAsync<Mensagem>(mensagemId);

                if (mensagem == null)
                {
                    _logger.LogWarning("Mensagem com ID {id} não encontrada para atualização de meta id.", mensagemId);
                    throw new AppException($"Mensagem com ID {mensagemId} não encontrada.");
                }

                mensagem.AtualizarIdExternoMeta(idMeta);
                _logger.LogWarning("Atualizando mensagem ID {MensagemId} com ID externo Meta {IdMeta}", mensagemId, idMeta);
                _mensagemRepository.Update(mensagem);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado em atualizar o id da meta na mensagem com id {id}.", mensagemId);
                throw;
            }
        }
    }
}
