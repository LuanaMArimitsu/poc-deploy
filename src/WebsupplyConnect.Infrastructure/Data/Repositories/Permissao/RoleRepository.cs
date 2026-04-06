using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Permissao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Permissao
{
    public class RoleRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IRoleRepository
    {
        public async Task<(IReadOnlyList<Role> Itens, int TotalItens)> GetRolesAsync(
            string nome,
            int empresaId,
            string contexto,
            int pagina,
            int tamanhoPagina
        )
        {
            var query = _context.Role
                .AsNoTracking()
                .Where(r => !r.Excluido);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(r => r.Nome.Contains(nome));

            if (empresaId > 0)
                query = query.Where(r => r.EmpresaId == empresaId || r.Contexto == "GLOBAL");

            if (!string.IsNullOrWhiteSpace(contexto))
                query = query.Where(r => r.Contexto == contexto);

            var totalItens = await query.CountAsync();

            var itens = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Include(x => x.RolePermissoes)
                .ThenInclude(x => x.Permissao)
                .Include(x => x.UsuarioRoles)
                .Include(x => x.Empresa)
                .ToListAsync();

            return (itens, totalItens);
        }


        public async Task<IReadOnlyList<Role>> GetRolesByUsuarioAsync(int usuarioId)
        {
            return await _context.UsuarioRole
                .AsNoTracking()
                .Where(ur => ur.UsuarioId == usuarioId)
                .Select(ur => ur.Role)
                .Where(r => !r.Excluido)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Domain.Entities.Permissao.Permissao>> GetPermissoesByRoleAsync(int roleId)
        {
            return await _context.RolePermissao
                .AsNoTracking()
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permissao)
                .Where(r => !r.Excluido && r.Ativa == true)
                .ToListAsync();
        }

        public void Remove(UsuarioRole usuarioRole)
        {
            _context.UsuarioRole.Remove(usuarioRole);
        }

        public async Task<Role?> GetRoleWithIncludes(int roleId)
        {
            return await _context.Role.Where(x => x.Id == roleId).Include(x => x.RolePermissoes).Include(x => x.UsuarioRoles).ThenInclude(x => x.Atribuidor).FirstOrDefaultAsync();
        }

        public async Task<bool> PossuiRolePermissao(int usuarioId, int? empresaId, string codigoPermissao)
        {
            return await _context.UsuarioRole
                .AsNoTracking()
                .Include(x => x.Role)
                    .ThenInclude(r => r.RolePermissoes)
                    .ThenInclude(rp => rp.Permissao)
                    .Where(x => x.Role.Excluido == false && x.Role.Ativa == true && (x.DataExpiracao == null || x.DataExpiracao > TimeHelper.GetBrasiliaTime()))
                .AnyAsync(x =>
                    x.UsuarioId == usuarioId &&
                    (x.Role.EmpresaId == empresaId || x.Role.EmpresaId == null) &&
                    x.Role.RolePermissoes.Any(rp => rp.Permissao.Codigo == codigoPermissao));
        }

        public async Task<(bool AcessoGlobal, List<int> EmpresasIds)> ObterAlcancePermissaoUsuarioAsync(int usuarioId, List<string> codigoPermissao)
        {
            var empresas = await _context.UsuarioRole
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    x.Role.Excluido == false &&
                    x.Role.Ativa == true &&
                    (x.DataExpiracao == null || x.DataExpiracao > TimeHelper.GetBrasiliaTime()) &&
                    x.Role.RolePermissoes.Any(rp => codigoPermissao.Contains(rp.Permissao.Codigo))
                )
                .Select(x => x.Role.EmpresaId)
                .Distinct()
                .ToListAsync();

            var acessoGlobal = empresas.Any(e => e == null);

            return (
                AcessoGlobal: acessoGlobal,
                EmpresasIds: empresas
                    .Where(e => e != null)
                    .Select(e => e.Value)
                    .ToList()
            );
        }


        public async Task<List<UsuarioRole>> ListarUsuarioByRoleAsync(int roleId)
        {
            var usuariosRole = await _context.UsuarioRole.Where(x => x.RoleId == roleId).Include(x => x.Usuario).ToListAsync();
            return usuariosRole;
        }
    }
}
