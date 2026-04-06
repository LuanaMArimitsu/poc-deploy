using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface INotificacaoClient
    {
        Task<HttpResponseMessage> NovoLead(NotificarNovoLeadDTO request);
        Task<HttpResponseMessage> NovoLeadVendedor(NotificarNovoLeadVendedorDTO request);
        Task<HttpResponseMessage> NovaMensagem(NotificarNovaMensagemDTO request);
        Task<HttpResponseMessage> AtualizarMensagemStatus(NotificarStatusMensagemAtualizadoDTO request);
        Task<HttpResponseMessage> LeadAlterado(NotificarNovoLeadDTO request);
        Task<HttpResponseMessage> EscalonamentoAutomaticoLider(NotificacaoEscalonamentoDTO request);
        Task<HttpResponseMessage> EscalonamentoAutomaticoVendedor(NotificacaoEscalonamentoDTO request);
        Task<HttpResponseMessage> NovoLeadEvento(NotificarNovoLeadDTO request);
    }
}
