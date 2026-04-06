using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IWebhookReaderService
    {
        /// <summary>
        /// Método que confirma se o Payload recebido na webhook veio da Meta.
        /// </summary>
        /// <param name="payload">Payload recebido pela webhook.</param>
        /// <param name="signature">Assintura encontrada no Header do payload recebido pela Webhook.</param>
        /// <returns>True ou false</returns>
        //bool IsValid(string payload, string signature);

        /// <summary>
        /// Método que retorna a assinatura gerada a partir do payload vindo da webhook e da assinatura dentro do seu header.
        /// </summary>
        /// <param name="payload">Payload recebido pela webhook.</param>
        /// <returns>Retorna uma assinatura.</returns>
        //string GenerateSignature(string payload);


        Task<AssinaturaMetaValidacaoResult> IsValid(string payload, string assinaturaRecebida);
    }
}
