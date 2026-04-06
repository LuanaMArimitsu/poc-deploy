using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IPushNotificationService
    {
        Task SendToDeviceAsync(string deviceToken, NotificacaoDTO notificacao);
    }
}
