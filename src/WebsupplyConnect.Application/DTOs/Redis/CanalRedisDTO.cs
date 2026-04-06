namespace WebsupplyConnect.Application.DTOs.Redis
{
    public record CanalRedisDTO(
        int Id,
        string Nome,
        int EmpresaId,
        string? WhatsAppNumero,
        string? ConfiguracaoIntegracao
    );
}
