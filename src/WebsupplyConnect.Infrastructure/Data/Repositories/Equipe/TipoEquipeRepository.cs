using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Equipe
{
    internal class TipoEquipeRepository : BaseRepository, ITipoEquipeRepository
    {
        private readonly ILogger<TipoEquipeRepository> _logger;

        public TipoEquipeRepository(
            WebsupplyConnectDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<TipoEquipeRepository> logger) : base(dbContext, unitOfWork)
        {
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<WebsupplyConnect.Domain.Entities.Equipe.TipoEquipe>()
                .AsNoTracking()
                .AnyAsync(t => t.Id == id && !t.Excluido);
        }

        public async Task<List<WebsupplyConnect.Domain.Entities.Equipe.TipoEquipe>> ListarAsync()
        {
            return await _context.Set<WebsupplyConnect.Domain.Entities.Equipe.TipoEquipe>()
                .AsNoTracking()
                .Where(t => !t.Excluido)
                .OrderBy(t => t.Ordem)
                .ToListAsync();
        }
    }
}
