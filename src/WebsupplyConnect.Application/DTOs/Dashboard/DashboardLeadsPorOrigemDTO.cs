namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardLeadsPorOrigemDTO
{
    public int OrigemId { get; set; }
    public string NomeOrigem { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Percentual { get; set; }
}
