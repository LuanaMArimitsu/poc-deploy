using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Notificacao
{
    public interface INotificacaoRepository : IBaseRepository
    {
        Task<List<WebsupplyConnect.Domain.Entities.Notificacao.Notificacao>> GetNotificacoesByUserAsync(int usuarioId);

        /// <summary>
        /// Todas as notificações não excluídas do destinatário (sem filtros de push/conteúdo).
        /// </summary>
        Task<List<WebsupplyConnect.Domain.Entities.Notificacao.Notificacao>> GetNotificacoesAtivasPorDestinatarioAsync(int usuarioId);

        Task<string> GetNotificacoesStatus(int id, bool includeDeleted = false);
    }
}
