using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;

namespace WebsupplyConnect.Application.Services.Equipe
{
    public class EquipeWriterService : IEquipeWriterService
    {
        private readonly IEquipeRepository _equipeRepo;
        private readonly IEmpresaReaderService _empresaReaderService;
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly ITipoEquipeReadService _tipoEquipeReadService;
        private readonly IMembroEquipeRepository _membroRepo;
        private readonly IStatusMembroEquipeRepository _statusMembroEquipeRepo;
        private readonly IMembroEquipeWriterService _membroEquipeWriterService;
        private readonly ILeadReaderService _leadReaderService;
        private readonly IFilaDistribuicaoService _filaDistribuicaoService;
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CriarEquipeDto> _validator;
        private readonly ILogger<EquipeWriterService> _logger;

        public EquipeWriterService(
            IEquipeRepository equipeRepo,
            IEmpresaReaderService empresaReaderService,
            IUsuarioReaderService usuarioReaderService,
            ITipoEquipeReadService tipoEquipeReadService,
            IMembroEquipeRepository membroRepo,
            IStatusMembroEquipeRepository statusMembroEquipeRepo,
            IMembroEquipeWriterService membroEquipeWriterService,
            ILeadReaderService leadReaderService,
            IFilaDistribuicaoService filaDistribuicaoService,
            IValidator<CriarEquipeDto> validator,
            IUnitOfWork uow,
            ILogger<EquipeWriterService> logger)
        {
            _equipeRepo = equipeRepo;
            _empresaReaderService = empresaReaderService;
            _usuarioReaderService = usuarioReaderService;
            _tipoEquipeReadService = tipoEquipeReadService;
            _membroRepo = membroRepo;
            _statusMembroEquipeRepo = statusMembroEquipeRepo;
            _membroEquipeWriterService = membroEquipeWriterService;
            _leadReaderService = leadReaderService;
            _filaDistribuicaoService = filaDistribuicaoService;
            _validator = validator;
            _uow = uow;
            _logger = logger;
        }

