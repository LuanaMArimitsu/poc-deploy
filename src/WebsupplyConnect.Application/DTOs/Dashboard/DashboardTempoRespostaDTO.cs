namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardTempoRespostaDTO
{
    public List<DashboardTempoRespostaItemDTO> DistribuicaoPorFaixa { get; set; } = new();
    public decimal TempoMedioMinutos { get; set; }
    public decimal MedianaMinutos { get; set; }
}

public class DashboardTempoRespostaItemDTO
{
    public string Faixa { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Percentual { get; set; }
}
