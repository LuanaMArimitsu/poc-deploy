namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardUltimaAtualizacaoDTO
{
    public DateTime? DataUltimaAtualizacao { get; set; }
    public string? StatusUltimaExecucao { get; set; }
    public int? RegistrosProcessados { get; set; }
    public int? TempoExecucaoSegundos { get; set; }
}
