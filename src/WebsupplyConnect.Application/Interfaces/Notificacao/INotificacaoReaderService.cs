using WebsupplyConnect.Application.DTOs.Notificacao;

namespace WebsupplyConnect.Application.Interfaces.Notificacao
{
    public interface INotificacaoReaderService
    {
          Task<List<NotificacaoListaDTO>> NotificacoesSyncAsync(int usuarioId);
    }
}
