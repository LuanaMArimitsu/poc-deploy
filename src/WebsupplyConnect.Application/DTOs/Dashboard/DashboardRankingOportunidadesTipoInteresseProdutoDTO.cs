namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardRankingOportunidadesTipoInteresseProdutoDTO
{
    public int? TipoInteresseId { get; set; }
    public string NomeTipoInteresse { get; set; } = string.Empty;
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;

    public int OportunidadesTotal { get; set; }
    public int OportunidadesAbertas { get; set; }
    public int OportunidadesGanhas { get; set; }
    public int OportunidadesPerdidas { get; set; }
}
