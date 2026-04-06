namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record CanalConfigDTO
    (
         string Assinatura,
         string WhatsAppPhoneName,
         string WhatsAppBusinessID,
         string WhatsAppPhoneID,
         string WhatsAppAcessToken,
         string? UrlBaseChatBot
    );

}
