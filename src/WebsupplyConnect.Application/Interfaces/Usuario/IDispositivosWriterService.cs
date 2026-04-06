using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Usuario
{
    public interface IDispositivosWriterService
    {
        Task Create(AdicionarDispositivoDTO request);  
        Task AlterarStatusDispositivoAsync(int id, AlterarStatusDispositivoRequestDTO dto);
        Task<bool> AtualizarConexaoSignalRAsync(string deviceId, int usuarioId, string connectionId);
        Task<bool> LimparConexaosignalRAsync(string deviceId, int usuarioId);
        Task<bool> RegistrarHeartbeatAsync(string deviceId, int usuarioLogadoId);
        Task<SincronizacaoDispositivoDTO> RegistrarSincronizacaoAsync(string deviceId, int usuarioLogadoId);
    }
}
