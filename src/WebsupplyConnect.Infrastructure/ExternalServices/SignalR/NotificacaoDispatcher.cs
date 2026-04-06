using Microsoft.AspNetCore.SignalR;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.SignalR
{
    public class NotificacaoDispatcher : INotificacaoDispatcher
    {
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public NotificacaoDispatcher(IHubContext<NotificacaoHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task EnviarNotificacaoAsync(NotificacaoDTO notificacao)
        {
            await _hubContext.Clients
                .Group($"user_{notificacao.Id}") // ID do usuário
                .SendAsync("ReceiveNotification", notificacao);
        }

    }
}
