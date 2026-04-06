using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Lead
{
    public class OrigemRepository : BaseRepository, IOrigemRepository
    {
        public OrigemRepository(WebsupplyConnectDbContext context, IUnitOfWork unitOfWork)
            : base(context, unitOfWork)
        {
        }

        public async Task<List<Origem>> ListarOrigensAsync()
        {
            return await _context.Set<Origem>()
                .Include(o => o.OrigemTipo)
                .Where(o => !o.Excluido)
                .OrderByDescending(o => o.DataCriacao)
                .ToListAsync();
        }

        public async Task<Origem?> GetOrigemByIdAsync(int id)
        {
            return await _context.Set<Origem>()
                .Include(o => o.OrigemTipo)
                .FirstOrDefaultAsync(o => o.Id == id && !o.Excluido);
        }

        public async Task<Origem?> GetOrigemByName(string name)
        {
            name = name.ToLowerInvariant().Replace(" ", "").Trim();

            return await _context.Set<Origem>()
                .Include(o => o.OrigemTipo)
                .FirstOrDefaultAsync(o =>
                    o.Nome.ToLower().Replace(" ", "") == name &&
                    !o.Excluido);
        }
    }
}