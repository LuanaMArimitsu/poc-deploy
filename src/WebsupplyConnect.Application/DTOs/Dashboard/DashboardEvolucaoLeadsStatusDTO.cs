namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardEvolucaoLeadsStatusDTO
{
    public DateTime Data { get; set; }
    public int StatusId { get; set; }
    public string NomeStatus { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
