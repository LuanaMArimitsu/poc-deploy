namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardRankingVendedorDTO
{
    public int VendedorId { get; set; }
    public string NomeResponsavel { get; set; } = string.Empty;
    public string NomeResponsavelResumido { get; set; } = string.Empty;
    public string? NomeEquipe { get; set; }
    public int? EmpresaId { get; set; }
    public string? NomeEmpresa { get; set; }
    public int? GrupoEmpresaId { get; set; }

    public int LeadsRecebidos { get; set; }
    public int LeadsComOportunidade { get; set; }
    public int LeadsConvertidos { get; set; }
    public int LeadsPerdidos { get; set; }

    public int OportunidadesAbertas { get; set; }
    public int OportunidadesGanhas { get; set; }
    public int OportunidadesPerdidas { get; set; }

    public decimal TaxaConversaoLeads { get; set; }
    public decimal TempoMedioRespostaMinutos { get; set; }
    public decimal IndicadorPerformance { get; set; }
}
