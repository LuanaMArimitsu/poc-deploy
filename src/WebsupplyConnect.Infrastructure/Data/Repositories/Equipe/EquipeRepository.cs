using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Equipe
{
    public class EquipeRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IEquipeRepository
    {
        public async Task<bool> ExistsNomeNaEmpresaAsync(string nome, int empresaId, int? ignorarEquipeId = null)
        {
            var query = _context.Set<WebsupplyConnect.Domain.Entities.Equipe.Equipe>()
                .AsNoTracking()
                .Where(e => !e.Excluido && e.EmpresaId == empresaId && e.Nome == nome);

            if (ignorarEquipeId.HasValue)
                query = query.Where(e => e.Id != ignorarEquipeId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<Domain.Entities.Equipe.Equipe>> GetByEmpresaAsync(int empresaId)
        {
            return await _context.Equipe
                .AsNoTracking()
                .Where(e => e.EmpresaId == empresaId && !e.Excluido)
                .Include(e => e.TipoEquipe)
                .Include(e => e.Empresa)
                .Include(e => e.ResponsavelMembro)
                 .ThenInclude(m => m.Usuario)
                .Include(e => e.Membros.Where(m => !m.Excluido && m.Usuario != null && !m.Usuario.IsBot)) // para Total/MembrosAtivos projetados no service
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        public async Task<List<Domain.Entities.Equipe.Equipe>> GetByEmpresaWithMembersAsync(int empresaId)
        {
            return await _context.Equipe
                .AsNoTracking()
                .Where(e => e.EmpresaId == empresaId && !e.Excluido)
                .Include(e => e.Membros.Where(m => !m.Excluido && m.StatusMembroEquipe.Codigo == "ATIVO" && m.Usuario != null && !m.Usuario.IsBot))
                .ThenInclude(m => m.Usuario)
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        public async Task<(List<Domain.Entities.Equipe.Equipe> Itens, int TotalItens)> ListarPorEmpresaFiltradoAsync(
            int empresaId,
            int? tipoEquipeId,
            bool? ativas,
            int? responsavelMembroId,
            string? busca,
            int? pagina = null,
            int? tamanhoPagina = null)
        {
            var query = _context.Equipe
                .AsNoTracking()
                .Where(e => e.EmpresaId == empresaId && !e.Excluido);

            if (tipoEquipeId.HasValue)
                query = query.Where(e => e.TipoEquipeId == tipoEquipeId.Value);

            if (ativas.HasValue)
                query = query.Where(e => e.Ativa == ativas.Value);

            if (responsavelMembroId.HasValue)
                query = query.Where(e => e.ResponsavelMembroId == responsavelMembroId.Value);

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.Trim().ToLower();
                query = query.Where(e =>
                    e.Nome.ToLower().Contains(busca) ||
                    (e.Descricao != null && e.Descricao.ToLower().Contains(busca)) ||
                    (e.ResponsavelMembro != null &&
                     e.ResponsavelMembro.Usuario != null &&
                     e.ResponsavelMembro.Usuario.Nome.ToLower().Contains(busca)) ||
                    (e.TipoEquipe != null && e.TipoEquipe.Nome.ToLower().Contains(busca))
                );
            }

            var totalItens = await query.CountAsync();

            query = query
                .Include(e => e.TipoEquipe)
                .Include(e => e.Empresa)
                .Include(e => e.ResponsavelMembro)
                     .ThenInclude(m => m.Usuario)
                .Include(e => e.Membros.Where(m => !m.Excluido && m.Usuario != null && !m.Usuario.IsBot))
                .OrderBy(e => e.Nome);

            if (pagina.HasValue && tamanhoPagina.HasValue && pagina > 0 && tamanhoPagina > 0)
            {
                query = query
                    .Skip((pagina.Value - 1) * tamanhoPagina.Value)
                    .Take(tamanhoPagina.Value);
            }

            var itens = await query.ToListAsync();
            return (itens, totalItens);
        }

        public Task<Domain.Entities.Equipe.Equipe?> GetByIdEquipeDetailsAsync(int equipeId)
        {
            return _context.Equipe
                .AsNoTracking()
                .Include(e => e.Empresa)
                .Include(e => e.TipoEquipe)
                .Include(e => e.ResponsavelMembro)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(e => e.Id == equipeId && !e.Excluido);
        }

        public async Task<List<WebsupplyConnect.Domain.Entities.Equipe.Equipe>> ListEquipesComResponsavelAsync(int empresaId)
        {
            var query = _context.Equipe
                .AsNoTracking()
                .Where(e => !e.Excluido && e.EmpresaId == empresaId)
                .Include(e => e.Empresa)
                .Include(e => e.ResponsavelMembro)
                    .ThenInclude(m => m.Usuario);

            return await query.ToListAsync();
        }

        public async Task<Domain.Entities.Equipe.Equipe?> GetEquipeIntegracaoPorEmpresaIdAsync(int empresaId)
        {
            return await _context.Equipe
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.TipoEquipe!.Codigo == "INTEGRACAO" && EF.Functions.Like(e.Nome, "%GERAIS%") && !e.Excluido);
        }

        public async Task<Domain.Entities.Equipe.Equipe?> ObterEquipeOlxIdAsync(int empresaId)
        {
            return await _context.Equipe
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.Nome == "OLX" && e.TipoEquipe!.Codigo == "INTEGRACAO" && !e.Excluido);
        }

        public async Task<List<Domain.Entities.Equipe.Equipe>> GetEquipeComMembrosByResponsavelAsync(List<int> responsavelMembroId)
        {
            return await _context.Equipe
                .AsNoTracking()
                .Where(e =>
                    e.ResponsavelMembroId.HasValue &&
                    responsavelMembroId.Contains(e.ResponsavelMembroId.Value) &&
                    !e.Excluido
                )
                .Include(e => e.Membros.Where(m => !m.Excluido && m.Usuario != null && !m.Usuario.IsBot))
                    .ThenInclude(m => m.Usuario)
                .Include(e => e.Empresa)
                .ToListAsync();
        }
    }
}
