using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Permissao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Perfil
{
    public class PermissaoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IPermissaoRepository
    {
        public async Task<(IReadOnlyList<Domain.Entities.Permissao.Permissao> Itens, int TotalItens)> GetPermissoesAsync(
            string nome,
            string modulo,
            bool criticas,
            string categoria,
            int pagina,
            int tamanhoPagina)
        {
            var query = _context.Permissao
                .AsNoTracking()
                .Where(p => !p.Excluido);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.Nome.Contains(nome));

            if (!string.IsNullOrWhiteSpace(modulo))
                query = query.Where(p => p.Modulo == modulo);

            if (criticas)
                query = query.Where(p => p.IsCritica);

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria == categoria);

            var totalItens = await query.CountAsync();

            var itens = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalItens);
        }

    }
}
