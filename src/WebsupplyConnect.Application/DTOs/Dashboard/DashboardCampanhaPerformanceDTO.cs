namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardCampanhaPerformanceDTO
{
    public int? Disparadas { get; set; }
    public int LeadsGerados { get; set; }
    public int EmNegociacao { get; set; }
    public int Convertidos { get; set; }
    public int Perdidos { get; set; }
    public decimal? ROI { get; set; }
    public decimal? ValorTotal { get; set; }
    public List<DashboardCampanhaDetalheDTO> Campanhas { get; set; } = new();
}

public class DashboardCampanhaDetalheDTO
{
    public string NomeCampanha { get; set; } = string.Empty;
    public int? Disparadas { get; set; }
    public int? Acessados { get; set; }
    public int? Clicados { get; set; }
    public int? Contatados { get; set; }
    public int Leads { get; set; }
    public int Negociacao { get; set; }
    public int Convertidos { get; set; }
    public int Perdidos { get; set; }
    public int EmAberto { get; set; }
    public decimal? ROI { get; set; }
}
