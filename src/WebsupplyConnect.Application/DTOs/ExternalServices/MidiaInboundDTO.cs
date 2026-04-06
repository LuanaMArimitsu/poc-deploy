using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public record MidiaInboundDTO
    (
        string ConversaMetaId,
        int UsuarioResponsavelId,
        string LeadNome,
        string MessageMetaId,
        int MensagemId,
        string MediaId,
        string MediaType,
        string? MimeType,
        string? Caption,
        string FileName,
        bool? Voice,
        bool? Animated,
        CanalConfigDTO MetaConfig
    );

}
