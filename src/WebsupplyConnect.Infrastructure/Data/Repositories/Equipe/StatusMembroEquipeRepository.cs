using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Equipe
{
    internal class StatusMembroEquipeRepository : BaseRepository, IStatusMembroEquipeRepository
    {
        public StatusMembroEquipeRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public async Task<IReadOnlyList<StatusMembroEquipe>> ListarStatusFixosAsync()
        {
            return await _context.Set<StatusMembroEquipe>()
                .AsNoTracking()
                .Where(s => !s.Excluido)
                .OrderBy(s => s.Nome)
                .ToListAsync();
        }
    }
}
