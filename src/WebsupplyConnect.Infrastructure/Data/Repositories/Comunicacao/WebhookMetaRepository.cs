using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao
{
    internal class WebhookMetaRepository : BaseRepository, IWebhookMetaRepository
    {
        /// <summary>
        /// Construtor do repositório
        /// </summary>
        /// <param name="dbContext">Contexto do banco de dados</param>
        public WebhookMetaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Busca um WebhookMeta pelo ID externo (object ID da Meta)
        /// </summary>
        /// <param name="idExterno">ID externo do webhook</param>
        /// <param name="includeDeleted">Se deve incluir webhooks excluídos</param>
        /// <returns>WebhookMeta encontrado ou null</returns>
        public async Task<WebhookMeta?> GetWebhookMetaByIdExternoAsync(string idExterno, bool includeDeleted = false)
        {
            if (string.IsNullOrWhiteSpace(idExterno))
                return null;

            return await GetByPredicateAsync<WebhookMeta>(
                w => w.IdExterno == idExterno,
                includeDeleted
            );
        }

        /// <summary>
        /// Busca um WebhookMeta pelo ID 
        /// </summary>
        /// <param name="id">ID webhook</param>
        /// <param name="includeDeleted">Se deve incluir webhooks excluídos</param>
        /// <returns>WebhookMeta encontrado ou null</returns>
        public async Task<WebhookMeta?> GetWebhookMetaByIdAsync(int id, bool includeDeleted = false)
        {
            return await GetByIdAsync<WebhookMeta>(id, includeDeleted);
        }

        /// <summary>
        /// Atualiza um WebhookMeta existente
        /// </summary>
        /// <param name="webhook">Webhook a ser atualizado</param>
        /// <returns>Webhook atualizado</returns>
        public WebhookMeta UpdateWebhookMeta(WebhookMeta webhook)
        {
            var resultado = Update(webhook);
            return resultado;
        }

    }
}
