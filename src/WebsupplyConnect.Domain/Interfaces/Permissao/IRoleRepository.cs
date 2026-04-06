using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Permissao
{
    public interface IRoleRepository : IBaseRepository
    {
        Task<(IReadOnlyList<Role> Itens, int TotalItens)> GetRolesAsync(
            string nome,
            int empresaId,
            string contexto,
            int pagina,
            int tamanhoPagina
        );

        Task<IReadOnlyList<Role>> GetRolesByUsuarioAsync(int usuarioId);
        Task<IReadOnlyList<Entities.Permissao.Permissao>> GetPermissoesByRoleAsync(int roleId);
        void Remove(UsuarioRole usuarioRole);
        Task<Role?> GetRoleWithIncludes(int roleId);
        Task<bool> PossuiRolePermissao(int usuarioId, int? empresaId, string codigoPermissao);
        Task<(bool AcessoGlobal, List<int> EmpresasIds)> ObterAlcancePermissaoUsuarioAsync(int usuarioId, List<string> codigoPermissao);
        Task<List<UsuarioRole>> ListarUsuarioByRoleAsync(int roleId);
    }
}
