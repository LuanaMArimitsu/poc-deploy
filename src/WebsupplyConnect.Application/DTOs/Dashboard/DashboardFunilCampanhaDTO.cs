namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardFunilCampanhaDTO
{
    public int LeadsGerados { get; set; }
    public int EmNegociacao { get; set; }
    public int Convertidos { get; set; }
    public int Perdidos { get; set; }
    public int? Disparadas { get; set; }
    public int? Acessados { get; set; }
    public int? Clicados { get; set; }
    public int? Contatados { get; set; }
    public decimal? TaxaAbertura { get; set; }
    public decimal? TaxaClique { get; set; }
    public bool DadosExternosDisponiveis { get; set; }
}
