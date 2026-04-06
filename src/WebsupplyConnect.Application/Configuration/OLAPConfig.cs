namespace WebsupplyConnect.Application.Configuration;

public class OLAPConfig
{
    public int? MetaConversaoDiaria { get; set; }
    public FormulaPerformanceConfig FormulaPerformance { get; set; } = new();
}

public class FormulaPerformanceConfig
{
    public decimal PesoTaxaConversao { get; set; } = 0.5m;
    public decimal PesoTempoMedioResposta { get; set; } = 0.3m;
    public decimal PesoConversasNaoLidas { get; set; } = 0.2m;
}
