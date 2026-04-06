namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardRankingVendedoresTotalizadoresDTO
{
    public int TotalVendedores { get; set; }
    public int TotalLeadsRecebidos { get; set; }
    public int TotalOportunidadesAbertas { get; set; }
    public int TotalOportunidadesGanhas { get; set; }
    public int TotalOportunidadesPerdidas { get; set; }
    public decimal TaxaConversaoPercentual { get; set; }
}

public class DashboardRankingVendedoresResponseDTO
{
    public List<DashboardRankingVendedorDTO> Itens { get; set; } = [];
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
    public DashboardRankingVendedoresTotalizadoresDTO Totalizadores { get; set; } = new();
}
