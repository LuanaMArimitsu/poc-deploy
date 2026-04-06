namespace WebsupplyConnect.Application.DTOs.Lead
{
    public record LeadDTO(
        int LeadId,
        int ResponsavelId,
        string NomeResponsavel,
        int MembroId,
        bool IsBot,
        string Nome,
        bool LeadNovo,
        string WhatsappNumero,
        int EmpresaId,
        int CanalId,
        int EquipeId
        );
}
