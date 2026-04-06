using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class ConversaWriterService(ILogger<ConversaWriterService> logger, IValidator<ConversaStatusDTO> validatorStatus, IMensagemReaderService mensagemReaderService, ILeadReaderService leadReader, IMidiaReaderService midiaReaderService, IConversaRepository conversaRepository, IUnitOfWork unitOfWork, IRedisCacheService redisCacheService, IUsuarioEmpresaReaderService usuarioEmpresaReaderService, IMembroEquipeReaderService membroEquipeReaderService, ILeadEventoWriterService leadEventoWriterService) : IConversaWriterService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly IValidator<ConversaStatusDTO> _validatorStatus = validatorStatus ?? throw new ArgumentNullException(nameof(validatorStatus));
        private readonly ILeadReaderService _leadReader = leadReader ?? throw new ArgumentNullException(nameof(leadReader));
        private readonly IMidiaReaderService _midiaReaderService = midiaReaderService ?? throw new ArgumentNullException(nameof(midiaReaderService));
        private readonly IConversaRepository _conversaRepository = conversaRepository ?? throw new ArgumentNullException(nameof(conversaRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IRedisCacheService _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService ?? throw new ArgumentNullException(nameof(usuarioEmpresaReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly ILeadEventoWriterService _leadEventoWriterService = leadEventoWriterService ?? throw new ArgumentNullException(nameof(leadEventoWriterService));

        public async Task<List<ConversaListaDTO>> ConversasSyncAsync(int usuarioId, int usuarioLogadoId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new AppException("ID do usuário deve ser maior que zero.");

                var statusEncerrado = await _conversaRepository.GetByPredicateAsync<ConversaStatus>(e => e.Codigo == "ENCERRADA") ?? throw new AppException("Status com código 'ENCERRADA' não foi encontrado");
                var statusEntregue = await _mensagemReaderService.GetMensagensStatusAsync(null, "DELIVERED");

                // Busca todas as conversas do usuário
                List<Conversa> conversas = await _conversaRepository.GetConversasByUsuarioAsync(usuarioId, statusEncerrado.Id);

                List<ConversaListaDTO> resultado = [];
                var dataAtual = TimeHelper.GetBrasiliaTime();

                foreach (var conversa in conversas)
                {

                    // Verifica se está dentro da janela de 24 horas
                    var janelaAberta = conversa.DataUltimaMensagem.HasValue &&
                                      (dataAtual - conversa.DataUltimaMensagem.Value).TotalHours <= 24;

                    // Conta mensagens não lidas
                    int qtdMensagensNaoLidas = 0;
                    if (conversa.PossuiMensagensNaoLidas)
                    {
                        var mensagensNaoLidas = await _mensagemReaderService.GetMensagensNaoLidasByConversaAsync(conversa.Id, statusEntregue.Id);
                        qtdMensagensNaoLidas = mensagensNaoLidas?.Count ?? 0;
                    }

                    // Busca a última mensagem
                    var ultimaMensagem = await _mensagemReaderService.GetUltimaMensagemByConversaAsync(conversa.Id) ?? throw new AppException("Última mensagem não pode ser nula.");

                    string caption = string.Empty;

                    var tipoUltimaMensagem = await _mensagemReaderService.GetMensagemTipoAsync(ultimaMensagem.TipoId) ?? throw new AppException("Tipo da mensagem não pode ser nulo.");

                    if (tipoUltimaMensagem.Codigo != "TEXT" && tipoUltimaMensagem.Codigo != "LOCATION" && tipoUltimaMensagem.Codigo != "INTERACTIVE" && tipoUltimaMensagem.Codigo != "UNKNOWN" && tipoUltimaMensagem.Codigo != "CONTACTS")
                    {
                        var midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(ultimaMensagem.Id);
                        caption = midia?.Caption ?? string.Empty;
                    }

                    // Busca dados do lead
                    var lead = await _leadReader.GetLeadByIdAsync(conversa.LeadId);
                    if (lead == null)
                    {
                        _logger.LogWarning("Lead com ID {LeadId} não encontrado para conversa {ConversaId}",
                            conversa.LeadId, conversa.Id);
                        continue;
                    }

                    // Busca o código do status do lead
                    var leadStatus = await _leadReader.GetLeadStatusCodigoAsync(lead.LeadStatusId) ?? throw new AppException("O status do lead não pode ser nulo.");

                    // Status da conversa
                    var conversaStatus = await _conversaRepository.GetByIdAsync<ConversaStatus>(conversa.StatusId) ?? throw new AppException($"Status da conversa com id {conversa.StatusId} não foi encontrado.");

                    var primiraMensagemCliente = await _conversaRepository.GetPrimeiraMensagemClienteAsync(conversa.Id);
                    string iniciouContato;

                    if (primiraMensagemCliente.UsuarioId != null)
                    {
                        iniciouContato = "Vendedor";
                    }
                    else
                    {
                        iniciouContato = "Cliente";
                    }

                    var numeroWhatsapp = lead.WhatsappNumero;
                    if (lead.Equipe.ResponsavelMembro.UsuarioId != usuarioLogadoId)
                    {
                        numeroWhatsapp = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                    }

                    // Monta o resultado
                    var conversaSincronizada = new ConversaListaDTO
                    {
                        ConversaId = conversa.Id,
                        NumeroWhatsapp = numeroWhatsapp,
                        ConversaStatus = conversaStatus.Nome,
                        LeadName = lead.Nome,
                        Apelido = lead.Apelido,
                        LeadId = lead.Id,
                        LeadStatus = leadStatus,
                        LeadEmpresaId = lead.EmpresaId,
                        JanelaAberta = janelaAberta,
                        Tipo = tipoUltimaMensagem.Codigo,
                        UltimaMensagem = ultimaMensagem?.Conteudo ?? caption ?? tipoUltimaMensagem.Codigo,
                        DataUltimaMensagem = conversa.DataUltimaMensagem ?? DateTime.MinValue,
                        QtdMensagensNaoLidas = qtdMensagensNaoLidas,
                        DataInicioConversa = conversa.DataInicio,
                        IniciouContato = iniciouContato,
                        UsuarioId = conversa.UsuarioId,
                        Fixada = conversa.Fixada
                    };

                    resultado.Add(conversaSincronizada);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar conversa para usuário {UsuarioId}", usuarioId);
                throw new AppException("Erro interno ao sincronizar conversas", ex);
            }
        }
        public async Task<ConversaPag> GetConversasListaPaginadaAsync(int usuarioId, int usuarioLogadoId, ConversaPagParam param)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new AppException("ID do usuário deve ser maior que zero.");

                if ((param.quantidadeInicial.HasValue && !param.quantidadeFinal.HasValue) || (!param.quantidadeInicial.HasValue && param.quantidadeFinal.HasValue))
                    throw new AppException("Quantidade início e fim devem ser informados juntos.");

                if (param.quantidadeInicial.HasValue && param.quantidadeFinal.HasValue && param.quantidadeFinal < param.quantidadeInicial)
                    throw new AppException("'quantidadeFim' não pode ser menor que 'quantidadeInicio'.");

                var statusEncerrado = await _conversaRepository.GetByPredicateAsync<ConversaStatus>(e => e.Codigo == "ENCERRADA") ?? throw new AppException("Status com código 'ENCERRADA' não foi encontrado");
                var statusEntregue = await _mensagemReaderService.GetMensagensStatusAsync(null, "DELIVERED");

                (List<Conversa> conversas, int total) = await _conversaRepository.GetConversasPaginadasByUsuarioAsync(usuarioId, statusEncerrado.Id, param.quantidadeInicial, param.quantidadeFinal, param.empresaId, param.EquipeId);

                List<ConversaListaDTO> resultado = [];

                foreach (var conversa in conversas)
                {
                    var janelaAberta = await _conversaRepository.JanelaAbertaDaConversaAsync(conversa.Id, statusEncerrado.Id);

                    int qtdMensagensNaoLidas = 0;
                    if (conversa.PossuiMensagensNaoLidas)
                    {
                        var mensagensNaoLidas = await _mensagemReaderService.GetMensagensNaoLidasByConversaAsync(conversa.Id, statusEntregue.Id);
                        qtdMensagensNaoLidas = mensagensNaoLidas?.Count ?? 0;
                    }

                    var ultimaMensagem = await _mensagemReaderService.GetUltimaMensagemByConversaAsync(conversa.Id) ?? throw new AppException("Última mensagem não pode ser nula.");

                    string caption = string.Empty;

                    var tipoUltimaMensagem = await _mensagemReaderService.GetMensagemTipoAsync(ultimaMensagem.TipoId) ?? throw new AppException("Tipo da mensagem não pode ser nulo.");

                    if (tipoUltimaMensagem.Codigo != "TEXT" && tipoUltimaMensagem.Codigo != "LOCATION" && tipoUltimaMensagem.Codigo != "INTERACTIVE" && tipoUltimaMensagem.Codigo != "UNKNOWN" && tipoUltimaMensagem.Codigo != "CONTACTS")
                    {
                        var midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(ultimaMensagem.Id);
                        caption = midia?.Caption ?? string.Empty;
                    }

                    var lead = await _leadReader.GetLeadByIdAsync(conversa.LeadId);
                    if (lead == null)
                    {
                        _logger.LogWarning("Lead com ID {LeadId} não encontrado para conversa {ConversaId}",
                            conversa.LeadId, conversa.Id);
                        continue;
                    }

                    var leadStatus = await _leadReader.GetLeadStatusCodigoAsync(lead.LeadStatusId) ?? throw new AppException("O status do lead não pode ser nulo.");

                    var primiraMensagemCliente = await _conversaRepository.GetPrimeiraMensagemClienteAsync(conversa.Id);
                    string iniciouContato;

                    if (primiraMensagemCliente.UsuarioId != null)
                    {
                        iniciouContato = "Vendedor";
                    }
                    else
                    {
                        iniciouContato = "Cliente";
                    }

                    var numeroWhatsapp = lead.WhatsappNumero;
                    if (lead.Equipe.ResponsavelMembro.UsuarioId != usuarioLogadoId)
                    {
                        numeroWhatsapp = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                    }

                    var ultimoEvento = lead.LeadEventos?.FirstOrDefault();
                    var conversaSincronizada = new ConversaListaDTO
                    {
                        ConversaId = conversa.Id,
                        NumeroWhatsapp = numeroWhatsapp!,
                        ConversaStatus = conversa.Status.Nome,
                        LeadName = lead.Nome,
                        Apelido = lead.Apelido,
                        LeadId = lead.Id,
                        LeadStatus = leadStatus,
                        LeadEmpresaId = lead.EmpresaId,
                        JanelaAberta = janelaAberta,
                        Tipo = tipoUltimaMensagem.Codigo,
                        UltimaMensagem = ultimaMensagem?.Conteudo ?? caption ?? tipoUltimaMensagem.Codigo,
                        DataUltimaMensagem = conversa.DataUltimaMensagem ?? DateTime.MinValue,
                        QtdMensagensNaoLidas = qtdMensagensNaoLidas,
                        DataInicioConversa = conversa.DataInicio,
                        IniciouContato = iniciouContato,
                        UsuarioId = conversa.UsuarioId,
                        CampanhaId = ultimoEvento?.CampanhaId,
                        CampanhaNome = ultimoEvento?.Campanha?.Nome ?? "O evento atual do lead não possui campanha.",
                        Fixada = conversa.Fixada,
                    };

                    resultado.Add(conversaSincronizada);
                }

                return new ConversaPag
                {
                    Conversas = resultado,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar conversa para usuário {UsuarioId}", usuarioId);
                throw new AppException("Erro interno ao sincronizar conversas", ex);
            }
        }

        public async Task<TemConversaDTO> VerificarSeLeadTemConversaAtivaAsync(int leadId, int usuarioLogadoId)
        {
            try
            {
                if (leadId <= 0)
                    throw new AppException("ID do Lead deve ser maior que zero.");

                // Busca dados do lead
                var lead = await _leadReader.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    throw new AppException($"Lead com ID {leadId} não foi encontrado.");
                }

                var statusEncerrado = await _conversaRepository.GetByPredicateAsync<ConversaStatus>(e => e.Codigo == "ENCERRADA") ?? throw new AppException("Status com código 'ENCERRADA' não foi encontrado");
                //var statusEntregue = await _mensagemReaderService.GetMensagensStatusAsync(null, "DELIVERED");

                Conversa? conversa = await _conversaRepository.GetConversaNaoEncerradasByLeadAAsync(leadId, statusEncerrado.Id);

                if (conversa != null)
                {
                    var janelaAberta = await _conversaRepository.JanelaAbertaDaConversaAsync(conversa.Id, statusEncerrado.Id);

                    int? mensagensNaoLidas = 0;
                    if (conversa.PossuiMensagensNaoLidas)
                    {
                        mensagensNaoLidas = await _mensagemReaderService.GetQntdMensagensNaoLidasByConversaAsync(conversa.Id, "DELIVERED");
                    }

                    var ultimaMensagem = await _mensagemReaderService.GetUltimaMensagemByConversaAsync(conversa.Id) ?? throw new AppException("Última mensagem não pode ser nula.");

                    string caption = string.Empty;
                    if (ultimaMensagem.Tipo.Codigo != "TEXT" && ultimaMensagem.Tipo.Codigo != "LOCATION" && ultimaMensagem.Tipo.Codigo != "INTERACTIVE" && ultimaMensagem.Tipo.Codigo != "UNKNOWN" && ultimaMensagem.Tipo.Codigo != "CONTACTS")
                    {
                        var midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(ultimaMensagem.Id);
                        caption = midia?.Caption ?? string.Empty;
                    }

                    // Busca o código do status do lead
                    var leadStatus = await _leadReader.GetLeadStatusCodigoAsync(lead.LeadStatusId) ?? throw new AppException("O status do lead não pode ser nulo.");

                    var primiraMensagemCliente = await _conversaRepository.GetPrimeiraMensagemClienteAsync(conversa.Id);
                    string iniciouContato;

                    if (primiraMensagemCliente.UsuarioId != null)
                    {
                        iniciouContato = "Vendedor";
                    }
                    else
                    {
                        iniciouContato = "Cliente";
                    }

                    var numeroWhatsapp = lead.WhatsappNumero;

                    if (lead.Equipe.ResponsavelMembro.UsuarioId != usuarioLogadoId)
                    {
                        numeroWhatsapp = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                    }

                    // Monta o resultado
                    var conversaSincronizada = new ConversaListaDTO
                    {
                        ConversaId = conversa.Id,
                        NumeroWhatsapp = numeroWhatsapp!,
                        ConversaStatus = conversa.Status.Nome,
                        LeadName = lead.Nome,
                        Apelido = lead.Apelido,
                        LeadId = lead.Id,
                        LeadStatus = leadStatus,
                        LeadEmpresaId = lead.EmpresaId,
                        JanelaAberta = janelaAberta,
                        Tipo = ultimaMensagem.Tipo.Codigo,
                        UltimaMensagem = ultimaMensagem?.Conteudo ?? caption ?? ultimaMensagem.Tipo.Codigo,
                        DataUltimaMensagem = conversa.DataUltimaMensagem ?? DateTime.MinValue,
                        QtdMensagensNaoLidas = mensagensNaoLidas ?? 0,
                        DataInicioConversa = conversa.DataInicio,
                        IniciouContato = iniciouContato,
                        UsuarioId = conversa.UsuarioId,
                        Fixada = conversa.Fixada
                    };

                    return new TemConversaDTO
                    {
                        TemConversa = true,
                        Conversa = conversaSincronizada
                    };
                }

                return new TemConversaDTO
                {
                    TemConversa = false,
                    Conversa = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se o lead {leadId} tem conversa ativa.", leadId);
                throw;
            }
        }

        public async Task<int> GetConversaByLeadAndCanalAsync(int leadId, int responsavelId, int canalId, string statusConversa, int equipeId, bool leadNovo, bool? integracao = false)
        {
            try
            {
                if (canalId <= 0)
                    throw new AppException("Canal ID deve ser maior que zero.");

                if (leadId <= 0)
                    throw new AppException("Lead Id deve ser maior que zero.");

                string redisKeyLeadCanal = $"conversa:lead:{leadId}:canal:{canalId}";

                var conversaExistente = await _redisCacheService.GetAsync<ConversaCacheDTO>(redisKeyLeadCanal);

                if (conversaExistente is not null)
                {
                    return conversaExistente.Id;
                }

                var conversaStatusEncerrado = await _conversaRepository.GetByPredicateAsync<ConversaStatus>(e => e.Codigo == "ENCERRADA") ?? throw new AppException("Conversa Status Id com o código 'ENCERRADA' não foi encontrado.");

                var conversaBanco = await _conversaRepository.GetConversaByLeadAndCanalAsync(leadId, canalId, conversaStatusEncerrado.Id);

                if (conversaBanco is not null)
                {
                    var dtoBanco = new ConversaCacheDTO
                    {
                        Id = conversaBanco.Id
                    };

                    await _redisCacheService.SetAsync(redisKeyLeadCanal, dtoBanco);

                    return conversaBanco.Id;
                }

                var conversaStatus = await _conversaRepository.GetByPredicateAsync<ConversaStatus>(e => e.Codigo == statusConversa) ?? throw new AppException($"Conversa status {statusConversa} não foi encontrado.");

                var conversaCreateDTO = new ConversaCreateDTO("Nova Conversa", leadId, canalId, conversaStatus.Id, equipeId);
                var novaConversa = await RegisterConversaAsync(conversaCreateDTO, responsavelId, leadNovo, integracao);
                var conversaCacheDTO = new ConversaCacheDTO
                {
                    Id = novaConversa.Id
                };

                await _redisCacheService.SetAsync(redisKeyLeadCanal, conversaCacheDTO);

                return novaConversa.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar conversa");
                throw;
            }
        }

        public async Task UpdateInfoMensagemAsync(int conversaId, DateTime dataMensagem)
        {
            try
            {
                if (conversaId <= 0)
                    throw new AppException("O ID da conversa precisa ser maior que zero.");

                var conversa = await _conversaRepository.GetByIdAsync<Conversa>(conversaId, false) ?? throw new AppException($"Conversa com ID {conversaId} não encontrada no banco de dados.");

                conversa.AtualizarUltimaMensagem(dataMensagem, true);

                _conversaRepository.Update<Conversa>(conversa);

                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao procurar uma conversa com o id: {id}", conversaId);
                throw;
            }
        }

        public async Task UpdateDataUltimaMensagemAsync(int conversaId, DateTime dataMensagem)
        {
            try
            {
                if (conversaId <= 0)
                    throw new AppException("O ID da conversa precisa ser maior que zero.");

                var conversa = await _conversaRepository.GetByIdAsync<Conversa>(conversaId, false) ?? throw new AppException($"Conversa com ID {conversaId} não encontrada no banco de dados.");

                conversa.AtualizarDataUltimaMensagem(dataMensagem);

                _conversaRepository.Update<Conversa>(conversa);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao procurar uma conversa com o id: {id}", conversaId);
                throw;
            }
        }
        public async Task UpdateConversaMetaIdAsync(int conversaId, string idExternoMeta)
        {
            try
            {
                if (conversaId <= 0)
                    throw new AppException("O ID da conversa precisa ser maior que zero.");
                if (string.IsNullOrWhiteSpace(idExternoMeta))
                    throw new AppException("O ID Externo Meta não pode ser nulo ou vazio.");
                var conversa = await _conversaRepository.GetByIdAsync<Conversa>(conversaId, false) ?? throw new AppException($"Conversa com ID {conversaId} não encontrada no banco de dados.");
                conversa.AtualizarIdMetaConversa(idExternoMeta);
                _conversaRepository.Update<Conversa>(conversa);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar o Id Externo Meta da conversa com o id: {id}", conversaId);
                throw;
            }
        }

        public async Task UpdateConversaStatus(ConversaStatusDTO statusDTO, string commit)
        {
            var validationResult = await _validatorStatus.ValidateAsync(statusDTO);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                throw new AppException($"Dados inválidos para criação e envio de mensagem para a fila: {errors}");
            }

            try
            {
                var conversa = await _conversaRepository.GetByIdAsync<Conversa>(statusDTO.ConversaID) ?? throw new AppException($"Conversa com id {statusDTO.ConversaID} não foi encontrada.");
                var status = await _conversaRepository.GetByIdAsync<ConversaStatus>(statusDTO.StatusId) ?? throw new AppException($"Status com id {statusDTO.StatusId} não existe.");

                if (status.Codigo == "ENCERRADA")
                {
                    await _redisCacheService.RemoveAsync($"conversa:lead:{conversa.LeadId}:canal:{conversa.CanalId}");
                    await _redisCacheService.RemoveAsync($"conversa:{conversa.Id}");
                }

                conversa.AtualizarStatus(statusDTO.StatusId);
                _conversaRepository.Update<Conversa>(conversa);

                if (commit == "commit")
                {
                    await _unitOfWork.BeginTransactionAsync();
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar status da conversa {conversaId}. O status id é {statusId}.", statusDTO.ConversaID, statusDTO.StatusId);
                throw;
            }
    ;
        }

        public async Task UpdateResponsavelAsync(Conversa conversa, int novoUsuarioId, int canalId, int equipeId)
        {
            try
            {
                if (conversa == null)
                {
                    throw new AppException("Conversa não pode ser nula.");
                }

                if (novoUsuarioId <= 0)
                {
                    throw new AppException("O ID do novo usuário deve ser maior que zero.");
                }
                conversa.AtualizarResponsavel(novoUsuarioId, canalId, equipeId);
                _conversaRepository.Update<Conversa>(conversa);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                _logger.LogError("Erro ao atualizar o responsável pela conversa. Conversa ID: {conversaId}, Novo Usuário ID: {novoUsuarioId}", conversa.Id, novoUsuarioId);
                throw;
            }
        }

        public async Task UpdatePossuiMensagensNaoLidas(int conversaId, bool possui)
        {
            try
            {
                var conversa = await _conversaRepository.GetByIdAsync<Conversa>(conversaId) ?? throw new AppException($"Conversa com ID {conversaId} não foi encontrada.");
                conversa.AtualizarPossuiMensagensNaoLidas(possui);
                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar o campo PossuiMensagensNaoLidas.");
                throw;
            }
        }

        private async Task<Conversa> RegisterConversaAsync(ConversaCreateDTO dto, int usuarioResponsavel, bool leadNovo, bool? integracao = false)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Titulo))
                {
                    throw new AppException($"Titulo não pode ser nulo.");
                }

                var conversa = new Conversa(dto.Titulo, dto.LeadId, usuarioResponsavel, dto.CanalId, dto.StatusId, dto.EquipeId);
                await _conversaRepository.CreateAsync<Conversa>(conversa);

                var lead = await _leadReader.GetLeadByIdAsync(dto.LeadId) ?? throw new AppException($"Lead com ID {dto.LeadId} não foi encontrado.");

                if (!leadNovo && !integracao.Value)
                    await _leadEventoWriterService.RegistrarEventoConversaViaWhatsAsync(lead, dto.CanalId, null);

                await _unitOfWork.SaveChangesAsync();
                return conversa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar conversa");
                throw;
            }
        }

        public async Task EncerrarConversasAtivasByLeadAsync(int leadId)
        {
            try
            {
                var conversaStatusEncerradaId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");
                var conversasAtivas = await _conversaRepository.GetListByPredicateAsync<Conversa>(
                    e => e.LeadId == leadId && e.StatusId != conversaStatusEncerradaId
                );

                if (conversasAtivas?.Any() == true)
                {

                    foreach (var conversa in conversasAtivas)
                    {
                        var dto = new ConversaStatusDTO
                        {
                            ConversaID = conversa.Id,
                            StatusId = conversaStatusEncerradaId
                        };

                        await UpdateConversaStatus(dto, "save");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encerrar conversas ativas para lead {leadId}", leadId);
                throw;
            }
        }

        public async Task EncerrarConversaAsync(int conversaId, int usuarioLogado)
        {
            try
            {
                var conversa = await _conversaRepository.GetConversaById(conversaId) ?? throw new Exception("Conversa não encontrada.");
                if (conversa.UsuarioId != usuarioLogado)
                {
                    throw new AppException("Apenas o responsável pela conversa pode encerrá-la.");
                }

                var conversaStatusEncerradaId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");
                await _unitOfWork.BeginTransactionAsync();
                if (conversa != null)
                {
                    var bot = await _usuarioEmpresaReaderService.GetBotByEmpresa(conversa.Lead.EmpresaId);

                    if (bot != null && usuarioLogado == bot.UsuarioId)
                    {
                        var membroEquipeBot = await _membroEquipeReaderService.GetBotMembroEquipeAsync(bot.UsuarioId, conversa.Lead.EmpresaId);
                        conversa.Lead.AtribuirResponsavel(membroEquipeBot.Id, membroEquipeBot.EquipeId, conversa.Lead.EmpresaId);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    if (conversa.StatusId == conversaStatusEncerradaId)
                    {
                        throw new AppException("A conversa já está encerrada.");
                    }

                    var dto = new ConversaStatusDTO
                    {
                        ConversaID = conversa.Id,
                        StatusId = conversaStatusEncerradaId
                    };

                    await UpdateConversaStatus(dto, "save");

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Encerra a conversa e garante que o lead seja transferido para o bot,
        /// evitando que fique sem responsável após o encerramento.
        /// </summary>
        /// <param name="conversaId">Id da conversa que será encerrada.</param>
        /// <exception cref="AppException">Lançada quando a conversa já está encerrada.</exception>
        /// <exception cref="Exception">Lançada quando a conversa não é encontrada.</exception>
        public async Task EncerrarConversaAsync(int conversaId)
        {
            try
            {
                var conversa = await _conversaRepository.GetConversaById(conversaId) ?? throw new Exception("Conversa não encontrada.");

                var conversaStatusEncerradaId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");
                await _unitOfWork.BeginTransactionAsync();
                if (conversa != null)
                {
                    var bot = await _usuarioEmpresaReaderService.GetBotByEmpresa(conversa.Lead.EmpresaId);
                    if (bot != null)
                    {
                        var membroEquipeBot = await _membroEquipeReaderService.GetBotMembroEquipeAsync(bot.UsuarioId, conversa.Lead.EmpresaId);
                        conversa.Lead.AtribuirResponsavel(membroEquipeBot.Id, membroEquipeBot.EquipeId, conversa.Lead.EmpresaId);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    if (conversa.StatusId == conversaStatusEncerradaId)
                    {
                        throw new AppException("A conversa já está encerrada.");
                    }

                    var dto = new ConversaStatusDTO
                    {
                        ConversaID = conversa.Id,
                        StatusId = conversaStatusEncerradaId
                    };

                    await UpdateConversaStatus(dto, "save");

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ConversasEncerradasLeadResultDTO> ListConversasEncerradasByLeadAsync(int leadId, LeadConversaEncerradaParamsDTO param)
        {
            try
            {
                if (leadId <= 0)
                    throw new AppException("ID do lead deve ser maior que zero.");

                if ((param.pagInicial.HasValue && !param.pagFinal.HasValue) ||
                    (!param.pagInicial.HasValue && param.pagFinal.HasValue))
                    throw new AppException("Pagina início e fim devem ser informados juntos.");

                if (param.pagInicial.HasValue && param.pagFinal.HasValue &&
                    param.pagFinal < param.pagInicial)
                    throw new AppException("'pagFim' não pode ser menor que 'pagInicio'.");

                var statusEncerrado = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");

                var (conversas, totalEncerradas) =
                    await _conversaRepository.GetConversasEncerradasByLeadAsync(
                        leadId,
                        statusEncerrado,
                        param.pagInicial,
                        param.pagFinal
                    );

                var conversaIds = conversas.Select(c => c.Id).ToList();

                var mensagensDict = await _mensagemReaderService.GetUltimasMensagensByListConversasAsync(conversaIds);

                var listaDto = new List<ListConversasEncerradasLeadDTO>();

                foreach (var c in conversas)
                {
                    mensagensDict.TryGetValue(c.Id, out var ultimaMensagem);

                    listaDto.Add(new ListConversasEncerradasLeadDTO
                    {
                        ConversaId = c.Id,
                        UsuarioId = c.UsuarioId,
                        UsuarioNome = c.Usuario.Nome ?? string.Empty,
                        LeadId = c.LeadId,
                        LeadNome = c.Lead.Nome ?? string.Empty,
                        Status = "ENCERRADA",
                        EmpresaId = c.Lead.EmpresaId,
                        EmpresaNome = c.Lead.Empresa.Nome,
                        DataInicio = c.DataCriacao,
                        DataEncerramento = c.DataModificacao,
                        EquipeId = c.EquipeId,
                        EquipeNome = c.Equipe?.Nome ?? string.Empty,
                        UltimaMensagem = ultimaMensagem?.Conteudo ?? string.Empty
                    });
                }

                return new ConversasEncerradasLeadResultDTO
                {
                    TotalEncerradas = totalEncerradas,
                    Conversas = listaDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar conversas encerradas do lead {leadId}", leadId);
                throw;
            }
        }

        public async Task<string> AlterarFixacaoConversaAsync(int conversaId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var conversa = await _conversaRepository.GetConversaById(conversaId) ?? throw new Exception("Conversa não encontrada");

                string mensagem;
                if (conversa.Fixada)
                {
                    conversa.Desafixar();
                    mensagem = "Desfixado com sucesso";
                }
                else
                {
                    var totalFixadas = await _conversaRepository.GetQuantidadeConversasFixadasAsync(conversaId, conversa.UsuarioId);

                    if (totalFixadas >= 3)
                        throw new Exception("O limite total de conversas fixadas (3) foi atingido.");

                    conversa.Fixar();
                    mensagem = "Fixado com sucesso";
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return mensagem;
            }

            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao fixar conversa {conversaId}.", conversaId);
                throw;
            }
        }
    }
}
