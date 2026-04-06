using WebsupplyConnect.Application.DTOs.ExternalServices;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IWebhookWriterService
    {
        /// <summary>
        /// Registra um novo webhook recebido da API da Meta
        /// </summary>
        /// <param name="idExterno">ID externo do webhook fornecido pela Meta</param>
        /// <param name="payload">Conteúdo completo do webhook em formato JSON</param>
        /// <param name="tipoEvento">Tipo do evento (mensagem, status, etc)</param>
        /// <returns>ID do webhook registrado</returns>
        Task<int> RegisterWebhookAsync(WebhookMetaInboundDTO webhookDto);


        /// <summary>
        /// Atualiza webhook
        /// </summary>
        /// <param name="id">ID da webhook</param>
        /// <param name="conversaID">ID da conversa</param>
        Task<bool> UpdateWebhookAsync(int id, int conversaID);
        Task ProcessWebhookAsync(string payload, string signature);
    }
}
