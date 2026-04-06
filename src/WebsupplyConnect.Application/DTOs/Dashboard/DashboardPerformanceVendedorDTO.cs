namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardPerformanceVendedorDTO
{
    public int VendedorId { get; set; }
    /// <summary>ID da empresa (transacional) do vendedor ou da equipe.</summary>
    public int EmpresaId { get; set; }
    /// <summary>
    /// Nome completo do vendedor/responsável
    /// </summary>
    public string NomeResponsavel { get; set; } = string.Empty;
    /// <summary>
    /// Nome resumido do vendedor (primeiro nome + sobrenomes abreviados + último sobrenome, máx. 20 caracteres)
    /// </summary>
    public string NomeResponsavelResumido { get; set; } = string.Empty;
    public string? NomeEquipe { get; set; }
    public int TotalLeads { get; set; }
    public int LeadsConvertidos { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal TempoMedioRespostaMinutos { get; set; }
    public decimal IndicadorPerformance { get; set; }
    public int TotalConversas { get; set; }
    public int TotalMensagens { get; set; }
    public int MensagensNaoLidas { get; set; }
}
