namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public record ConversaCreateDTO
    (
        string Titulo,
        int LeadId,
        int CanalId,
        int StatusId,
        int EquipeId
    );
}
