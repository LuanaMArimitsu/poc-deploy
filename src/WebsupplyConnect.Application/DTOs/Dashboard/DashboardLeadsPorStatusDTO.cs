namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardLeadsPorStatusDTO
{
    public int StatusId { get; set; }
    public string NomeStatus { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Percentual { get; set; }
}
