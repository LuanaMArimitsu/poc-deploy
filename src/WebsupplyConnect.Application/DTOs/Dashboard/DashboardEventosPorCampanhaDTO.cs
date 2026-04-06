namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardEventosPorCampanhaDTO
{
    public string NomeCampanha { get; set; } = string.Empty;
    public int TotalEventos { get; set; }
    public int OportunidadesGeradas { get; set; }
    public int OportunidadesGanhas { get; set; }
    public decimal ValorTotal { get; set; }
}
