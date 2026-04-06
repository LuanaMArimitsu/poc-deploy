namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Ranking de leads por nome de campanha (dimensão OLAP), somando campanhas distintas com o mesmo nome.
/// </summary>
public class DashboardRankingLeadsNomeCampanhaDTO
{
    public string NomeCampanha { get; set; } = string.Empty;
    public int LeadsRecebidos { get; set; }
}
