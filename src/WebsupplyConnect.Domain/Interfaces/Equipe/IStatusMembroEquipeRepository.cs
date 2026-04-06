using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Equipe
{
    public interface IStatusMembroEquipeRepository : IBaseRepository
    {
        Task<IReadOnlyList<StatusMembroEquipe>> ListarStatusFixosAsync();
    }
}
