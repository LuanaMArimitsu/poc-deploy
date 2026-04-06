namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record MidiaMetaDTO
    (
        string Url,
        string Mime_type,
        string Sha256,
        long File_size,
        string Id,
        string Messaging_product
     );
}
