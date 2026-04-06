using WebsupplyConnect.Application.DTOs.Lead.Evento;
using WebsupplyConnect.Application.DTOs.Lead.Historico;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ILeadEventoReaderService
    {
        Task<List<LeadEventoResponseDTO>> GetAllAsync();
        Task<List<LeadEventoResponseDTO>> GetByLeadIdAsync(int leadId);
        Task<EventosPaginadoDto> ListarEventosPorCampanhaAsync(ListEventoRequestDTO request);
        Task<LeadEvento?> GetLeadEventoByIdAsync(int id);
        /// <summary>Eventos por período (data evento ou modificação) para ETL. Inclui excluídos.</summary>
        Task<List<LeadEvento>> ObterEventosPorPeriodoParaETLAsync(DateTime dataInicio, DateTime dataFim);
        /// <summary>Eventos por lead para ETL. Inclui excluídos.</summary>
        Task<List<LeadEvento>> ObterEventosPorLeadIdParaETLAsync(int leadId);

        /// <summary>Eventos por vários leads em uma consulta (ETL).</summary>
        Task<Dictionary<int, List<LeadEvento>>> ObterEventosAgrupadosPorLeadIdsParaETLAsync(IEnumerable<int> leadIds);
    }
}
