using WebsupplyConnect.Application.DTOs.ExternalServices;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IConnectIntegradorService
    {
        Task<GerarEventoResultDTO> ConnectIntegradorAsync(OportunidadeRequestDTO dto, string url, string token, int sistemaId);
    }
}
