using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.VersaoApp;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.VersaoApp
{
    public class VersaoAppRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IVersaoAppRepository 
    {
        public async Task<Domain.Entities.VersaoApp.VersaoApp> GetUltimaVersaoAppAsync(string? plataformaApp)
        {
            var query = _context.VersaoApp
                .Where(v => !v.Excluido);

            if (!string.IsNullOrWhiteSpace(plataformaApp))
            {
                query = query.Where(v => v.PlataformaApp == plataformaApp);
            }

            return await query
                .OrderByDescending(v => v.Id)
                .FirstOrDefaultAsync();
        }
    }
}
