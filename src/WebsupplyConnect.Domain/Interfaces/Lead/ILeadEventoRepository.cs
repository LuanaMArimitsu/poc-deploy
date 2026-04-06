using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Lead
{
    public interface ILeadEventoRepository : IBaseRepository
    {
        Task<List<LeadEvento>> GetAllAsync();
        Task<List<LeadEvento>> GetByLeadIdAsync(int leadId);
        Task<(List<LeadEvento> Itens, int TotalItens)> ListEventosPorCampanhaAsync(
                    int campanhaId,
                    int? pagina = null,
                    int? tamanhoPagina = null);

        Task<LeadEvento?> GetLeadEventoByIdAsync(int id);
    }
}
