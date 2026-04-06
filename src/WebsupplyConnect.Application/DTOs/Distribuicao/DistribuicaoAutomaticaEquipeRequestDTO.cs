namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    public record DistribuicaoAutomaticaEquipeRequestDTO
    (
        int LeadId,
        int EmpresaId,
        int EquipeId
    );
}
