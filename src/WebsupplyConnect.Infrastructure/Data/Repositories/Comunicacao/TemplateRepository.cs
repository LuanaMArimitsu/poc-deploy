using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao
{
    public class TemplateRepository(WebsupplyConnectDbContext context, IUnitOfWork unitOfWork) : BaseRepository(context, unitOfWork), ITemplateRepository
    {

        public async Task<Template?> GetTemplateByOrigem(int origem, int canalId)
        {
            var templatesEmpresa = await _context.Set<Template>()
                .Where(t => t.CanalId == canalId)
                .ToListAsync();

            var templateOrigem = await _context.Set<TemplateOrigem>()
                .Where(to => to.OrigemId == origem && templatesEmpresa.Select(te => te.Id).Contains(to.TemplateId))
                .FirstOrDefaultAsync();

            if (templateOrigem == null)
                return null;

            return await _context.Set<Template>()
                .Where(t => t.Id == templateOrigem.TemplateId)
                .FirstOrDefaultAsync();
        }
    }
}
