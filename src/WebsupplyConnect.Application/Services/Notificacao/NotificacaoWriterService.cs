using FluentValidation;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using System.Globalization;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Notificacao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Notificacao;

namespace WebsupplyConnect.Application.Services.Notificacao
{
    public class NotificacaoWriterService(IUnitOfWork unitOfWork, ILogger<NotificacaoWriterService> logger, INotificacaoRepository notificacaoRepository, ILeadReaderService leadReaderService, IDispositivosReaderService dispositivoReaderService, IUsuarioReaderService usuarioReaderService, IMensagemReaderService mensagemReaderService, INotificacaoDispatcher hubContext, IPushNotificationService pushNotification, IValidator<NotificarNovoLeadDTO> novoLeadValidator, IValidator<NotificarNovaMensagemDTO> novaMensagemValidator, IValidator<NotificarStatusMensagemAtualizadoDTO> statusMensagemValidator, IValidator<NotificarNovoLeadVendedorDTO> novoLeadVendedorValidator, IRedisCacheService redisCacheService, IValidator<NotificacaoEscalonamentoDTO> escalonamentoValidator) : INotificacaoWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILogger<NotificacaoWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INotificacaoRepository _notificacaoRepository = notificacaoRepository ?? throw new ArgumentNullException(nameof(notificacaoRepository));
        private readonly IDispositivosReaderService _dispositivoReaderService = dispositivoReaderService ?? throw new ArgumentNullException(nameof(dispositivoReaderService));
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
        private readonly ILeadReaderService _leadReaderService = leadReaderService ?? throw new ArgumentNullException(nameof(leadReaderService));
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly INotificacaoDispatcher _dispatcher = hubContext;
        private readonly IPushNotificationService _pushNotification = pushNotification ?? throw new ArgumentNullException(nameof(pushNotification));
        private readonly IRedisCacheService _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        // Validadores 
        private readonly IValidator<NotificarNovoLeadDTO> _novoLeadValidator = novoLeadValidator;
        private readonly IValidator<NotificarNovaMensagemDTO> _novaMensagemValidator = novaMensagemValidator;
        private readonly IValidator<NotificarStatusMensagemAtualizadoDTO> _atualizarStatusValidator = statusMensagemValidator;
        private readonly IValidator<NotificarNovoLeadVendedorDTO> _novoLeadVendedorValidator = novoLeadVendedorValidator;
        private readonly IValidator<NotificacaoEscalonamentoDTO> _escalonamentoValidator = escalonamentoValidator;

