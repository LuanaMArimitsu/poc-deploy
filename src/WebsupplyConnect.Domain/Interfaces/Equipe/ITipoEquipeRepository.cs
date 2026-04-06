using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Equipe
{
    public interface ITipoEquipeRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(int id);
        Task<List<WebsupplyConnect.Domain.Entities.Equipe.TipoEquipe>> ListarAsync();
    }
}
