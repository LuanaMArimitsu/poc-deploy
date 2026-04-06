namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record BaseResponseDTO(
        int codRetorno,
        string? Mensagem
        );
}
