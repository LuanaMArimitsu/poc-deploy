namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    /// <summary>
    /// DTO para receber e processar webhooks da API do WhatsApp da Meta
    /// </summary>
    public record WebhookMetaInboundDTO(
        string IdExterno,
        string Payload,
        string AssinaturaHMAC
    );
   
}
