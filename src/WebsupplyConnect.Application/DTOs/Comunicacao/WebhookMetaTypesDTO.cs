namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record WebhookMetaTypesDTO(
        string From,
        string Id,
        string Timestamp,
        string? Type,
        Text? Text,
        Image? Image,
        Audio? Audio,
        Video? Video,
        Document? Document,
        Sticker? Sticker,
        List<Errors>? Errors
    );

    public record Errors(
        int Code,
        string Title,
        string Message,
        Error_Data Error_Data
    );

    public record Error_Data(
        string Details
    );

    public record Text(
        string Body
    );

    public record Image(
        string? Caption,
        string Mime_Type,
        string Sha256,
        string Id
    );

    public record Audio(
        string Mime_Type,
        string Sha256,
        string Id,
        bool Voice
    );

    public record Video(
         string? Caption,
        string Mime_Type,
        string Sha256,
        string Id
    );

    public record Document(
        string? Caption,
        string Filename,
        string Mime_Type,
        string Sha256,
        string Id
    );

    public record Sticker(
    string Mime_Type,
    string Sha256,
    string Id,
    bool Animated
    );
}
