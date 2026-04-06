namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Agregação de oportunidades por etapa do funil (OLAP), após deduplicação por oportunidade no período.
/// </summary>
public class DashboardFunilOportunidadesPorEtapaDTO
{
    public int EtapaId { get; set; }
    public int FunilId { get; set; }
    public string NomeEtapa { get; set; } = string.Empty;
    public string NomeFunil { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string Cor { get; set; } = string.Empty;
    public int QuantidadeOportunidades { get; set; }
    public int QuantidadeLeadsDistintos { get; set; }
    public decimal ValorPipeline { get; set; }
}
