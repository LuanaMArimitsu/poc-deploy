using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Usuario
{
    public interface IDispositivosReaderService
    {     
        Task<PagedResponseDTO<DispositivoListagemDTO>> ListarDispositivosPaginadoAsync(DispositivoFiltroRequestDTO filtro);
        Task<DispositivoDetalheDTO?> ObterDispositivoDetalhadoAsync(string deviceId);    
        Task<bool> UsuarioPossuiDispositivoAsync(int usuarioId, string deviceId);
        Task<DispositivoAcessoDTO> VerificarDispositivoStatusAsync(int dispositivoId);
        Task<List<Dispositivo>> GetDispositivosByUserAsync(int usuarioId);
    }
}
