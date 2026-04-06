using WebsupplyConnect.Application.DTOs.Permissao.Role;

namespace WebsupplyConnect.Application.Interfaces.Permissao
{
    public interface IRoleWriterService
    {
        Task CriarRoleAsync(CreateRoleDTO dto);
        Task ExcluirRoleAsync(int roleId);
        Task AtualizarRoleAsync(int roleId, int usuarioId, UpdateRoleDTO dto);
        Task AtribuirPermissaoARoleAsync(int roleId, int permissaoId, int concessorId, string? observacoes = null);
        Task RemoverPermissaoDaRoleAsync(int roleId, int permissaoId);
        Task AssociarUsuarioARoleAsync(int usuarioId, int roleId, int atribuidorId, string? observacoes = null);
        Task RemoverUsuarioDaRoleAsync(int usuarioId, int roleId);
    }
}
