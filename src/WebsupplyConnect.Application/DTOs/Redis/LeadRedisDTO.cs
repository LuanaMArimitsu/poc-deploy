namespace WebsupplyConnect.Application.DTOs.Redis
{
    public record LeadRedisDTO(
        int Id,
        string Nome,
        string? WhatsAppNumero,
        int ResponsavelId,
        int EquipeId,
        int EmpresaId);
}
