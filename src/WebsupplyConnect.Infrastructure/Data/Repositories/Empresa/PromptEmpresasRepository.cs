using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Empresa;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Empresa
{
    public class PromptEmpresasRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IPromptEmpresaRepository
    {

        public async Task<PromptEmpresas?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            try
            {
                if (id <= 0)
                    return null;
                var query = _context.Set<PromptEmpresas>().AsQueryable();
                if (!includeDeleted)
                {
                    query = query.Where(x => !x.Excluido);
                }
                return await query.FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw new InfraException($"Erro ao buscar PromptEmpresa por ID: {ex.Message}");
            }
        }

        public async Task<string?> GetPromptAsync(int empresaId, bool sistema, string tipoPrompt, bool includeDeleted = false)
        {
            try
            {
                if (empresaId <= 0)
                    return null;

                if (string.IsNullOrWhiteSpace(tipoPrompt))
                {
                    return null;
                }

                var query = _context.Set<PromptEmpresas>().AsQueryable();
                if (!includeDeleted)
                {
                    query = query.Where(x => !x.Excluido);
                }

                tipoPrompt = tipoPrompt.ToUpperInvariant();

                var prompts = await query.Where(p => p.EmpresaId == empresaId && p.Sistema == sistema)
                    .Include(p => p.TipoPrompt)
                    .Where(p => p.TipoPrompt!.Codigo == tipoPrompt)
                    .Select(p => p.Prompt)
                    .ToListAsync();

                return string.Join("\n\n", prompts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }
            catch (Exception ex)
            {
                throw new InfraException($"Erro ao buscar PromptEmpresa por Empresa ID: {ex.Message}");
            }
        }
    }
}