        public async Task<int> CreateEquipe(CriarEquipeDto dto)
        {
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.ErrorMessage));
                throw new AppException($"Dados inválidos para cadastro de equipe: {errors}");
            }

            if (!await _empresaReaderService.EmpresaExistsAsync(dto.EmpresaId))
                throw new DomainException("A empresa informada não existe.");

            await _tipoEquipeReadService.TipoExisteAsync(dto.TipoEquipeId);

            if (!await _usuarioReaderService.UserExistsAsync(dto.ResponsavelId))
                throw new DomainException("Usuário responsável não encontrado.");

            var nome = dto.Nome?.Trim() ?? string.Empty;
            if (await _equipeRepo.ExistsNomeNaEmpresaAsync(nome, dto.EmpresaId))
                throw new DomainException("Já existe uma equipe com este nome nessa empresa.");



            //TimeSpan? tempoSemAtendimento = null;
            //if (dto.NotificarSemAtendimentoLideres)
            //{
            //    var horas = dto.TempoSemAtendimentoHoras ?? 0;
            //    var minutos = dto.TempoSemAtendimentoMinutos ?? 0;
            //    tempoSemAtendimento = TimeSpan.FromHours(horas) + TimeSpan.FromMinutes(minutos);
            //}

            var entidade = new WebsupplyConnect.Domain.Entities.Equipe.Equipe(
                nome: nome,
                tipoEquipeId: dto.TipoEquipeId,
                empresaId: dto.EmpresaId,
                responsavelMembroId: null, // será definido após criar o membro líder
                descricao: dto.Descricao,
                ativa: true,
                tempoMaxSemAtendimento: TimeSpan.FromMinutes(dto.TempoMaxSemAtendimento)
            //notificarDestinatario: dto.NotificarAtribuicaoAoDestinatario,
            //notificarLideres: dto.NotificarAtribuicaoAosLideres,
            );

            var equipesExistentes = await _equipeRepo.GetListByPredicateAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(
                e => e.EmpresaId == dto.EmpresaId && !e.Excluido
            );
            var ehPrimeiraEquipe = !equipesExistentes.Any();

            if (ehPrimeiraEquipe)
            {
                entidade.DefinirComoPadrao();
            }

            await _uow.BeginTransactionAsync();
            try
            {
                await _equipeRepo.CreateAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(entidade);
                await _uow.SaveChangesAsync();

                var statusAtivo = await _statusMembroEquipeRepo
                    .GetByPredicateAsync<StatusMembroEquipe>(s => s.Codigo == "ATIVO", false);

                if (statusAtivo is null)
                    throw new DomainException("Status 'ATIVO' não encontrado.");

                var membro = new WebsupplyConnect.Domain.Entities.Equipe.MembroEquipe(
                    equipeId: entidade.Id,
                    usuarioId: dto.ResponsavelId,
                    statusMembroEquipeId: statusAtivo.Id,
                    isLider: true
                );

                var novoMembro = await _membroRepo.CreateAsync(membro);
                await _uow.SaveChangesAsync();

                entidade.DefinirResponsavel(membro.Id);
                _equipeRepo.Update(entidade);
                await _uow.SaveChangesAsync();

                if (!ehPrimeiraEquipe && entidade.EhPadrao)
                {
                    var outrasPadrao = await _equipeRepo.GetListByPredicateAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(
                        e => e.EmpresaId == entidade.EmpresaId
                             && !e.Excluido
                             && e.EhPadrao
                             && e.Id != entidade.Id
                    );

                    if (outrasPadrao.Any())
                    {
                        foreach (var eq in outrasPadrao)
                        {
                            eq.RemoverPadrao();
                            _equipeRepo.Update(eq);
                        }

                        await _uow.SaveChangesAsync();
                    }
                }

                await _uow.CommitAsync();
                return entidade.Id;
            }
            catch
            {
                await _uow.RollbackAsync();
                _logger.LogError("Erro ao criar equipe {@Equipe}", dto);
                throw;
            }
        }

        public async Task UpdateEquipeAsync(int id, AtualizarEquipeDto dto)
        {
            var equipe = await _equipeRepo.GetByIdAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(id);
            if (equipe is null)
                throw new DomainException("Equipe não encontrada.");

            bool houveAlteracao = false;

            if (dto.Nome is not null || dto.Descricao is not null)
            {
                var novoNome = dto.Nome?.Trim() ?? equipe.Nome;
                var novaDescricao = dto.Descricao ?? equipe.Descricao;

                if (!string.Equals(novoNome, equipe.Nome, StringComparison.Ordinal) ||
                    !string.Equals(novaDescricao, equipe.Descricao, StringComparison.Ordinal))
                {
                    if (!string.Equals(novoNome, equipe.Nome, StringComparison.Ordinal))
                    {
                        var duplicado = await _equipeRepo.ExistsNomeNaEmpresaAsync(novoNome, equipe.EmpresaId, ignorarEquipeId: id);
                        if (duplicado)
                            throw new DomainException("Já existe uma equipe com este nome nessa empresa.");
                    }

                    equipe.AtualizarInformacoes(novoNome, novaDescricao);
                    houveAlteracao = true;
                }
            }

            if (dto.Ativa.HasValue && dto.Ativa.Value != equipe.Ativa)
            {
                if (dto.Ativa.Value)
                {
                    equipe.Ativar();
                }
                else
                {
                    var existeLead = await _leadReaderService.ExisteLeadAtribuidoAsync(id);
                    if (existeLead)
                        throw new DomainException("Há leads atribuídos a membros da equipe. Transfira os leads antes de desativar a equipe.");

                    equipe.Desativar();

                    // Desativar todos os membros da equipe
                    var statusInativo = await _statusMembroEquipeRepo
                        .GetByPredicateAsync<StatusMembroEquipe>(s => s.Codigo == "INATIVO", false);

                    if (statusInativo is null)
                        throw new DomainException("Status 'INATIVO' não encontrado.");

                    var membrosAtivos = await _membroRepo.GetListByPredicateAsync<MembroEquipe>(
                        m => m.EquipeId == id && !m.Excluido
                    );

                    var statusPausadoId = await _filaDistribuicaoService.ObterStatusFilaPorCodigoAsync("PAUSADO");

                    foreach (var membro in membrosAtivos)
                    {
                        membro.AlterarStatus(statusInativo.Id);
                        await _filaDistribuicaoService.AtualizarStatusVendedorAsync(equipe.EmpresaId, membro.Id, statusPausadoId);
                        _membroRepo.Update(membro);
                    }
                }

                houveAlteracao = true;
            }

            // === Notificações (mantidas comentadas) ===
            //var (notificarDestinatario,
            //     notificarLideres,
            //     slaAtivo,
            //     tempoMaxSemAtendimento)
            //    = NotificacaoEquipeHelper.CalcularNovoEstado(
            //        equipe.NotificarAtribuicaoAoDestinatario,
            //        equipe.NotificarAtribuicaoAosLideres,
            //        equipe.NotificarSemAtendimentoLideres,
            //        equipe.TempoMaxSemAtendimento,
            //        dto.NotificarAtribuicaoAoDestinatario,
            //        dto.NotificarAtribuicaoAosLideres,
            //        dto.NotificarSemAtendimentoLideres,
            //        dto.TempoSemAtendimentoHoras,
            //        dto.TempoSemAtendimentoMinutos
            //    );

            //equipe.AtualizarNotificacoes(
            //    notificarDestinatario,
            //    notificarLideres,
            //    slaAtivo,
            //    tempoMaxSemAtendimento
            //);

            if (dto.EhPadrao.HasValue && dto.EhPadrao.Value != equipe.EhPadrao)
            {
                var equipesDaEmpresa = await _equipeRepo.GetListByPredicateAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(
                    e => !e.Excluido && e.EmpresaId == equipe.EmpresaId
                );

                if (dto.EhPadrao.Value)
                {
                    foreach (var eq in equipesDaEmpresa.Where(e => e.EhPadrao && e.Id != equipe.Id)) //remove o padrão das outras equipes
                    {
                        eq.RemoverPadrao();
                        _equipeRepo.Update(eq);
                    }

                    equipe.DefinirComoPadrao();
                }
                else
                {
                    var existeOutraPadrao = equipesDaEmpresa.Any(e => e.EhPadrao && e.Id != equipe.Id);
                    if (!existeOutraPadrao)
                        throw new AppException("A empresa não pode ficar sem equipe padrão. Selecione outra antes de remover esta.");

                    equipe.RemoverPadrao();
                }

                houveAlteracao = true;
                ;
            }

            if (dto.TempoMaxSemAtendimento > 0 && TimeSpan.FromMinutes(dto.TempoMaxSemAtendimento) != equipe.TempoMaxSemAtendimento)
            {
                if (dto.TempoMaxSemAtendimento <= 0 || dto.TempoMaxSemAtendimento < 5)
                    throw new DomainException("Tempo máximo sem atendimento deve ser no mínimo 5 minutos.");
                equipe.AtualizarTempoSemAtendimento(TimeSpan.FromMinutes(dto.TempoMaxSemAtendimento));
                houveAlteracao = true;
            }

            // Salvar todas as alterações
            if (houveAlteracao)
            {
                await _uow.BeginTransactionAsync();
                try
                {
                    _equipeRepo.Update(equipe);
                    await _uow.SaveChangesAsync();
                    await _uow.CommitAsync();
                }
                catch
                {
                    await _uow.RollbackAsync();
                    _logger.LogError("Erro ao atualizar equipe {@EquipeId} - {@Dto}", id, dto);
                    throw;
                }
            }
        }

        public async Task DeleteEquipeAsync(int id)
        {
            var equipe = await _equipeRepo.GetByIdAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(id);
            if (equipe is null)
                throw new DomainException("Equipe não encontrada.");

            var existeLead = await _leadReaderService.ExisteLeadAtribuidoAsync(id);
            if (existeLead)
                throw new DomainException("Há leads atribuídos a membros da equipe. Transfira os leads antes de excluir a equipe.");

            if (equipe.EhPadrao)
                throw new DomainException("Não é permitido excluir a equipe padrão da empresa.");

            await _uow.BeginTransactionAsync();
            try
            {
                var (quantidadeRemovidos, membrosRemovidos) = await _membroEquipeWriterService.DeleteTodosDaEquipeAsync(id);
                await _filaDistribuicaoService.RemoverTodosVendedorFilaAsync(membrosRemovidos);
                equipe.ExcluirLogicamente();
                _equipeRepo.Update(equipe);
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                _logger.LogError("Erro ao excluir equipe {EquipeId}", id);
                throw;
            }
        }
    }
}
