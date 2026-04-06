using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Notificacao;

namespace WebsupplyConnect.Infrastructure.ExternalServices.SignalR
{
    // Hub SignalR para lidar com notificações em tempo real
    public class NotificacaoHub(ISignalRConnection connectionManager, ILogger<NotificacaoHub> logger, INotificacaoWriterService notificacaoService) : Hub
    {
        private readonly ISignalRConnection _connectionManager = connectionManager;
        private readonly ILogger<NotificacaoHub> _logger = logger;
        private readonly INotificacaoWriterService _notificationService = notificacaoService;

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = GetUserId(httpContext);

                var connectionId = Context.ConnectionId;

                _logger.LogInformation($"User {userId} connected. ConnectionId: {connectionId}");

                // Registrar conexão
                _connectionManager.AddConnection(userId, connectionId);

                // Adicionar aos grupos
                await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
                await Groups.AddToGroupAsync(connectionId, "all_users");
                await Groups.AddToGroupAsync(connectionId, "empresa_1");

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = GetUserId(httpContext);
                var connectionId = Context.ConnectionId;

                _logger.LogInformation($"User {userId} disconnected. ConnectionId: {connectionId}");

                // Remover conexão
                _connectionManager.RemoveConnection(userId, connectionId);

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        [HubMethodName("NotificationReceived")]
        public async Task NotificationReceived(int notificationId, DateTime receivedAt)
        {
            try
            {
                var userId = GetUserId(Context.GetHttpContext());
                _logger.LogInformation($"Notification {notificationId} received by user {userId}");

                //_notificationService.MarkAsDelivered(notificationId, userId);

                await Clients.Caller.SendAsync("NotificationDeliveryConfirmed", notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing NotificationReceived for notification {notificationId}");
            }
        }

        [HubMethodName("Heartbeat")]
        public async Task Heartbeat()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = GetUserId(httpContext);

                var connectionId = Context.ConnectionId;

                _connectionManager.UpdateConnectionActivity(userId, connectionId);

                // Responde com timestamp do servidor
                await Clients.Caller.SendAsync("HeartbeatResponse", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no heartbeat para conexão {ConnectionId}", Context.ConnectionId);
            }
        }

        [HubMethodName("NotificationViewed")]
        public async Task NotificationViewed(int notificationId, DateTime viewedAt)
        {
            try
            {
                await _notificationService.Visualizada(notificationId, viewedAt);

                await Clients.Caller.SendAsync("LogDoBackend", $"Notificação {notificationId} visualizada em {viewedAt}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing NotificationViewed for notification {notificationId}");
                await Clients.Caller.SendAsync("LogDoBackend", $"Erro ao processar NotificationViewed: {ex.Message}");
            }
        }

        private int GetUserId(HttpContext? httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            // Tentar obter do header
            if (httpContext.Request.Headers.TryGetValue("x-user-id", out var headerUserId))
            {
                if (int.TryParse(headerUserId, out var uid))
                    return uid;
            }

            // Tentar obter da query string
            if (httpContext.Request.Query.TryGetValue("userId", out var queryUserId))
            {
                if (int.TryParse(queryUserId, out var uid))
                    return uid;
            }

            throw new UnauthorizedAccessException("Usuário não identificado.");
        }

    }
}
