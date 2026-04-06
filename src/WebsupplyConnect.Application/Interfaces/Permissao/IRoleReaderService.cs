using System.Security.Claims;
using WebsupplyConnect.Application.DTOs.Permissao;
using WebsupplyConnect.Application.DTOs.Permissao.Role;

namespace WebsupplyConnect.Application.Interfaces.Permissao
{
    public interface IRoleReaderService
    {
        Task<RolePaginadoDTO> GetRoles(RoleFiltroDTO filtro);
        Task<IReadOnlyList<RoleDTO>> GetRolesByUsuario(int usuarioId);
        Task<IReadOnlyList<Domain.Entities.Permissao.Permissao>> GetPermissoesByRole(int roleId);
        Task<RoleDTO?> GetRoleByIdWithDetails(int roleId);
        Task<bool> UsuarioTemPermissaoAsync(int usuarioId, int? empresaId, string codigoPermissao);
        Task<PermissaoEmpresasResult> EmpresasPermissaoAsync(int usuarioId, List<string> codigoPermissao);
        int ObterUsuarioId(ClaimsPrincipal user);
        Task<List<UsuarioRoleDTO>> ListarUsuarioByRoleAsync(int roleId);
    }
}
