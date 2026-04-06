namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardInsightsDTO
{
    public decimal? TaxaConversaoSemanaAtual { get; set; }
    public decimal? TaxaConversaoSemanaAnterior { get; set; }
    public decimal? VariacaoTaxaConversao { get; set; }
    public int? HoraPicoAtendimento { get; set; }
    public List<DashboardOportunidadeEstagnadaDTO> OportunidadesEstagnadas { get; set; } = new();
}

public class DashboardOportunidadeEstagnadaDTO
{
    public int OportunidadeId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int DiasEstagnada { get; set; }
    public string? ResponsavelNome { get; set; }
    /// <summary>
    /// Nome resumido do responsável (primeiro nome + sobrenomes abreviados + último sobrenome, máx. 20 caracteres)
    /// </summary>
    public string? NomeResponsavelResumido { get; set; }
}
