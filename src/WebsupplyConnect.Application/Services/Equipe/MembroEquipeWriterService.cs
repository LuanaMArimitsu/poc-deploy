using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Application.Services.Usuario;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Equipe
{
    public class MembroEquipeWriterService(
        IMembroEquipeRepository membroRepo,
        IEquipeRepository equipeRepo,
        ILeadRepository leadRepo,
        IUsuarioReaderService usuarioReaderService,
        ILeadReaderService leadReaderService,
        IStatusMembroEquipeReadService statusRead,
        IUnitOfWork uow,
        IFilaDistribuicaoService filaDistribuicaoService,
        IUsuarioEmpresaReaderService usuarioEmpresaReaderService,
        ILogger<MembroEquipeWriterService> logger) : IMembroEquipeWriterService
    {
        private readonly IMembroEquipeRepository _membroRepo = membroRepo ?? throw new ArgumentNullException(nameof(membroRepo));
        private readonly IEquipeRepository _equipeRepo = equipeRepo ?? throw new ArgumentNullException(nameof(equipeRepo));
        private readonly ILeadRepository _leadRepo = leadRepo ?? throw new ArgumentNullException(nameof(leadRepo));
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
        private readonly IStatusMembroEquipeReadService _statusRead = statusRead ?? throw new ArgumentNullException(nameof(statusRead));
        private readonly ILeadReaderService _leadReaderService = leadReaderService ?? throw new ArgumentNullException(nameof(leadReaderService));
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<MembroEquipeWriterService> _logger = logger;
        private readonly IFilaDistribuicaoService _filaDistribuicaoService = filaDistribuicaoService ?? throw new ArgumentNullException(nameof(filaDistribuicaoService));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService ?? throw new ArgumentNullException(nameof(usuarioEmpresaReaderService));

        public async Task<int> AddMembroAsync(int equipeId, AdicionarMembroDto dto)
        {
            var equipe = await _equipeRepo.GetByIdAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(equipeId);
            if (equipe is null)
                throw new DomainException("Equipe não encontrada.");

            if (!await _usuarioReaderService.UserExistsAsync(dto.UsuarioId))
                throw new DomainException("Usuário não encontrado.");

            var usuariosDaEmpresa = await _usuarioReaderService.UsuariosEmpresa(equipe.EmpresaId);
            var pertence = usuariosDaEmpresa?.Any(u => u.Id == dto.UsuarioId) == true;
            if (!pertence)
                throw new DomainException("Usuário não pertence à empresa da equipe.");

            await _statusRead.StatusExisteAsync(dto.StatusMembroEquipeId);

            if (await _membroRepo.ExistsAtivoAsync(equipeId, dto.UsuarioId))
                throw new DomainException("Este usuário já é membro ativo desta equipe.");

            if (dto.IsLider && await _membroRepo.ExistsLiderAtivoAsync(equipeId))
                throw new DomainException("Já existe um líder ativo nesta equipe.");

            var membroDesativado = await _membroRepo.GetByPredicateAsync<MembroEquipe>(m => m.EquipeId == equipeId && m.UsuarioId == dto.UsuarioId && m.Excluido == true, true);

            MembroEquipe membro;
            await _uow.BeginTransactionAsync();
            try
            {

                var statusAtivo = await _statusRead.GetStatusMembro(codigoStatus: "ATIVO");
                var isAtivo = dto.StatusMembroEquipeId == statusAtivo.Id;

                if (membroDesativado is null)
                {
                    membro = new MembroEquipe(
                        equipeId: equipeId,
                        usuarioId: dto.UsuarioId,
                        statusMembroEquipeId: dto.StatusMembroEquipeId,
                        isLider: dto.IsLider,
                        observacoes: string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes!.Trim()
                    );
                    await _membroRepo.CreateAsync(membro);
                    await _uow.SaveChangesAsync();

                    if (isAtivo && !dto.IsLider)
                        await _filaDistribuicaoService.InicializarPosicaoFilaVendedorAsync(membro.Id, equipe.EmpresaId);
                }
                else
                {
                    membro = membroDesativado;
                    membro.Reativar(
                        statusMembroEquipeId: dto.StatusMembroEquipeId,
                        isLider: dto.IsLider,
                        observacoes: string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes!.Trim()
                    );
                    _membroRepo.Update(membro);
                    await _uow.SaveChangesAsync();

                    if (!dto.IsLider)
                    {
                        var isFilaDistribuicao = await _filaDistribuicaoService.ObterPosicaoVendedorExcluidoAsync(equipe.EmpresaId, membro.Id);
                        if (isFilaDistribuicao == null && isAtivo)
                        {
                            await _filaDistribuicaoService.InicializarPosicaoFilaVendedorAsync(membro.Id, equipe.EmpresaId);
                        }
                        else
                        {
                            await _filaDistribuicaoService.RestaurarVendedorNaFilaAsync(equipe.EmpresaId, membro.Id);
                        }

                        var codigoFila = isAtivo ? "ATIVO" : "PAUSADO";
                        var statusFila = await _filaDistribuicaoService.ObterStatusFilaPorCodigoAsync(codigoFila);
                        await _filaDistribuicaoService.AtualizarStatusVendedorAsync(equipe.EmpresaId, membro.Id, statusFila);
                    }

                }

                await _uow.CommitAsync();
                return membro.Id;
            }
            catch
            {
                await _uow.RollbackAsync();
                _logger.LogError("Erro ao adicionar membro à equipe {EquipeId}", equipeId);
                throw;
            }
        }

        public async Task<int> AtualizarStatusAsync(AtualizarMembroEquipeDto dto)
        {
            if (dto is null) throw new DomainException("Payload inválido.");
            if (dto.MembroId <= 0) throw new DomainException("MembroId inválido.");
            if (dto.StatusMembroEquipeId <= 0) throw new DomainException("StatusId inválido.");
            await _statusRead.StatusExisteAsync(dto.StatusMembroEquipeId);

            var membro = await _membroRepo.GetByIdAsync<MembroEquipe>(dto.MembroId, includeDeleted: true) ?? throw new DomainException("Membro não encontrado.");
            if (membro.Excluido)
                throw new DomainException("Não é possível atualizar o status: o vínculo está excluído.");

            var equipe = await _equipeRepo.GetByIdAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(membro.EquipeId) ?? throw new DomainException("Equipe não encontrada.");

            var statusAtivo = await _statusRead.GetStatusMembro(codigoStatus: "ATIVO");
            var idStatusAtivo = statusAtivo.Id;

            var possuiLeads = await _leadRepo.ExisteLeadAtribuidoAsync(membro.EquipeId, membro.Id);
            if (possuiLeads && dto.StatusMembroEquipeId != idStatusAtivo)
            {
                throw new DomainException("Não é possível desativar este membro, pois ele possui leads atribuídos. Transfira os leads antes de alterar o status.");
            }

            var ehUltimoAtivo = await _membroRepo.EhUltimoAtivoAsync(membro.EquipeId, membro.Id);

            if (ehUltimoAtivo && dto.StatusMembroEquipeId != idStatusAtivo)
            {
                throw new DomainException("Este membro não pode ser desativado, pois é o único ativo na equipe. Para desativá-lo, primeiro ative outro membro.");
            }

            try
            {
                var vendedorExisteNaFila = await _filaDistribuicaoService.ObterPosicaoVendedorAsync(equipe.EmpresaId, membro.Id);

                await _uow.BeginTransactionAsync();
                if (dto.StatusMembroEquipeId != idStatusAtivo && vendedorExisteNaFila != null)
                {
                    var statusPausado = await _filaDistribuicaoService.ObterStatusFilaPorCodigoAsync("PAUSADO");
                    await _filaDistribuicaoService.AtualizarStatusVendedorAsync(equipe.EmpresaId, membro.Id, statusPausado);
                }
                else if (dto.StatusMembroEquipeId == idStatusAtivo && vendedorExisteNaFila == null)
                {
                    await _filaDistribuicaoService.InicializarPosicaoFilaVendedorAsync(membro.Id, equipe.EmpresaId);
                }
                else if (dto.StatusMembroEquipeId == idStatusAtivo && vendedorExisteNaFila != null)
                {
                    var statusAtivoFila = await _filaDistribuicaoService.ObterStatusFilaPorCodigoAsync("ATIVO");
                    await _filaDistribuicaoService.AtualizarStatusVendedorAsync(equipe.EmpresaId, membro.Id, statusAtivoFila);
                }

                membro.AlterarStatus(dto.StatusMembroEquipeId);
                _membroRepo.Update(membro);
                await _uow.CommitAsync();
                return membro.StatusMembroEquipeId;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<(int? LiderAnteriorMembroId, int NovoLiderMembroId)> TransferirLiderancaAsync(TransferirLiderancaRequestDto dto)
        {
            if (dto is null) throw new DomainException("Payload inválido.");
            if (dto.EquipeId <= 0) throw new DomainException("EquipeId inválido.");
            if (dto.NovoResponsavelMembroId <= 0) throw new DomainException("NovoLiderMembroId inválido.");

            var novo = await _membroRepo.GetByIdAsync<MembroEquipe>(dto.NovoResponsavelMembroId);

            if (novo is null)
                throw new DomainException("Membro não encontrado");

            if (novo.DataSaida != null)
                throw new DomainException("Apenas membros ativos podem ser líderes.");

            if (novo.EquipeId != dto.EquipeId)
                throw new DomainException("O membro informado não pertence à equipe indicada.");

            if (novo.IsLider && novo.DataSaida is null && !novo.Excluido)
                throw new DomainException("Este membro já é o líder desta equipe.");

            var statusAtivo = await _statusRead.GetStatusMembro(codigoStatus: "ATIVO");

            if (novo.StatusMembroEquipeId != statusAtivo.Id)
                throw new DomainException("Apenas membros com status ativo podem ser líderes.");

            var liderAtual = await _membroRepo.GetByPredicateAsync<MembroEquipe>(
                m => m.EquipeId == dto.EquipeId
                  && m.IsLider
                  && m.DataSaida == null
                  && !m.Excluido
                  && m.Id != novo.Id);

            await _uow.BeginTransactionAsync();
            try
            {
                if (liderAtual is not null)
                {
                    liderAtual.RemoverLideranca();
                    await _uow.SaveChangesAsync();
                }

                novo.DefinirComoLider();

                var equipe = await _equipeRepo.GetByIdAsync<WebsupplyConnect.Domain.Entities.Equipe.Equipe>(dto.EquipeId);
                if (equipe is null)
                    throw new DomainException("Equipe não encontrada.");

                equipe.DefinirResponsavel(novo.Id);
                _equipeRepo.Update(equipe);

                await _uow.SaveChangesAsync();

                var isAtivo = liderAtual.StatusMembroEquipeId == statusAtivo.Id;
                var isFilaDistribuicao = await _filaDistribuicaoService.ObterPosicaoVendedorExcluidoAsync(equipe.EmpresaId, liderAtual!.Id);
                if (isFilaDistribuicao == null && isAtivo)
                {
                    await _filaDistribuicaoService.InicializarPosicaoFilaVendedorAsync(liderAtual.Id, equipe.EmpresaId);
                }
                else if (isAtivo && isFilaDistribuicao != null)
                {
                    var statusAtivoFila = await _filaDistribuicaoService.ObterStatusFilaPorCodigoAsync("ATIVO");
                    await _filaDistribuicaoService.AtualizarStatusVendedorAsync(equipe.EmpresaId, liderAtual!.Id, statusAtivoFila);
                }

                await _uow.CommitAsync();

                return (liderAtual?.Id, novo.Id);
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<(int quantidadeRemovidos, List<MembroEquipe> membrosRemovidos)> DeleteTodosDaEquipeAsync(int equipeId)
        {
            var equipe = await _equipeRepo.GetByIdAsync<Domain.Entities.Equipe.Equipe>(equipeId);
            if (equipe is null)
                throw new DomainException("Equipe não encontrada.");

            var existeLead = await _leadReaderService.ExisteLeadAtribuidoAsync(equipeId);
            if (existeLead)
                throw new DomainException("Há leads atribuídos a membros da equipe.");

            var (quantidadeRemovidos, membrosRemovidos) = await _membroRepo.SoftDeleteAllByEquipeAsync(equipeId);
            await _uow.SaveChangesAsync();

            return (quantidadeRemovidos, membrosRemovidos);
        }

        public async Task DeleteMembroAsync(int membroId)
        {
            var membro = await _membroRepo.GetByIdAsync<MembroEquipe>(membroId);
            if (membro is null)
                throw new DomainException("Membro da equipe não encontrado.");

            if (membro.IsLider && membro.DataSaida is null)
                throw new DomainException("Não é permitido excluir o líder da equipe. Transfira a liderança antes de removê-lo.");

            var existeLead = await _leadRepo.ExisteLeadAtribuidoAsync(membro.EquipeId, membro.Id);
            if (existeLead)
                throw new DomainException("Há leads atribuídos a este membro. Transfira os leads antes de removê-lo.");

            var equipe = await _equipeRepo.GetByIdAsync<Domain.Entities.Equipe.Equipe>(membro.EquipeId);

            var equipePadrao = await _usuarioEmpresaReaderService.GetEquipePadraoByUsuarioEmpresaAsync(membro.UsuarioId, equipe.EmpresaId);
            if (equipePadrao?.EquipePadraoId == membro.EquipeId)
                throw new DomainException("Esta equipe é a equipe padrão do usuário. Defina outra equipe padrão antes de removê-lo.");

            var isFilaDistribuicao = await _filaDistribuicaoService.ObterPosicaoVendedorAsync(equipe.EmpresaId, membroId);

            if (isFilaDistribuicao != null)
                await _filaDistribuicaoService.RemoverVendedorFilaAsync(equipe!.EmpresaId, membroId);

            await _uow.BeginTransactionAsync();
            try
            {
                await _membroRepo.SoftDeleteAsync(membro);
                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                _logger.LogError("Erro ao excluir o membro da equipe {MembroId}", membroId);
                throw;
            }
        }

        public async Task<MembroEquipe> GetMembroEquipePorEmail(string email, int empresaId, string? statusCodigo = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Para recuperar o membro com o e-mail é necessário que esse valor não seja nulo.");

            var membro = await _membroRepo.ObterMembroPorEmail(email, empresaId, statusCodigo);
            if (membro is null)
            {
                var usuario = await _usuarioReaderService.GetUsuarioByEmail(email);
                if (usuario is null)
                {
                    var equipeIntegracao = await _equipeRepo.GetEquipeIntegracaoPorEmpresaIdAsync(empresaId);
                    var responsavelEquipe = await _membroRepo.GetLiderAtivoByEquipeAsync(equipeIntegracao!.Id);
                    return responsavelEquipe!;
                }
                var membroNovo = new MembroEquipe(
                    equipeId: (await _equipeRepo.GetEquipeIntegracaoPorEmpresaIdAsync(empresaId))!.Id,
                    usuarioId: usuario.Id,
                    statusMembroEquipeId: (await _statusRead.GetStatusMembro(codigoStatus: "ATIVO")).Id,
                    isLider: false,
                    observacoes: "Membro criado automaticamente ao receber lead para e-mail não cadastrado na equipe de integração."
                );

                await _membroRepo.CreateAsync(membroNovo);
                await _uow.SaveChangesAsync();

                return membroNovo;
            };
            return membro;
        }
    }
}
