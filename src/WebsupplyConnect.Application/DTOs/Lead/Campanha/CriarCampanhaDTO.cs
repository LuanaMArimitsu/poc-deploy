namespace WebsupplyConnect.Application.DTOs.Lead.Campanha
{
    public record CriarCampanhaDTO(
        string Nome,
        string Codigo,
        int EmpresaId,
        int EquipeId,
        DateTime? DataInicio,
        DateTime? DataFim
    );
}
