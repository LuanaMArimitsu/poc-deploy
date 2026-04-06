using WebsupplyConnect.Application.DTOs.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Equipe
{
    public interface ITipoEquipeReadService
    {
        Task<bool> TipoExisteAsync(int tipoEquipeId);
        Task<List<TipoEquipeDto>> GetTiposFixosAsync();
    }
}