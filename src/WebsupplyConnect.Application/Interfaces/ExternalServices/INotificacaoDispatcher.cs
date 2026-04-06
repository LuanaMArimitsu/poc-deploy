using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface INotificacaoDispatcher
    {
        Task EnviarNotificacaoAsync(NotificacaoDTO notificacao);
    }
}
