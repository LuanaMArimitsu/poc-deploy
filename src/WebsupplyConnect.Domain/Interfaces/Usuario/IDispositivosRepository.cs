using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Usuario
{
    public interface IDispositivosRepository : IBaseRepository
    {
        Task<List<Dispositivo>> DispositivosUserAsync(int usuarioId, bool? ativo = null);
        Task<Dispositivo?> ObterPorDeviceIdAsync(int usuarioId, string deviceId);
        Task<Dispositivo?> ObterPorIdAsync(int id);
        IQueryable<Dispositivo> ObterQueryComUsuario();
        Task<Dispositivo?> ObterDetalhadoPorDeviceIdAsync(string deviceId);
        Task AtualizarAsync(Dispositivo dispositivo);
    }
}
