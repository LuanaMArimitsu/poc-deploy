using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Lead
{
    public class LeadEventoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), ILeadEventoRepository
    {
        public async Task<List<LeadEvento>> GetAllAsync()
        {
            return await _context.LeadEvento
                .Include(h => h.Lead)
                .Include(h => h.Origem)
                .Include(h => h.Canal)
                .Include(h => h.Campanha)
                .Where(h => !h.Excluido)
                .OrderByDescending(h => h.DataEvento)
                .ToListAsync();
        }

        public async Task<List<LeadEvento>> GetByLeadIdAsync(int leadId)
        {
            return await _context.LeadEvento
                .Include(h => h.Lead)
                .Include(h => h.Origem)
                .Include(h => h.Canal)
                .Include(h => h.Campanha)
                .Include(h => h.Oportunidades.Where(o => !o.Excluido))
                .Where(h => !h.Excluido && h.LeadId == leadId)
                .OrderByDescending(h => h.DataEvento)
                .ToListAsync();
        }

        public async Task<(List<LeadEvento> Itens, int TotalItens)> ListEventosPorCampanhaAsync(
            int campanhaId,
            int? pagina = null,
            int? tamanhoPagina = null)
        {
            var query = _context.LeadEvento
                .AsNoTracking()
                .Where(h => !h.Excluido && h.CampanhaId == campanhaId);

            var totalItens = await query.CountAsync();

            query = query
                .Include(h => h.Lead)
                .Include(h => h.Origem)
                .Include(h => h.Campanha)
                .OrderByDescending(h => h.DataEvento);

            if (pagina.HasValue && tamanhoPagina.HasValue && pagina > 0 && tamanhoPagina > 0)
            {
                query = query
                    .Skip((pagina.Value - 1) * tamanhoPagina.Value)
                    .Take(tamanhoPagina.Value);
            }

            var itens = await query.ToListAsync();
            return (itens, totalItens);
        }

        public async Task<LeadEvento?> GetLeadEventoByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new AppException("ID do Evento inválido.");

                var evento = await _context.LeadEvento
                    .Include(h => h.Lead)
                    .Include(h => h.Origem)
                    .Include(h => h.Canal)
                    .Include(h => h.Campanha)
                    .Include(h => h.Oportunidades)
                    .Where(h => h.Id == id)
                    .FirstOrDefaultAsync();

                return evento;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
