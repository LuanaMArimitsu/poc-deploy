using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Equipe
{
    public interface IStatusMembroEquipeReadService
    {
        Task<bool> StatusExisteAsync(int statusId);
        Task<List<StatusMembroEquipeDto>> ListarStatusFixoAsync();
        Task<StatusMembroEquipe> GetStatusMembro(int? statusId = null, string? codigoStatus = null);
    }
}
