using WebsupplyConnect.Application.DTOs.Lead.Evento;
using WebsupplyConnect.Application.DTOs.Lead.Historico;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ILeadEventoWriterService
    {
        /// <summary>
        /// Registra o histórico quando o lead é criado manualmente via API.
        /// </summary>
        Task RegistrarEventoAsync(Domain.Entities.Lead.Lead lead, int? campanhaId = null, string? observacao = null, int? origemId = null);

        /// <summary>
        /// Registra o histórico quando o lead é criado manualmente via BOT.
        /// </summary>
        Task RegistrarEventoViaWhatsAsync(Domain.Entities.Lead.Lead lead, int canalId, int? campanhaId = null);

        /// <summary>
        /// Registra o histórico quando o lead é criado manualmente pelo usuário.
        /// </summary>    
        Task RegistrarEventoManualAsync(LeadEventoDTO dto);

        /// <summary>
        /// Edita um evento de lead existente.
        /// </summary>
        Task UpdateEventoAsync(int eventoId, LeadEventoUpdateDTO dto);
        Task TransferirLeadsAsync(int campanhaOrigemId, int campanhaDestinoId);
        Task RegistrarEventoConversaViaWhatsAsync(Domain.Entities.Lead.Lead lead, int canalId, int? campanhaId = null);
    }
}
