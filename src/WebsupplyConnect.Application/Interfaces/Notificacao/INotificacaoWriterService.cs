using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Interfaces.Notificacao
{
    public interface INotificacaoWriterService
    {
        Task NovoLead(NotificarNovoLeadDTO dto);
        Task NovoLeadVendedor(NotificarNovoLeadVendedorDTO dto);
        Task LeadAtualizado(NotificarNovoLeadDTO dto);
        Task LeadExcluido(NotificarNovoLeadDTO dto);
        Task NovaMensagem(NotificarNovaMensagemDTO dto);
        Task Visualizada(int notificacaoId, DateTime date);
        Task MensagemAtualizarStatus(NotificarStatusMensagemAtualizadoDTO dto);
        Task MarcarTodasComoLidasAsync(int usuarioId);
        Task<List<NotificacaoProcessadaDTO>> ProcessarNotificacoesCriadasAsync(int usuarioId);
        Task EscalonamentoAutomaticoLider(NotificacaoEscalonamentoDTO dto);
        Task EscalonamentoAutomaticoVendedor(NotificacaoEscalonamentoDTO dto);
        Task NovoLeadEvento(NotificarNovoLeadDTO dto);
        Task ExcluirNotificacaoAsync(int notificacaoId, int usuarioId);

        /// <summary>
        /// Marca como visualizadas e exclui logicamente todas as notificações ativas do destinatário.
        /// </summary>
        Task<NotificacaoLimpezaResultadoDTO> ExcluirTodasEMarcarComoLidasAsync(int usuarioId);
    }
}
