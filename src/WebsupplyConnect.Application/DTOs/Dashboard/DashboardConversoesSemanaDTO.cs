namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardConversoesSemanaDTO
{
    public DateTime Data { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public int Conversoes { get; set; }
    public int? Meta { get; set; }
}
