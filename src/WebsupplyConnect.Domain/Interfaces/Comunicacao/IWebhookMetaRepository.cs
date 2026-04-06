using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Comunicacao
{
    public interface IWebhookMetaRepository : IBaseRepository
    {
        /// <summary>
        /// Busca um WebhookMeta pelo ID externo (object ID da Meta)
        /// </summary>
        /// <param name="idExterno">ID externo do webhook</param>
        /// <param name="includeDeleted">Se deve incluir webhooks excluídos</param>
        /// <returns>WebhookMeta encontrado ou null</returns>
        Task<WebhookMeta?> GetWebhookMetaByIdExternoAsync(string idExterno, bool includeDeleted = false);


        /// <summary>
        /// Busca um WebhookMeta pelo ID
        /// </summary>
        /// <param name="id">ID webhook</param>
        /// <param name="includeDeleted">Se deve incluir webhooks excluídos</param>
        /// <returns>WebhookMeta encontrado ou null</returns>
        Task<WebhookMeta?> GetWebhookMetaByIdAsync(int id, bool includeDeleted = false);


        /// <summary>
        /// Atualiza um WebhookMeta existente
        /// </summary>
        /// <param name="webhook">Webhook a ser atualizado</param>
        /// <returns>Webhook atualizado</returns>
        WebhookMeta UpdateWebhookMeta(WebhookMeta webhook);
    }
}
