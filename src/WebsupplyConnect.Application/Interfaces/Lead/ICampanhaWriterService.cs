using WebsupplyConnect.Application.DTOs.Lead.Campanha;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ICampanhaWriterService
    {
        Task CriarCampanhaAsync(CriarCampanhaApiDTO dto, bool commit);
        Task EditarCampanhaAsync(int id, EditarCampanhaDTO dto);
        Task DeleteCampanhaAsync(int id);
        Task TransferirCampanhasAsync(int campanhaOrigemId, int campanhaDestinoId);
    }
}
