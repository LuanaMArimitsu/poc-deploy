namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Ranking de leads por nome de campanha e empresa (uma linha por par nome + empresa transacional).
/// </summary>
public class DashboardRankingLeadsNomeCampanhaEmpresaDTO
{
    public string NomeCampanha { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string NomeEmpresa { get; set; } = string.Empty;
    public int GrupoEmpresaId { get; set; }
    public int LeadsRecebidos { get; set; }
}
