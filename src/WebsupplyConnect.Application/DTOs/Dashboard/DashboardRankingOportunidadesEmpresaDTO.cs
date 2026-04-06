namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardRankingOportunidadesEmpresaDTO
{
    public int EmpresaId { get; set; }
    public string NomeEmpresa { get; set; } = string.Empty;
    public int GrupoEmpresaId { get; set; }

    public int OportunidadesTotal { get; set; }
    public int OportunidadesAbertas { get; set; }
    public int OportunidadesGanhas { get; set; }
    public int OportunidadesPerdidas { get; set; }
}