        public async Task NovoLead(NotificarNovoLeadDTO dto)
        {
            try
            {
                var validationResult = await _novoLeadValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de novo lead: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var leadvalido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadvalido)
                {
                    throw new AppException("Lead não encontrado");
                }

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "LEAD_ATRIBUIDO", false) ?? throw new AppException("NotificacaoTipo igual a NOVA_MENSAGEM não encontrada") ?? throw new AppException("NotificacaoTipo igual a NOVA_MENSAGEM não encontrada");
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada") ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Novo Lead",
                    Content = "Novo lead atribuido a você",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.LeadId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, true);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de novo lead para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }

        public async Task NovoLeadVendedor(NotificarNovoLeadVendedorDTO dto)
        {
            try
            {
                var validationResult = await _novoLeadVendedorValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de novo lead: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var leadvalido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadvalido)
                {
                    throw new AppException("Lead não encontrado");
                }

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "LEAD_ATRIBUIDO", false) ?? throw new AppException("NotificacaoTipo igual a LEAD_ATRIBUIDO não encontrada") ?? throw new AppException("NotificacaoTipo igual a LEAD_ATRIBUIDO não encontrada");
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada") ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Novo Lead",
                    Content = $"Novo lead atribuido a {dto.NomeVendedor}",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.LeadId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, true);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de novo lead para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }

        public async Task LeadAtualizado(NotificarNovoLeadDTO dto)
        {
            try
            {
                var validationResult = await _novoLeadValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de novo lead: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var leadvalido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadvalido)
                {
                    throw new AppException("Lead não encontrado");
                }

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "LEAD_ATUALIZADO", false) ?? throw new AppException("NotificacaoTipo igual a LEAD_ATUALIZADO não encontrada");
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Lead Alterado",
                    Content = "Seu lead foi alterado",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.LeadId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, true);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de lead atualizado para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }

        public async Task NovaMensagem(NotificarNovaMensagemDTO dto)
        {
            try
            {
                var validationResult = await _novaMensagemValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de nova mensagem: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var mensagem = await _mensagemReaderService.GetMensagemByIdAsync(dto.MensagemId) ?? throw new AppException("Mensagem não encontrada");

                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");
                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "NOVA_MENSAGEM", false) ?? throw new AppException("NotificacaoTipo igual a NOVA_MENSAGEM não encontrada");

                string alvo = "mensagem";
                bool enviada = mensagem.Sentido == 'E';
                bool enviarPush = !enviada; // só envia push se não for enviada pelo vendedor

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Title = dto.Titulo,
                    Type = alvo,
                    File = dto.MensagemSincronizacao.File,
                    MensagemID = mensagem.Id,
                    ConversaID = mensagem.ConversaId,
                    Content = "Nova Mensagem",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone,
                    MensagemSincronizacao = dto.MensagemSincronizacao
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.MensagemId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, enviarPush);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de nova mensagem para o usuário {UsuarioId} e mensagem {MensagemId}", dto.UsuarioId, dto.MensagemId);
                throw;
            }
        }

        public async Task MensagemAtualizarStatus(NotificarStatusMensagemAtualizadoDTO dto)
        {
            try
            {
                var validationResult = await _atualizarStatusValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para atualização de status: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }
                var mensagem = await _mensagemReaderService.GetMensagemByIdAsync(dto.MensagemId) ?? throw new AppException("Mensagem não encontrada");

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == dto.Status, false) ?? throw new AppException($"NotificacaoTipo igual a {dto.Status} não encontrada");
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "mensagem";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = $"{dto.Status}",
                    Content = notificacaoTipo.Descricao,
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone,
                    MensagemID = mensagem.Id,
                    ConversaID = mensagem.ConversaId
                };


                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.MensagemId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, false);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status de mensagem {MensagemId} para {Status} do usuário {UsuarioId}", dto.MensagemId, dto.Status, dto.UsuarioId);
                throw;
            }
        }

        public async Task EscalonamentoAutomaticoLider(NotificacaoEscalonamentoDTO dto)
        {
            try
            {
                var validationResult = await _escalonamentoValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de novo lead: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var leadvalido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadvalido)
                {
                    throw new AppException("Lead não encontrado");
                }

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "TRANSFERENCIA_AUTO_LIDER", false) ?? throw new AppException("NotificacaoTipo igual a TRANSFERENCIA_AUTO_LIDER não encontrada");
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Lead recebido",
                    Content = "Um lead foi transferido à você devido a inatividade do vendedor.",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.LeadId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, true);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de lead atualizado para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }

        public async Task EscalonamentoAutomaticoVendedor(NotificacaoEscalonamentoDTO dto)
        {
            try
            {
                var validationResult = await _escalonamentoValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de novo lead: {errors}");
                }

                var uservalido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var leadvalido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadvalido)
                {
                    throw new AppException("Lead não encontrado");
                }

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "TRANSFERENCIA_AUTOMATICA_AVISO", false) ?? throw new AppException("NotificacaoTipo igual a TRANSFERENCIA_AUTOMATICA_AVISO não encontrada");
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Lead transferido",
                    Content = "Seu lead foi transferido para o líder da equipe.",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(notificacaoDto.Title, notificacaoDto.Content, dto.UsuarioId, notificacaoTipo.Id, notificacaoStatus.Id, null, null, dto.LeadId, alvo);
                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarPushSignalR(notificacao, notificacaoDto, true);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de lead atualizado para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }


        private async Task EnviarPushSignalR(Domain.Entities.Notificacao.Notificacao notificacao, NotificacaoDTO dto, bool enviarPush)
        {
            try
            {
                bool enviadaSignalR = await EnviarSignalR(notificacao, dto);
                bool enviadaPush = false;

                if (enviarPush)
                {
                    enviadaPush = await EnviarPush(notificacao, dto);
                }

                // Só marca como "Enviada" se pelo menos um dos canais foi enviado com sucesso
                if (enviadaSignalR || enviadaPush)
                {
                    await AtualizarStatus(notificacao.Id, "Enviada", null);
                }
                else
                {
                    await AtualizarStatus(notificacao.Id, "Falha", null);
                    _logger.LogWarning("Falha ao enviar notificação {NotificacaoId} para usuário {UserId} via todos os canais.", notificacao.Id, notificacao.UsuarioDestinatarioId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação {NotificacaoId} para usuário {UserId}", notificacao.Id, notificacao.UsuarioDestinatarioId);
                throw;
            }
        }

        private async Task<bool> EnviarSignalR(Domain.Entities.Notificacao.Notificacao notificacao, NotificacaoDTO dto)
        {
            notificacao.MarcarSignalR(false);

            try
            {
                await _dispatcher.EnviarNotificacaoAsync(dto);
                notificacao.MarcarSignalR(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao enviar notificação {NotificacaoId} via SignalR para usuário {UserId}", notificacao.Id, notificacao.UsuarioDestinatarioId);
                return false;
            }
        }

        private async Task<bool> EnviarPush(Domain.Entities.Notificacao.Notificacao notificacao, NotificacaoDTO dto)
        {
            notificacao.MarcarPush(false);

            try
            {
                var dispositivos = await _dispositivoReaderService.GetDispositivosByUserAsync(notificacao.UsuarioDestinatarioId);

                if (!dispositivos.Any())
                {
                    _logger.LogInformation("Nenhum dispositivo encontrado para push do usuário {UserId}.", notificacao.UsuarioDestinatarioId);
                    return false;
                }

                var tasks = dispositivos.Select(d => _pushNotification.SendToDeviceAsync(d.DeviceId, dto));

                var resultados = await Task.WhenAll(
                    dispositivos.Select(async d =>
                    {
                        try
                        {
                            await _pushNotification.SendToDeviceAsync(d.DeviceId, dto);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Erro ao enviar Push para dispositivo {DeviceId} do usuário {UserId}", d.DeviceId, notificacao.UsuarioDestinatarioId);
                            return false;
                        }
                    })
                );

                bool peloMenosUmSucesso = resultados.Any(r => r);
                notificacao.MarcarPush(peloMenosUmSucesso);
                return peloMenosUmSucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral ao enviar Push para usuário {UserId}", notificacao.UsuarioDestinatarioId);
                return false;
            }
        }

        private async Task AtualizarStatus(int notificacaoId, string status, DateTime? dataVisualizada)
        {
            try
            {
                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == status, false);
                var notificacao = await _notificacaoRepository.GetByIdAsync<Domain.Entities.Notificacao.Notificacao>(notificacaoId);

                if (notificacao != null && notificacaoStatus != null)
                {
                    notificacao.AtualizarStatus(notificacaoStatus.Id);

                    if (dataVisualizada != null && dataVisualizada.Value != DateTime.MinValue)
                    {
                        notificacao.MarcarComoVisualizada(dataVisualizada.Value);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao atualizar status '{Status}' da notificação {NotificacaoId}", status, notificacaoId);
            }
        }

        public async Task ExcluirNotificacaoAsync(int notificacaoId, int usuarioId)
        {
            try
            {
                if (notificacaoId <= 0 || usuarioId <= 0)
                    throw new AppException("IDs devem ser maiores que zero.");

                var uservalido = await _usuarioReaderService.UserExistsAsync(usuarioId);
                if (!uservalido)
                    throw new AppException("Usuário não encontrado");

                var notificacao = await _notificacaoRepository.GetByIdAsync<Domain.Entities.Notificacao.Notificacao>(notificacaoId, includeDeleted: true);
                if (notificacao == null)
                    throw new AppException("Notificação não encontrada.");

                if (notificacao.UsuarioDestinatarioId != usuarioId)
                    throw new AppException("Notificação não pertence ao usuário.");

                if (notificacao.Excluido)
                    return;

                notificacao.ExcluirLogicamente();
                _notificacaoRepository.Update(notificacao);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao excluir notificação {NotificacaoId} do usuário {UsuarioId}.", notificacaoId, usuarioId);
                throw;
            }
        }

        public async Task<NotificacaoLimpezaResultadoDTO> ExcluirTodasEMarcarComoLidasAsync(int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new AppException("ID do usuário deve ser maior que zero.");

                var uservalido = await _usuarioReaderService.UserExistsAsync(usuarioId);
                if (!uservalido)
                    throw new AppException("Usuário não encontrado");

                var notificacoes = await _notificacaoRepository.GetNotificacoesAtivasPorDestinatarioAsync(usuarioId);
                if (notificacoes == null || notificacoes.Count == 0)
                    return new NotificacaoLimpezaResultadoDTO();

                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "VISUALIZADA", false);
                if (notificacaoStatus == null)
                    throw new AppException("Status 'VISUALIZADA' não encontrado.");

                var marcadasComoLidas = 0;
                foreach (var notificacao in notificacoes)
                {
                    if (notificacao.StatusId != notificacaoStatus.Id)
                    {
                        notificacao.AtualizarStatus(notificacaoStatus.Id);
                        notificacao.MarcarComoVisualizada(TimeHelper.GetBrasiliaTime());
                        marcadasComoLidas++;
                    }

                    notificacao.ExcluirLogicamente();
                    _notificacaoRepository.Update(notificacao);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new NotificacaoLimpezaResultadoDTO
                {
                    Excluidas = notificacoes.Count,
                    MarcadasComoLidas = marcadasComoLidas
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao excluir todas e marcar como lidas para o usuário {UsuarioId}.", usuarioId);
                throw;
            }
        }

        public async Task MarcarTodasComoLidasAsync(int usuarioId)
        {
            try
            {
                var uservalido = await _usuarioReaderService.UserExistsAsync(usuarioId);
                if (!uservalido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var notificacoes = await _notificacaoRepository.GetNotificacoesByUserAsync(usuarioId);
                if (notificacoes == null || notificacoes.Count == 0)
                    throw new AppException("Nenhuma notificação encontrada para o usuário.");

                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "VISUALIZADA", false);
                if (notificacaoStatus == null)
                    throw new AppException("Status 'VISUALIZADA' não encontrado.");

                foreach (var notificacao in notificacoes)
                {
                    if (notificacao.StatusId != notificacaoStatus.Id)
                    {
                        notificacao.AtualizarStatus(notificacaoStatus.Id);
                        notificacao.MarcarComoVisualizada(TimeHelper.GetBrasiliaTime());
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, $"Erro ao marcar todas as notificações como lidas do usuario com ID {usuarioId}.");
                throw;
            }
        }

        public async Task Visualizada(int notificacaoId, DateTime date)
        {
            try
            {
                await AtualizarStatus(notificacaoId, "Visualizada", date);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao atualizar a notificacao ID {NotificacaoId} para visualizada", notificacaoId);
            }
        }

        public async Task LeadExcluido(NotificarNovoLeadDTO dto)
        {
            try
            {
                var validationResult = await _novoLeadValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de exclusão de lead: {errors}");
                }

                var usuarioValido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!usuarioValido)
                {
                    throw new AppException("Usuário não encontrado");
                }

                var leadValido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadValido)
                {
                    throw new AppException("Lead não encontrado");
                }

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(
                    n => n.Codigo == "LEAD_EXCLUIDO", false
                ) ?? throw new AppException("NotificacaoTipo igual a LEAD_EXCLUIDO não encontrada");

                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(
                    n => n.Codigo == "CRIADA", false
                ) ?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Lead Excluído",
                    Content = "O lead foi excluído",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(
                    notificacaoDto.Title,
                    notificacaoDto.Content,
                    dto.UsuarioId,
                    notificacaoTipo.Id,
                    notificacaoStatus.Id,
                    null,
                    null,
                    dto.LeadId,
                    alvo
                );

                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();

                await EnviarSignalR(notificacao, notificacaoDto);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar notificação de lead excluído para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }

        public async Task<List<NotificacaoProcessadaDTO>> ProcessarNotificacoesCriadasAsync(int usuarioId)
        {
            var processadas = new List<NotificacaoProcessadaDTO>();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var statusCriada = await _notificacaoRepository
                    .GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false);

                if (statusCriada == null)
                    return processadas;

                var notificacoes = await _notificacaoRepository.GetNotificacoesByUserAsync(usuarioId);

                notificacoes = notificacoes
                    .Where(n => n.StatusId == statusCriada.Id)
                    .ToList();

                if (!notificacoes.Any())
                    return processadas;

                var agora = TimeHelper.GetBrasiliaTime();
                var diaSemana = agora.ToString("dddd", new CultureInfo("pt-BR"));
                var cacheKey = $"usuario:{usuarioId}:horarios";

                var horarios = await _redisCacheService.GetAsync<List<UsuarioHorarioDTO>>(cacheKey);
                if (horarios == null)
                {
                    horarios = await _usuarioReaderService.ObterHorariosUsuarioAsync(usuarioId);
                    if (horarios != null)
                    {
                        await _redisCacheService.SetAsync(cacheKey, horarios, TimeSpan.FromDays(1));
                    }
                }

                var horarioHoje = horarios?.FirstOrDefault(h =>
                    string.Equals(h.DiaSemanaDescricao, diaSemana, StringComparison.OrdinalIgnoreCase));

                if (horarioHoje == null || horarioHoje.SemExpediente == true || !horarioHoje.HorarioFim.HasValue)
                    return processadas;

                var fimExpediente = agora.Date.Add(horarioHoje.HorarioFim.Value);

                foreach (var notificacao in notificacoes)
                {
                    if (notificacao.DataHora <= fimExpediente)
                        continue;

                    var dto = new NotificacaoDTO
                    {
                        Id = notificacao.UsuarioDestinatarioId,
                        Title = notificacao.Titulo,
                        Content = notificacao.Conteudo,
                        Type = notificacao.TipoEntidadeAlvo,
                        Color = notificacao.NotificacaoTipo?.Cor,
                        Icon = notificacao.NotificacaoTipo?.Icone
                    };

                    switch (notificacao.NotificacaoTipo.Codigo.ToUpper())
                    {
                        case "LEAD_ATUALIZADO":
                            await EnviarPushSignalR(notificacao, dto, true);
                            break;

                        case "LEAD_ATRIBUIDO":
                            await EnviarPushSignalR(notificacao, dto, true);
                            break;

                        case "NOVA_MENSAGEM":
                            await EnviarPush(notificacao, dto);
                            break;

                        default:
                            continue;
                    }

                    processadas.Add(new NotificacaoProcessadaDTO
                    {
                        Id = notificacao.Id,
                        Tipo = notificacao.NotificacaoTipo.Codigo,
                        DataCriacao = notificacao.DataHora
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return processadas;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao processar notificações pendentes para o usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task NovoLeadEvento(NotificarNovoLeadDTO dto)
        {
            try
            {
                var validationResult = await _novoLeadValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para notificação de evento de lead: {errors}");
                }

                var usuarioValido = await _usuarioReaderService.UserExistsAsync(dto.UsuarioId);
                if (!usuarioValido)
                    throw new AppException("Usuário não encontrado");

                var leadValido = await _leadReaderService.LeadExistsAsync(dto.LeadId);
                if (!leadValido)
                    throw new AppException("Lead não encontrado");

                var notificacaoTipo = await _notificacaoRepository.GetByPredicateAsync<NotificacaoTipo>(n => n.Codigo == "LEAD_EVENTO", false
                    )?? throw new AppException("NotificacaoTipo igual a LEAD_EVENTO não encontrada");

                var notificacaoStatus = await _notificacaoRepository.GetByPredicateAsync<NotificacaoStatus>(n => n.Codigo == "CRIADA", false
                    )?? throw new AppException("NotificacaoStatus igual a CRIADA não encontrada");

                string alvo = "lead";

                var notificacaoDto = new NotificacaoDTO
                {
                    Id = dto.UsuarioId,
                    Type = alvo,
                    Title = "Novo evento no Lead",
                    Content = "Seu lead recebeu um novo evento.",
                    Color = notificacaoTipo.Cor,
                    Icon = notificacaoTipo.Icone
                };

                var notificacao = new Domain.Entities.Notificacao.Notificacao(
                    notificacaoDto.Title,
                    notificacaoDto.Content,
                    dto.UsuarioId,
                    notificacaoTipo.Id,
                    notificacaoStatus.Id,
                    null,
                    null,
                    dto.LeadId,
                    alvo
                );

                await _notificacaoRepository.CreateAsync(notificacao);
                await _unitOfWork.SaveChangesAsync();
                await EnviarPushSignalR(notificacao, notificacaoDto, true);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento de novo lead para o usuário {UsuarioId} e lead {LeadId}", dto.UsuarioId, dto.LeadId);
                throw;
            }
        }
    }
}
