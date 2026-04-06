namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Campanha disponível para filtro no dashboard (sempre por nome agregado).
/// </summary>
public class DashboardCampanhaDisponivelDTO
{
    public string NomeCampanha { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade de leads únicos no período/filtros atuais.
    /// </summary>
    public int QuantidadeLeads { get; set; }
}
