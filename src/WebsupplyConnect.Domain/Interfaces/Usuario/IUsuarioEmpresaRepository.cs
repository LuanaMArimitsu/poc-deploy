using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Usuario
{
    public interface IUsuarioEmpresaRepository : IBaseRepository
    {
        Task<UsuarioEmpresa?> GetVinculoUsuarioEmpresaAsync(int usuarioId, int empresaId);
        Task<List<UsuarioEmpresa>> GetVinculosPorUsuarioIdAsync(int usuarioId);
        Task<UsuarioEmpresa?> GetBotVinculoByEmpresaAsync(int empresaId);
        Task<UsuarioEmpresa?> GetUsuarioEmpresaAsync(int empresaId, int usuarioLogado);
        Task<UsuarioEmpresa> GetEquipePadraoVinculoAsync(int usuarioId, int empresaId);
    }
}
