using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Equipe
{
    public class MembroEquipeRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IMembroEquipeRepository
    {
        public Task<bool> ExistsAtivoAsync(int equipeId, int usuarioId)
            => _context.MembrosEquipe.AsNoTracking()
                .AnyAsync(m => !m.Excluido
                            && m.EquipeId == equipeId
                            && m.UsuarioId == usuarioId
                            && m.DataSaida == null);

        public Task<bool> ExistsLiderAtivoAsync(int equipeId)
            => _context.MembrosEquipe.AsNoTracking()
                .AnyAsync(m => !m.Excluido
                            && m.EquipeId == equipeId
                            && m.IsLider
                            && m.DataSaida == null);

        public async Task<bool> EhUltimoAtivoAsync(int equipeId, int membroId)
        {
            // Conta quantos membros ativos existem na equipe (ignorando excluídos)
            var ativos = await _context.MembrosEquipe.AsNoTracking()
                .CountAsync(m => !m.Excluido
                             && m.EquipeId == equipeId
                             && m.DataSaida == null
                             && m.StatusMembroEquipe != null
                             && m.StatusMembroEquipe.Codigo == "ATIVO");

            // Se só há 1 ativo, verificar se é o membro que está sendo alterado
            if (ativos == 1)
            {
                var membroAtivo = await _context.MembrosEquipe.AsNoTracking()
                    .FirstOrDefaultAsync(m => !m.Excluido
                                           && m.EquipeId == equipeId
                                           && m.DataSaida == null
                                           && m.StatusMembroEquipe != null
                                           && m.StatusMembroEquipe.Codigo == "ATIVO");

                if (membroAtivo != null && membroAtivo.Id == membroId)
                    return true;
            }

            return false;
        }

        public async Task<MembroEquipe?> GetLiderDaEquipeAsync(int equipeId)
        {
            return await _context.MembrosEquipe
                .AsNoTracking()
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(m => m.IsLider &&
                                          m.EquipeId == equipeId &&
                                          !m.Excluido);
        }

        public async Task<Dictionary<int, MembroEquipe>> GetLideresPorEquipesAsync(List<int> equipeIds)
        {
            return await _context.MembrosEquipe
                .Include(m => m.Usuario)
                .Where(m => equipeIds.Contains(m.EquipeId) && m.IsLider)
                .ToDictionaryAsync(x => x.EquipeId, x => x);
        }

        public async Task<MembroEquipe?> GetMembroPorUsuario(int usuarioId, int equipeId)
        {
            return await _context.MembrosEquipe
                .Include(m => m.StatusMembroEquipe)
                .Include(u => u.Usuario)
                   .Where(m => m.UsuarioId == usuarioId && m.EquipeId == equipeId)
                   .FirstOrDefaultAsync();
        }

        public async Task<(IReadOnlyList<MembroEquipe> Itens, int Total)> ListarMembrosAsync(
            int equipeId,
            bool apenasAtivos,
            IEnumerable<int>? statusIds,
            string? buscaNome,
            int? pagina = null,
            int? tamanho = null)
        {
            var query = _context.MembrosEquipe
                    .AsNoTracking()
                    .Where(m => m.EquipeId == equipeId && !m.Excluido);

            query = query.Where(m => m.Usuario != null && !m.Usuario.IsBot);

            if (apenasAtivos)
                query = query.Where(m => m.DataSaida == null);

            if (statusIds is not null && statusIds.Any())
            {
                var set = statusIds.ToHashSet();
                query = query.Where(m => set.Contains(m.StatusMembroEquipeId));
            }

            if (!string.IsNullOrWhiteSpace(buscaNome))
            {
                var like = buscaNome.Trim();
                query = query.Where(m => m.Usuario != null &&
                                 EF.Functions.Like(m.Usuario.Nome, $"%{like}%"));
            }

            var total = await query.CountAsync();

            query = query
                .Include(m => m.Usuario)
                .Include(m => m.StatusMembroEquipe)
                .OrderByDescending(m => m.IsLider)
                .ThenBy(m => m.Usuario!.Nome);

            if ((pagina.HasValue && pagina > 0) && (tamanho.HasValue && tamanho > 0))
            {
                query = query
                    .Skip((pagina.Value - 1) * tamanho.Value)
                    .Take(tamanho.Value);
            }

            var itens = await query.ToListAsync();

            return (itens, total);
        }

        public async Task<MembroEquipe?> GetLiderAtivoByEquipeAsync(int equipeId, int? ignoreMembroId = null)
        {
            var query = _context.MembrosEquipe
                .Where(m => m.EquipeId == equipeId &&
                            m.IsLider &&
                            m.DataSaida == null &&
                            !m.Excluido);

            if (ignoreMembroId.HasValue)
                query = query.Where(m => m.Id != ignoreMembroId.Value);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<int> CountAtivosByEquipeAsync(int equipeId)
        {
            return await _context.MembrosEquipe
                .AsNoTracking()
                .CountAsync(m => m.EquipeId == equipeId && !m.Excluido);
        }

        public async Task<(int quantidadeRemovidos, List<MembroEquipe> membrosRemovidos)> SoftDeleteAllByEquipeAsync(int equipeId)
        {
            var membros = await _context.MembrosEquipe
                .Where(m => m.EquipeId == equipeId && !m.Excluido)
                .ToListAsync();

            foreach (var m in membros)
            {
                m.ExcluirLogicamente();
                m.DefinirDataSaida();
            }

            // UoW fará o Commit
            return (membros.Count, membros);
        }

        public Task SoftDeleteAsync(MembroEquipe membro)
        {
            membro.ExcluirLogicamente();
            membro.DefinirDataSaida();
            _context.MembrosEquipe.Update(membro);
            return Task.CompletedTask;
        }

        public Task RestaurarMembro(MembroEquipe membro)
        {
            membro.RestaurarExclusaoLogica();
            membro.RemoverDataSaida();
            _context.MembrosEquipe.Update(membro);
            return Task.CompletedTask;
        }


        public async Task<MembroEquipe?> ObterMembroPorEmail(string email, int empresaId, string? statusCodigo = null)
        {
            if (statusCodigo is not null)
            {
                statusCodigo = statusCodigo.Trim().ToUpper();
                return await _context.MembrosEquipe
                    .Include(m => m.Usuario)
                    .Include(m => m.Equipe)
                    .ThenInclude(e => e!.Empresa)
                    .Include(m => m.StatusMembroEquipe)
                    .FirstOrDefaultAsync(m => !m.Excluido
                                              && m.Usuario != null
                                              && m.Usuario.Email == email
                                              && m.Equipe != null
                                              && m.Equipe.EmpresaId == empresaId
                                              && m.StatusMembroEquipe != null
                                              && m.StatusMembroEquipe.Codigo == statusCodigo);
            }

            return await _context.MembrosEquipe
                .Include(m => m.Usuario)
                .Include(m => m.Equipe)
                .ThenInclude(e => e!.Empresa)
                .FirstOrDefaultAsync(m => !m.Excluido
                                          && m.Usuario != null
                                          && m.Usuario.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase)
                                          && m.Equipe != null
                                          && m.Equipe.EmpresaId == empresaId);
        }

        /// <summary>
        /// Obtém todos os vendedores ativos de uma Equipe
        /// </summary>
        public async Task<List<MembroEquipe>> ObterMembrosPorEquipeAsync(int equipeId, string? statusCodigo = null, int? statusId = null)
        {
            if (statusCodigo is not null && statusId.HasValue && statusId.Value > 0)
                throw new ArgumentException("Apenas um dos parâmetros statusCodigo ou statusId deve ser fornecido.");

            if (statusCodigo is not null)
            {
                statusCodigo = statusCodigo.Trim().ToUpper();
                return await _context.Set<MembroEquipe>()
                    .Include(m => m.Usuario)
                    .Include(m => m.StatusMembroEquipe)
                    .Where(u => !u.Excluido
                                && u.Usuario.IsBot == false &&
                                 u.IsLider == false
                                && u.EquipeId == equipeId
                                && u.StatusMembroEquipe != null
                                && u.StatusMembroEquipe.Codigo == statusCodigo)
                    .ToListAsync();
            }

            if (statusId.HasValue && statusId.Value > 0)
            {
                return await _context.Set<MembroEquipe>()
                    .Include(m => m.Usuario)
                    .Include(m => m.StatusMembroEquipe)
                    .Where(u => !u.Excluido
                                && u.Usuario.IsBot == false &&
                                 u.IsLider == false
                                && u.EquipeId == equipeId
                                && u.StatusMembroEquipeId == statusId.Value)
                    .ToListAsync();
            }

            return await _context.Set<MembroEquipe>()
                .Include(m => m.Usuario)
                .Where(u => !u.Excluido && u.IsLider == false && u.EquipeId == equipeId && u.Usuario.IsBot == false)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<MembroEquipe>> ObterVendedoresAtivosPorEquipeIdsAsync(IReadOnlyList<int> equipeIds)
        {
            if (equipeIds == null || equipeIds.Count == 0)
                return [];

            return await _context.Set<MembroEquipe>()
                .AsNoTracking()
                .Include(m => m.Usuario)
                .Include(m => m.StatusMembroEquipe)
                .Where(u => !u.Excluido
                            && u.Usuario != null
                            && u.Usuario.IsBot == false
                            && equipeIds.Contains(u.EquipeId)
                            && u.StatusMembroEquipe != null
                            && u.StatusMembroEquipe.Codigo == "ATIVO")
                .ToListAsync();
        }

        public async Task<MembroEquipe?> GetByIdComStatusAsync(int membroId)
        {
            return await _context.MembrosEquipe
                .Include(m => m.StatusMembroEquipe)
                .Include(m => m.Usuario)
                .Include(m => m.Equipe)
                .FirstOrDefaultAsync(m => m.Id == membroId && !m.Excluido);
        }

        public async Task<List<MembroEquipe>> ObterVendedoresPorEquipeDisponiveisNoHorarioAsync(int diaSemana, TimeSpan horaAtual, List<int> membrosEquipeIds)
        {
            return await _context.Set<MembroEquipe>()
                .Include(u => u.Usuario)
                .ThenInclude(u => u.HorariosUsuario)
                .Include(u => u.StatusMembroEquipe)
                .Where(u => !u.Excluido &&
                            membrosEquipeIds.Contains(u.Id) &&
                            u.StatusMembroEquipe!.Codigo == "ATIVO" &&
                            u.Usuario != null &&
                            u.IsLider == false &&
                            u.Usuario.HorariosUsuario != null &&
                            u.Usuario.HorariosUsuario.Any(h =>
                                h.DiaSemanaId == diaSemana &&
                                h.HorarioInicio <= horaAtual &&
                                h.HorarioFim >= horaAtual))
                .ToListAsync();
        }

        public async Task<bool> VerificarAssociacaoEquipeAsync(int usuarioId, int empresaId)
        {
            return await _context.MembrosEquipe
                .AnyAsync(m => m.UsuarioId == usuarioId &&
                               m.Equipe.EmpresaId == empresaId &&
                               !m.Excluido);
        }

        public async Task<MembroEquipe?> GetBotMembroEquipeAsync(int usuarioId, int empresaId)
        {
            return await _context.MembrosEquipe
                .Include(m => m.Equipe)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(m => m.UsuarioId == usuarioId &&
                                          m.Equipe!.EmpresaId == empresaId &&
                                          !m.Excluido);
        }
    }
}
