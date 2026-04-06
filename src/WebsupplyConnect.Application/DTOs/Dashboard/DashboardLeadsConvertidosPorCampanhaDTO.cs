namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardLeadsConvertidosPorCampanhaDTO
{
    public string NomeCampanha { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int TotalConvertidos { get; set; }
    public decimal TaxaConversao { get; set; }
}
