namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record WebhoookMetaStatusDTO(
       string Id,
       string Status,
       string Timestamp,
       string Recipient_Id,
       Conversation? Conversation,
       Pricing? Pricing
   );

    public record Conversation(
        string Id,
        string Expiration_Timestamp,
        Origin? Origin
    );

    public record Origin(
        string Type
    );

    public record Pricing(
        bool Billable,
        string Pricing_Model,
        string Category
    );

}
