namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardRankingLeadsEmpresaDTO
{
    public int EmpresaId { get; set; }
    public string NomeEmpresa { get; set; } = string.Empty;
    public int GrupoEmpresaId { get; set; }
    public int LeadsRecebidos { get; set; }
}
