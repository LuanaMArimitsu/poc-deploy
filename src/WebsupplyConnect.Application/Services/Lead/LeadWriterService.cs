using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.DTOs.Redis;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Notificacao;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Application.Services.Equipe;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class LeadWriterService(IUnitOfWork unitOfWork, ILeadRepository leadRepository, IMembroEquipeRepository membroEquipeRepository, ILogger<LeadWriterService> logger, IRedisCacheService redisCacheService, IEnderecoWriterService enderecoWriterService, ICanalReaderService canalReaderService, IEmpresaReaderService empresaReaderService, IConversaReaderService conversaReaderService, IConversaWriterService conversaWriterService, IOportunidadeReaderService oportunidadeReaderService, IOportunidadeWriterService oportunidadeWriterService, IUsuarioEmpresaReaderService usuarioEmpresaReaderService, IValidator<LeadCompletoDTO> leadCompletoValidator, INotificacaoClient notificacaoClient, INotificacaoWriterService notificacaoWriterService, ILeadEventoWriterService leadEventoWriterService, ICampanhaReaderService campanhaReaderService, IOrigemReaderService origemReaderService, IEquipeReaderService equipeReaderService, IMembroEquipeReaderService membroEquipeReaderService, ITemplateReaderService templateReaderService,
      IMensagemWriterService mensagemWriterService) : ILeadWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILeadRepository _leadRepository = leadRepository ?? throw new ArgumentNullException(nameof(leadRepository));
        private readonly IMembroEquipeRepository _membroRepo = membroEquipeRepository ?? throw new ArgumentNullException(nameof(membroEquipeRepository));
        private readonly ILogger<LeadWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRedisCacheService _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly IEnderecoWriterService _enderecoWriterService = enderecoWriterService ?? throw new ArgumentNullException(nameof(enderecoWriterService));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService ?? throw new ArgumentNullException(nameof(conversaWriterService));
        private readonly IOportunidadeReaderService _oportunidadeReaderService = oportunidadeReaderService ?? throw new ArgumentNullException(nameof(oportunidadeReaderService));
        private readonly IOportunidadeWriterService _oportunidadeWriterService = oportunidadeWriterService ?? throw new ArgumentNullException(nameof(oportunidadeWriterService));
        private readonly IValidator<LeadCompletoDTO> _leadCompletoValidator = leadCompletoValidator ?? throw new ArgumentNullException(nameof(leadCompletoValidator));
        private readonly INotificacaoClient _notificacaoClient = notificacaoClient ?? throw new ArgumentNullException(nameof(notificacaoClient));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService ?? throw new ArgumentNullException(nameof(usuarioEmpresaReaderService));
        private readonly INotificacaoWriterService _notificacaoWriterService = notificacaoWriterService ?? throw new ArgumentNullException(nameof(notificacaoWriterService));
        private readonly ILeadEventoWriterService _leadEventoWriterService = leadEventoWriterService ?? throw new ArgumentNullException(nameof(leadEventoWriterService));
        private readonly ICampanhaReaderService _campanhaReaderService = campanhaReaderService ?? throw new ArgumentNullException(nameof(campanhaReaderService));
        private readonly IOrigemReaderService _origemReaderService = origemReaderService ?? throw new ArgumentNullException(nameof(origemReaderService));
        private readonly IEquipeReaderService _equipeReaderService = equipeReaderService ?? throw new ArgumentNullException(nameof(equipeReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly ITemplateReaderService _templateReaderService = templateReaderService ?? throw new ArgumentNullException(nameof(templateReaderService));
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService ?? throw new ArgumentNullException(nameof(mensagemWriterService));

        public async Task<Domain.Entities.Lead.Lead> CreateAsync(LeadCompletoDTO dto, bool commit = true, string? observacaoLeadEvento = null)
        {
            try
            {
                // Validação do DTO
                var validationResult = await _leadCompletoValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                    throw new AppException($"Dados inválidos para cadastro de lead: {errors}");
                }

                var equipe = await _equipeReaderService.GetEquipeByIdAsync(dto.EquipeId)
                    ?? throw new AppException("Equipe não encontrada.");

                if (!dto.ResponsavelId.HasValue || dto.ResponsavelId <= 0)
                {
                    throw new AppException("É obrigatório informar o responsável pelo lead.");
                }

                var membro = await _membroRepo.GetByIdComStatusAsync(dto.ResponsavelId.Value)
                    ?? throw new AppException("Membro não encontrado ou inativo.");

                if (membro.EquipeId != dto.EquipeId)
                    throw new AppException("O responsável selecionado não pertence à equipe informada.");

                if (!string.Equals(membro.StatusMembroEquipe.Codigo, "ATIVO", StringComparison.OrdinalIgnoreCase))
                    throw new AppException("O membro não possui status 'ATIVO'.");

                //ORIGEM
                var origem = await _leadRepository.ExistsInDatabaseAsync<Origem>(dto.OrigemId);
                if (!origem)
                    throw new AppException($"Origem com ID igual a {dto.OrigemId} não existe.");
                //STATUS
                int leadStatusNovo = await _leadRepository.GetLeadStatusId("NOVO");
                if (leadStatusNovo <= 0)
                    throw new AppException("Status 'NOVO' não encontrado para leads");

                //EMPRESA
                var empresa = await _empresaReaderService.EmpresaExistsAsync(dto.EmpresaId);
                if (!empresa)
                    throw new AppException($"Empresa com ID igual a {dto.EmpresaId} não existe.");

                // CAMPANHA
                if (dto.CampanhaId.HasValue && dto.CampanhaId.Value > 0)
                {
                    var campanha = await _campanhaReaderService.CampanhaExistsByIdAsync(dto.CampanhaId.Value, dto.EmpresaId) ?? throw new AppException($"Campanha com ID igual a {dto.CampanhaId} não existe para a empresa ID {dto.EmpresaId}.");
                }

                // Normalizar CPF se houver
                if (!string.IsNullOrWhiteSpace(dto.CPF))
                {
                    dto.CPF = new string(dto.CPF.Where(char.IsDigit).ToArray());
                }

                if (!string.IsNullOrWhiteSpace(dto.CNPJEmpresa))
                {
                    dto.CNPJEmpresa = new string(dto.CNPJEmpresa.Where(char.IsDigit).ToArray());
                }

                // Normalizar WhatsApp
                var whatsappNormalizado = dto.WhatsappNumero != null
                    ? NormalizarWhatsApp(dto.WhatsappNumero)
                    : null;

                if (whatsappNormalizado is not null &&
                   (whatsappNormalizado.Length != 12 &&
                    whatsappNormalizado.Length != 13))
                {
                    throw new AppException("Número de WhatsApp inválido.");
                }

                var grupoEmpresas = await _empresaReaderService.GetGrupoEmpresaByEmpresaId(dto.EmpresaId);
                var leadNoMesmoGrupo = await _leadRepository.ObterLeadExistenteNoMesmoGrupo(whatsappNormalizado, dto.Email, dto.CPF, grupoEmpresas.Id);

                if (leadNoMesmoGrupo != null)
                {
                    throw new AppException($"Lead não pode ser criado pois já está em atendimento na filial: {leadNoMesmoGrupo.Empresa.Nome}. Solicite transferência.");
                }

                var leadExistente = await _leadRepository.GetByPredicateAsync<Domain.Entities.Lead.Lead>(
                    x => ((whatsappNormalizado != null && x.WhatsappNumero == whatsappNormalizado)
                       || (dto.CPF != null && x.CPF == dto.CPF)
                       || (dto.Email != null && x.Email == dto.Email))
                       && x.EmpresaId == dto.EmpresaId,
                    true
                );

                if (leadExistente != null)
                {
                    if (leadExistente.Excluido == true)
                    {
                        leadExistente.RestaurarExclusaoLogica();
                        await UpdateAsync(leadExistente.Id, new LeadUpdateDTO
                        {
                            NivelInteresse = dto.NivelInteresse ?? leadExistente.NivelInteresse,
                            Cargo = dto.Cargo,
                            CNPJEmpresa = dto.CNPJEmpresa,
                            CPF = dto.CPF,
                            DataNascimento = dto.DataNascimento,
                            Email = dto.Email,
                            EmpresaId = dto.EmpresaId,
                            Genero = dto.Genero,
                            Nome = dto.Nome,
                            NomeEmpresa = dto.NomeEmpresa,
                            ObservacoesCadastrais = dto.ObservacoesCadastrais,
                            OrigemId = dto.OrigemId,
                            ResponsavelId = dto.ResponsavelId.Value,
                            Telefone = dto.Telefone,
                            WhatsappNumero = dto.WhatsappNumero,
                            StatusId = leadStatusNovo,
                            EquipeId = dto.EquipeId
                        }, true);

                        return leadExistente;
                    }
                    else
                    {
                        throw new AppException($"Já existe um lead ativo cadastrado com o número de WhatsApp e/ou E-mail informado para essa empresa.");
                    }
                }

                int? enderecoResidencialId = null;
                int? enderecoComercialId = null;

                var novoLead = new Domain.Entities.Lead.Lead(
                    dto.Nome,
                    leadStatusNovo,
                    membro.Id,
                    dto.EquipeId,
                    dto.OrigemId,
                    dto.EmpresaId,
                    whatsappNormalizado,
                    dto.Email,
                    dto.Telefone,
                    dto.Cargo,
                    dto.CPF,
                    dto.Genero,
                    dto.CNPJEmpresa,
                    dto.NomeEmpresa,
                    dto.NivelInteresse,
                    dto.ObservacoesCadastrais,
                    dto.DataNascimento,
                    enderecoResidencialId,
                    enderecoComercialId
                );

                await _unitOfWork.BeginTransactionAsync();
                await _leadRepository.CreateAsync(novoLead);
                await _unitOfWork.SaveChangesAsync();

                await _leadEventoWriterService.RegistrarEventoAsync(novoLead, dto.CampanhaId, observacaoLeadEvento);

                if (commit)
                {
                    await _unitOfWork.CommitAsync();

                    NotificarNovoLeadDTO novoLeadDto = new()
                    {
                        LeadId = novoLead.Id,
                        UsuarioId = membro.UsuarioId
                    };

                    await _notificacaoClient.NovoLead(novoLeadDto);
                }

                return novoLead;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro inesperado ao registrar lead");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var lead = await _leadRepository.GetLeadWithUsuarioAsync(id) ?? throw new AppException($"Lead com ID {id} não encontrado.");

                await _unitOfWork.BeginTransactionAsync();
                lead.ExcluirLogicamente();

                //excluir endereços associados
                if (lead.EnderecoResidencialId > 0)
                {
                    await _enderecoWriterService.ExcluirEnderecoAsync(lead.EnderecoResidencialId.Value);
                }
                if (lead.EnderecoComercialId > 0)
                {
                    await _enderecoWriterService.ExcluirEnderecoAsync(lead.EnderecoComercialId.Value);
                }

                var conversasAtivas = await _conversaReaderService.GetAllConversasAtivaByLeadAsync(id, "ENCERRADA");
                var conversaStatusEncerrada = await _conversaReaderService.GetConversaStatusAsync(codigo: "ENCERRADA");

                await _conversaWriterService.EncerrarConversasAtivasByLeadAsync(id);

                var oportunidades = await _oportunidadeReaderService.GetListOportunidadesByLeadIdAsync(id);
                if (oportunidades != null && oportunidades.Count != 0)
                {
                    foreach (var oportunidade in oportunidades)
                    {
                        await _oportunidadeWriterService.DeleteOportunidadeAsync(oportunidade.Id);
                    }
                }

                var primaryKey = $"lead:{lead.Id}";
                await _redisCacheService.RemoveAsync(primaryKey);

                var dto = new NotificarNovoLeadDTO
                {
                    UsuarioId = lead.Responsavel.UsuarioId,
                    LeadId = lead.Id
                };

                await _unitOfWork.CommitAsync();
                await _notificacaoWriterService.LeadExcluido(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao deletar lead com ID {id}", id);
                throw;
            }
        }

        public async Task UpdateStatusAsync(int id, int statusId, string observacao)
        {
            try
            {
                // Busca do Lead
                var lead = await _leadRepository.GetLeadWithUsuarioAsync(id)
                           ?? throw new AppException($"Lead não foi encontrado.");

                // Validação do novo status
                var leadStatus = await _leadRepository.GetByIdAsync<LeadStatus>(statusId, false)
                                  ?? throw new AppException($"Status não existe no sistema.");

                await _unitOfWork.BeginTransactionAsync();

                lead.AlterarStatus(statusId, lead.Responsavel.UsuarioId, observacao);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao editar o status do lead com ID {id}", id);
                throw;
            }
        }

        public async Task UpdateAsync(int id, LeadUpdateDTO dto, bool excluido = false)
        {
            try
            {
                // Validação: WhatsApp e Email não podem ser ambos nulos
                if (string.IsNullOrWhiteSpace(dto.WhatsappNumero) && string.IsNullOrWhiteSpace(dto.Email))
                {
                    throw new AppException("É obrigatório informar pelo menos o número de WhatsApp ou o e-mail do lead.");
                }

                if (dto.StatusId <= 0)
                {
                    throw new AppException("Status do lead é obrigatório e deve ser maior que zero.");
                }

                if (dto.DataNascimento.HasValue)
                {
                    var dataAtual = TimeHelper.GetBrasiliaTime();

                    if (dto.DataNascimento.Value.Date > dataAtual.Date)
                    {
                        throw new AppException("A data de nascimento não pode ser maior que a data atual.");
                    }
                }

                // Normalizar CPF se houver
                if (!string.IsNullOrWhiteSpace(dto.CPF))
                {
                    dto.CPF = new string(dto.CPF.Where(char.IsDigit).ToArray());
                }

                if (!string.IsNullOrWhiteSpace(dto.CNPJEmpresa))
                {
                    dto.CNPJEmpresa = new string(dto.CNPJEmpresa.Where(char.IsDigit).ToArray());
                }


                Domain.Entities.Lead.Lead lead;

                if (excluido)
                {
                    lead = await _leadRepository.GetLeadWithUsuarioIncludingDeletedAsync(id)
                        ?? throw new AppException($"Lead com ID {id} não encontrado.");
                }
                else
                {
                    lead = await _leadRepository.GetLeadWithUsuarioAsync(id)
                        ?? throw new AppException($"Lead com ID {id} não encontrado.");
                }

                // Validar e atualizar OrigemId se enviado e diferente
                if (dto.OrigemId.HasValue && dto.OrigemId.Value > 0 && dto.OrigemId != lead.OrigemId)
                {
                    var origemExiste = await _leadRepository.ExistsInDatabaseAsync<Origem>(dto.OrigemId.Value);
                    if (!origemExiste)
                        throw new AppException($"Origem com ID igual a {dto.OrigemId} não existe.");
                }

                // Validar e atualizar Status se enviado e diferente
                if (dto.StatusId > 0 && dto.StatusId != lead.LeadStatusId)
                {
                    var leadStatus = await _leadRepository.GetByIdAsync<LeadStatus>(dto.StatusId, false) ?? throw new AppException($"Status do lead ID {dto.StatusId} não encontrado.");

                    lead.AlterarStatus(dto.StatusId, lead.Responsavel.UsuarioId, "STATUS ALTERADO");
                }

                if (dto.NivelInteresse == null)
                {
                    throw new AppException("Nível de interesse é obrigatório.");
                }

                // Guardar dados antigos
                var numeroAntigo = ProtegerInfoHelper.ProtegerTelefone(lead.WhatsappNumero);
                var emailAntigo = ProtegerInfoHelper.ProtegerEmail(lead.Email);
                var cpfAntigo = lead.CPF;
                var telefoneAntigo = ProtegerInfoHelper.ProtegerTelefone(lead.Telefone);

                // Normaliza os novos valores
                var whatsappNormalizado = NormalizarWhatsApp(dto.WhatsappNumero);

                // Flags de alteração
                var whatsappAlterado = !string.Equals(numeroAntigo, whatsappNormalizado, StringComparison.Ordinal);
                if (whatsappAlterado)
                {
                    var conversasAtivas = await _conversaReaderService.GetAllConversasAtivaByLeadAsync(lead.Id, "ENCERRADA");

                    if (conversasAtivas.Any())
                    {
                        throw new AppException("O número de WhatsApp não pode ser alterado enquanto o lead possuir uma conversa ativa.");
                    }
                }

                var emailAlterado = !string.Equals(emailAntigo, dto.Email, StringComparison.OrdinalIgnoreCase);
                var cpfAlterado = !string.Equals(cpfAntigo, dto.CPF, StringComparison.Ordinal);
                var telefoneAlterado = !string.Equals(telefoneAntigo, dto.Telefone, StringComparison.Ordinal);
                if (telefoneAlterado && !string.IsNullOrWhiteSpace(dto.Telefone))
                {
                    var telefoneLimpo = new string(dto.Telefone.Where(char.IsDigit).ToArray());

                    if (telefoneLimpo.Length != 10)
                        throw new AppException("Número de telefone inválido. O telefone deve conter pelo menos 10 dígitos com DDD.");
                }

                if (whatsappAlterado || emailAlterado || cpfAlterado)
                {
                    var erros = new List<string>();

                    var grupoEmpresas = await _empresaReaderService.GetGrupoEmpresaByEmpresaId(lead.EmpresaId);
                    var existeLeadNoMesmoGrupo = await _leadRepository.ObterLeadExistenteNoMesmoGrupo(whatsappNormalizado, dto.Email, dto.CPF, grupoEmpresas.Id);

                    if (existeLeadNoMesmoGrupo is not null && existeLeadNoMesmoGrupo.EmpresaId != lead.EmpresaId)
                    {
                        throw new AppException($"Lead não pode ser alterado pois já está em atendimento na filial: {existeLeadNoMesmoGrupo.Empresa.Nome}. Solicite transferência.");
                    }

                    if (whatsappAlterado && !string.IsNullOrWhiteSpace(whatsappNormalizado))
                    {
                        if (whatsappNormalizado is not null && whatsappNormalizado.Length != 12 && whatsappNormalizado.Length != 13)
                        {
                            throw new AppException("Número de WhatsApp inválido.");
                        }

                        var existeWhatsapp = await _leadRepository.GetByPredicateAsync<Domain.Entities.Lead.Lead>(
                            x => x.EmpresaId == lead.EmpresaId && x.Id != lead.Id && x.WhatsappNumero == whatsappNormalizado
                        );
                        if (existeWhatsapp != null)
                            erros.Add("número de WhatsApp");
                    }

                    if (emailAlterado && !string.IsNullOrWhiteSpace(dto.Email))
                    {
                        var existeEmail = await _leadRepository.GetByPredicateAsync<Domain.Entities.Lead.Lead>(
                            x => x.EmpresaId == lead.EmpresaId && x.Id != lead.Id && x.Email == dto.Email
                        );
                        if (existeEmail != null)
                            erros.Add("E-mail");
                    }

                    if (cpfAlterado && !string.IsNullOrWhiteSpace(dto.CPF))
                    {
                        var existeCpf = await _leadRepository.GetByPredicateAsync<Domain.Entities.Lead.Lead>(
                            x => x.EmpresaId == lead.EmpresaId && x.Id != lead.Id && x.CPF == dto.CPF
                        );
                        if (existeCpf != null)
                            erros.Add("CPF");
                    }

                    if (erros.Any())
                    {
                        var mensagem = "Já existe um lead ativo cadastrado com este "
                                       + string.Join(", ", erros.Take(erros.Count - 1))
                                       + (erros.Count > 1 ? " e " + erros.Last() : erros.First())
                                       + ".";

                        throw new AppException(mensagem);
                    }
                }
                await _unitOfWork.BeginTransactionAsync();
                // Chamada do método de edição na entidade Lead
                lead.EditarLead(
                    dto.Nome,
                    dto.OrigemId,
                    dto.NivelInteresse,
                    emailAlterado ? dto.Email : lead.Email,
                    telefoneAlterado ? dto.Telefone : lead.Telefone,
                    dto.Cargo,
                    whatsappAlterado ? whatsappNormalizado : lead.WhatsappNumero,
                    dto.CPF,
                    dto.Genero,
                    dto.CNPJEmpresa,
                    dto.NomeEmpresa,
                    dto.ObservacoesCadastrais,
                    dto.DataNascimento,
                    lead.EnderecoResidencialId,
                    lead.EnderecoComercialId
                );

                _leadRepository.Update(lead);

                await _unitOfWork.CommitAsync();

                // Atualiza o cache
                var leadDto = new LeadRedisDTO(
                    lead.Id,
                    lead.Nome,
                    whatsappNormalizado,
                    lead.Responsavel.UsuarioId,
                    lead.EquipeId!.Value,
                    lead.EmpresaId
                );

                var primaryKey = $"lead:{lead.Id}";
                await _redisCacheService.SetAsync(primaryKey, leadDto);

                if (!string.Equals(numeroAntigo, whatsappNormalizado, StringComparison.Ordinal))
                {
                    var canal = await _usuarioEmpresaReaderService.GetCanalPadraoByUsuarioEmpresaAsync(
                        lead.Responsavel.UsuarioId,
                        lead.EmpresaId
                    );

                    // Remove índice antigo se existia
                    if (!string.IsNullOrWhiteSpace(numeroAntigo))
                    {
                        var oldIdxKey = $"idx:whatsapp:{numeroAntigo}:canal:{canal.CanalPadraoId}";
                        await _redisCacheService.RemoveAsync(oldIdxKey);
                    }

                    // Cria índice novo se WhatsApp não for nulo
                    if (!string.IsNullOrWhiteSpace(whatsappNormalizado))
                    {
                        var newIdxKey = $"idx:whatsapp:{whatsappNormalizado}:canal:{canal.CanalPadraoId}";
                        await _redisCacheService.SetStringAsync(newIdxKey, primaryKey);
                    }
                }

                var membro = await _membroRepo.GetByIdComStatusAsync(lead.ResponsavelId.Value)
                    ?? throw new AppException("Membro não encontrado ou inativo.");

                NotificarNovoLeadDTO leadAlterado = new()
                {
                    LeadId = lead.Id,
                    UsuarioId = membro.UsuarioId
                };
                await _notificacaoClient.LeadAlterado(leadAlterado);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao editar lead com ID {id}", id);
                throw;
            }
        }


        /// <summary>
        /// Verifica se um lead já existe com base no número WhatsApp e canal.
        /// Se não existir, cria um novo lead automaticamente.
        /// Utiliza cache Redis para otimização de performance.
        /// </summary>
        /// <param name="whatsappNumero">Número do WhatsApp do lead</param>
        /// <param name="canalId">ID do canal de comunicação</param>
        /// <returns>Informações do lead existente ou recém-criado</returns>
        public async Task<LeadDTO> VerificarLeadExistente(string whatsappNumero, List<CanalDTO> listaCanais, string apelido)
        {
            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(whatsappNumero))
                    throw new AppException("Número do WhatsApp é obrigatório");
                var whatsappNormalizado = NormalizarWhatsApp(whatsappNumero);

                if (listaCanais == null || listaCanais.Count == 0)
                    throw new AppException("Lista de canais não pode ser nula ou vazia");

                var empresaIds = listaCanais.Select(c => c.EmpresaId).Distinct().ToList();
                var canal = listaCanais.First();
                var canalId = canal.CanalId;

                // Busca lead em QUALQUER empresa do grupo (todas da lista)
                var leadEncontrado = await _leadRepository.GetLeadByWhatsAppNumberAndGroupAsync(whatsappNormalizado, empresaIds);

                Domain.Entities.Lead.Lead leadFinal;
                bool isNewLead;

                if (leadEncontrado != null)
                {
                    if (leadEncontrado.Excluido)
                    {
                        leadEncontrado.RestaurarExclusaoLogica();
                        await _unitOfWork.SaveChangesAsync();
                    }

                    leadFinal = leadEncontrado;

                    canalId = listaCanais.FirstOrDefault(c => c.EmpresaId == leadEncontrado.EmpresaId)?.CanalId ?? throw new AppException("Lista de empresas não é igual ao do lead.");

                    isNewLead = false;
                }
                else
                {
                    // cria novo lead vinculado à primeira empresa (ou canal principal)
                    leadFinal = await CriarNovoLead(whatsappNormalizado, canalId, apelido);
                    await _leadEventoWriterService.RegistrarEventoViaWhatsAsync(leadFinal, canalId, null);
                    await _unitOfWork.SaveChangesAsync();
                    isNewLead = true;
                }

                // Guarda no cache
                //var leadRedis = new LeadRedisDTO(
                //    leadFinal.Id,
                //    leadFinal.Nome,
                //    leadFinal.WhatsappNumero,
                //    leadFinal.Responsavel?.Usuario?.Id ?? 0,
                //    leadFinal.EquipeId ?? 0,
                //    leadFinal.EmpresaId
                //);

                //var finalPrimaryKey = $"lead:{leadFinal.Id}";
                //var finalIndiceKey = $"idx:whatsapp:{leadFinal.WhatsappNumero}:canal:{canalId}";
                //await _redisCacheService.SetAsync(finalPrimaryKey, leadRedis);
                //await _redisCacheService.SetStringAsync(finalIndiceKey, finalPrimaryKey);

                return new LeadDTO(
                    leadFinal.Id,
                    leadFinal.Responsavel?.Usuario?.Id ?? 0,
                    leadFinal.Responsavel?.Usuario?.Nome ?? string.Empty,
                    leadFinal.ResponsavelId ?? 0,
                    leadFinal.Responsavel?.Usuario?.IsBot ?? false,
                    leadFinal.Nome,
                    isNewLead,
                    whatsappNumero,
                    leadFinal.EmpresaId,
                    canalId,
                    leadFinal.EquipeId ?? 0
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar lead existente. WhatsApp: {WhatsAppNumero}.",
                    whatsappNumero);
                throw;
            }
        }

        /// <summary>
        /// Normaliza o número do WhatsApp removendo caracteres especiais e formatações
        /// </summary>
        public static string NormalizarWhatsApp(string numero, string dddPadrao = "11")
        {
            if (string.IsNullOrWhiteSpace(numero))
                return null;

            var n = new string(numero
                .Where(c => char.IsDigit(c) || c == '*')
                .ToArray());

            if (string.IsNullOrWhiteSpace(n))
                return null;

            if (n.StartsWith("55"))
                n = n.Substring(2);

            if (n.Length == 8 || n.Length == 9)
                n = dddPadrao + n;

            if (n.Length != 10 && n.Length != 11)
                throw new Exception("Número de WhatsApp inválido");

            return "55" + n;
        }

        /// <summary>
        /// Cria um novo lead quando não existe um lead com o número do WhatsApp no canal especificado
        /// </summary>
        private async Task<Domain.Entities.Lead.Lead> CriarNovoLead(string whatsappNumero, int canalId, string apelido)
        {
            try
            {
                var canal = await _canalReaderService.GetCanalByIdAsync(canalId) ?? throw new AppException("Canal não existe");
                if (canal.OrigemPadraoId <= 0)
                    throw new AppException("Canal não possui origem padrão válido");

                int leadStatusNovo = await _leadRepository.GetLeadStatusId("NOVO");
                if (leadStatusNovo <= 0)
                    throw new AppException("Status 'NOVO' não encontrado para leads");

                var novoLead = new Domain.Entities.Lead.Lead(whatsappNumero, leadStatusNovo, null, canal.OrigemPadraoId, canal.EmpresaId, apelido);

                await _leadRepository.CreateAsync(novoLead);
                await _unitOfWork.SaveChangesAsync();

                return novoLead;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar novo lead. WhatsApp: {WhatsApp}, CanalId: {CanalId}",
                    whatsappNumero, canalId);
                throw;
            }
        }

        public async Task AtualizarEnderecoLeadAsync(int leadId, Endereco endereco, bool isComercial)
        {
            try
            {
                if (endereco == null)
                    throw new ArgumentNullException(nameof(endereco));

                var lead = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(leadId);
                if (lead == null)
                    throw new AppException($"Lead com ID {leadId} não encontrado.");

                await _unitOfWork.BeginTransactionAsync();

                int enderecoId = await _enderecoWriterService.CriarEnderecoAsync(endereco);

                if (isComercial)
                {
                    lead.AdicionarEnderecoComercial(enderecoId);
                }
                else
                {
                    lead.AdicionarEnderecoResidencial(enderecoId);
                }

                _leadRepository.Update(lead);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar endereço do lead com ID {leadId}", leadId);
                throw;
            }
        }

        public async Task RemoverEnderecoLeadAsync(int leadId, int enderecoId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(leadId);
                if (lead == null)
                    throw new AppException($"Lead com ID {leadId} não encontrado.");

                await _unitOfWork.BeginTransactionAsync();

                if (lead.EnderecoResidencialId == enderecoId)
                {
                    lead.RemoverEnderecoResidencial();
                }
                else if (lead.EnderecoComercialId == enderecoId)
                {
                    lead.RemoverEnderecoComercial();
                }
                else
                {
                    throw new AppException("O endereço informado não está associado a este lead.");
                }

                await _enderecoWriterService.ExcluirEnderecoAsync(enderecoId);

                _leadRepository.Update(lead);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, $"Erro ao remover endereço do lead com ID {leadId}");
                throw;
            }
        }

        public async Task AtualizarResponsavel(int leadId, int novoResponsavelId, int equipeId, int empresaId)
        {
            try
            {
                var leadBanco = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(leadId)
                    ?? throw new AppException($"Lead com ID {leadId} não encontrado no banco de dados.");

                if (leadBanco.ResponsavelId == novoResponsavelId &&
                    leadBanco.EquipeId == equipeId &&
                    leadBanco.EmpresaId == empresaId)
                {
                    throw new AppException("O lead já está atribuído a este responsável, equipe e empresa.");
                }

                var membro = await _membroRepo.GetByIdComStatusAsync(novoResponsavelId)
                    ?? throw new AppException("Membro não encontrado ou inativo.");

                if (!string.Equals(membro.StatusMembroEquipe.Codigo, "ATIVO", StringComparison.OrdinalIgnoreCase))
                    throw new AppException("Não é permitido transferir o lead para um membro inativo.");

                //Atualiza os vínculos
                leadBanco.AtribuirResponsavel(novoResponsavelId, equipeId, empresaId);

                _leadRepository.Update(leadBanco);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir lead com ID {LeadId}", leadId);
                throw;
            }
        }

        public async Task AtualizarResponsavelSemNotificar(int leadId, int novoResponsavelId, int equipeId, int empresaId)
        {
            try
            {
                var leadBanco = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(leadId)
                    ?? throw new AppException($"Lead não encontrado.");

                leadBanco.AtribuirResponsavel(novoResponsavelId, equipeId, empresaId);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir lead com ID {LeadId}", leadId);
                throw;
            }
        }

        public async Task AtribuirSomenteEquipe(int leadId, int equipeId)
        {
            try
            {
                var leadBanco = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(leadId) ?? throw new AppException($"Lead não encontrado.");
                leadBanco.AtribuirSomenteEquipe(equipeId);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar equipe do lead com ID {leadId} para equipe {novaEquipeId}", leadId, equipeId);
                throw;
            }
        }

        public async Task AlterarNomeLeadIdAsync(int id, string novoNome)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(id) ??
                        throw new AppException($"Lead com ID {id} não encontrado.");

                await _unitOfWork.BeginTransactionAsync();
                lead.AlterarNomeLead(novoNome);
                _leadRepository.Update(lead);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar nome do lead {id}", id);
                throw;
            }
        }

        public async Task<Domain.Entities.Lead.Lead> CreateLeadRapidoAsync(LeadRapidoDTO dto, int usuarioId)
        {
            Domain.Entities.Lead.Lead? novoLead = null;

            try
            {
                //EMPRESA
                var empresa = await _empresaReaderService.EmpresaExistsAsync(dto.EmpresaId);
                if (!empresa)
                    throw new AppException($"Empresa com ID igual a {dto.EmpresaId} não existe.");

                var vinculo = await _usuarioEmpresaReaderService.GetEquipePadraoByUsuarioEmpresaAsync(usuarioId, dto.EmpresaId)
                    ?? throw new AppException("Usuário não possui vínculo com esta empresa.");

                if (vinculo.EquipePadraoId is null)
                    throw new AppException("Usuário não possui equipe padrão definida para esta empresa.");

                var membros = await _membroEquipeReaderService.ObterMembrosPorUsuarioAsync(usuarioId, vinculo.EquipePadraoId.Value);
                var membro = membros.FirstOrDefault()
                    ?? throw new AppException("Usuário não é membro da sua equipe padrão.");

                //ORIGEM
                var origem = await _origemReaderService.GetOrigemByName("Fluxo de loja")
                    ?? throw new AppException("Origem 'Fluxo de loja' não encontrada.");

                //STATUS
                int leadStatusNovo = await _leadRepository.GetLeadStatusId("NOVO");
                if (leadStatusNovo <= 0)
                    throw new AppException("Status 'NOVO' não encontrado para leads");

                // Normalizar WhatsApp
                var whatsappNormalizado = dto.WhatsappNumero != null
                    ? NormalizarWhatsApp(dto.WhatsappNumero)
                    : null;

                if (whatsappNormalizado is not null &&
                   (whatsappNormalizado.Length != 12 &&
                    whatsappNormalizado.Length != 13))
                    throw new AppException("Número de WhatsApp inválido.");

                var grupoEmpresas = await _empresaReaderService.GetGrupoEmpresaByEmpresaId(dto.EmpresaId);
                var leadNoMesmoGrupo = await _leadRepository.ObterLeadExistenteNoMesmoGrupo(whatsappNormalizado, null, null, grupoEmpresas.Id);

                var leadDoProprioUsuario = leadNoMesmoGrupo != null && leadNoMesmoGrupo.Responsavel.UsuarioId == usuarioId;

                if (leadNoMesmoGrupo != null && !leadDoProprioUsuario)
                    throw new AppException($"Não é possível criar o lead, pois ele já está em atendimento na filial {leadNoMesmoGrupo.Empresa.Nome} sob responsabilidade do(a) vendedor(a) {leadNoMesmoGrupo.Responsavel.Usuario.Nome}. Para prosseguir, solicite a transferência do atendimento para o seu gerente.");

                if (leadDoProprioUsuario)
                {
                    novoLead = leadNoMesmoGrupo!;
                    await _unitOfWork.BeginTransactionAsync();
                    string obsEventoExistente = $"Lead já existente acessado via fluxo de loja por {membro.Usuario.Nome}";
                    await _leadEventoWriterService.RegistrarEventoAsync(leadNoMesmoGrupo, null, obsEventoExistente, origem.Id);
                }
                else
                {
                    novoLead = new Domain.Entities.Lead.Lead(
                        dto.Nome,
                        leadStatusNovo,
                        membro.Id,
                        vinculo.EquipePadraoId.Value,
                        origem.Id,
                        dto.EmpresaId,
                        whatsappNormalizado,
                        null,  // email
                        null,  // telefone
                        null,  // cargo
                        null,  // cpf
                        null,  // genero
                        null,  // cnpjEmpresa
                        null,  // nomeEmpresa
                        null,  // nivelInteresse
                        null,  // observacoesCadastrais
                        null,  // dataNascimento
                        null,  // enderecoResidencialId
                        null   // enderecoComercialId
                    );

                    await _unitOfWork.BeginTransactionAsync();
                    await _leadRepository.CreateAsync(novoLead);
                    await _unitOfWork.SaveChangesAsync();

                    string obsEvento = $"Lead criado rapidamente via fluxo de loja por {membro.Usuario.Nome}";
                    await _leadEventoWriterService.RegistrarEventoAsync(novoLead, null, obsEvento, origem.Id);
                }

                var conversaAtiva = await _conversaReaderService.GetConversaByLead(novoLead.Id, "ENCERRADA");
                if (conversaAtiva == null && novoLead.WhatsappNumero != null)
                {
                    var canal = await _canalReaderService.GetCanalByIdAsync(vinculo.CanalPadraoId);
                    if (canal != null)
                    {
                        var template = await _templateReaderService.GetTemplateByOrigem(origem.Id, canal.Id);
                        if (template != null)
                        {
                            var conversaId = await _conversaWriterService.GetConversaByLeadAndCanalAsync(
                                novoLead.Id,
                                membro.UsuarioId,
                                canal.Id,
                                "ATIVA",
                                vinculo.EquipePadraoId.Value,
                                false,
                                true);

                            var mensagem = await _mensagemWriterService.ProcessarMensagemEnvioTemplateIntegracaoAsync(
                                "text",
                                conversaId,
                                membro.UsuarioId,
                                novoLead,
                                canal,
                                template.Id,
                                null);

                            await _conversaWriterService.UpdateDataUltimaMensagemAsync(conversaId, mensagem.DataCriacao);
                            await _unitOfWork.CommitAsync();

                            await _notificacaoClient.NovaMensagem(new NotificarNovaMensagemDTO
                            {
                                MensagemId = mensagem.Id,
                                UsuarioId = membro.UsuarioId,
                                Titulo = novoLead.Nome,
                                MensagemSincronizacao = new MensagemDTO
                                {
                                    MensagemId = mensagem.Id,
                                    Conteudo = mensagem.Conteudo,
                                    TipoMensagem = mensagem.Tipo.Codigo,
                                    DataEnvio = mensagem.DataEnvio!.Value,
                                    TipoRemetente = mensagem.Sentido,
                                    LeadId = novoLead.Id,
                                    UsuarioId = membro.UsuarioId
                                }
                            });
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Nenhum template encontrado para OrigemId {OrigemId} e CanalId {CanalId}. LeadId {LeadId}.",
                                origem.Id, canal.Id, novoLead.Id);
                        }
                    }
                }

                await _unitOfWork.CommitAsync();

                if (leadDoProprioUsuario)
                {
                    await _notificacaoClient.NovoLeadEvento(new NotificarNovoLeadDTO
                    {
                        LeadId = leadNoMesmoGrupo.Id,
                        UsuarioId = membro.UsuarioId
                    });
                }
                else
                {
                    await _notificacaoClient.NovoLead(new NotificarNovoLeadDTO
                    {
                        LeadId = novoLead.Id,
                        UsuarioId = membro.UsuarioId
                    });
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro inesperado ao criar lead rápido");
                throw;
            }

            return novoLead!;
        }
    }
}
