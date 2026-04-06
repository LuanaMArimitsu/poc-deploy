namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record MetaWebhookRootDTO(string Object, Entry[] Entry);
    public record Entry(string Id, Change[] Changes);
    public record Change(Value Value);
    public record Value(
        string Messaging_Product,
        Metadata Metadata,
        Contact[]? Contacts,
        List<WebhoookMetaStatusDTO>? Statuses,
        List<WebhookMetaTypesDTO>? Messages
    );
    public record Metadata(string Display_Phone_Number, string Phone_Number_Id);
    public record Contact(Profile Profile, string Wa_Id);
    public record Profile(string Name);

}
