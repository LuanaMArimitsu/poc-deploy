namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardKPIDTO
{
    public int TotalLeads { get; set; }
    public int LeadsConvertidos { get; set; }
    public int OportunidadesAbertas { get; set; }
    public int OportunidadesGanhas { get; set; }
    public int OportunidadesPerdidas { get; set; }
    public decimal ValorTotalPipeline { get; set; }
    public decimal ValorTotalGanho { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal TempoMedioRespostaMinutos { get; set; }
}
