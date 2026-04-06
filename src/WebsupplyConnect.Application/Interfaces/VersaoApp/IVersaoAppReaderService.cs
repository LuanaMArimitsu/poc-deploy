using WebsupplyConnect.Application.DTOs.VersaoApp;

namespace WebsupplyConnect.Application.Interfaces.VersaoApp
{
    public interface IVersaoAppReaderService
    {
        Task<VersaoAppRetornoDTO> GetUltimaVersaoAppAsync(string? plataformaApp);
    }
}
