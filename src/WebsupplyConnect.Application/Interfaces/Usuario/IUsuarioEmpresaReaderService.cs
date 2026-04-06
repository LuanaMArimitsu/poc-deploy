using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Usuario
{
    public interface IUsuarioEmpresaReaderService
    {
        Task<UsuarioEmpresa> GetCanalPadraoByUsuarioEmpresaAsync(int usuarioId, int empresaId);
        Task<List<UsuarioEmpresa>> GetVinculosPorUsuarioIdAsync(int usuarioId);
        Task<UsuarioEmpresa?> GetBotByEmpresa(int empresaId);
        Task<UsuarioEmpresa?> GetUsuarioEmpresaByEmpresa(int empresaId, int usuarioLogado);
        Task<UsuarioEmpresa?> GetEquipePadraoByUsuarioEmpresaAsync(int usuarioId, int empresaId);
    }
}
