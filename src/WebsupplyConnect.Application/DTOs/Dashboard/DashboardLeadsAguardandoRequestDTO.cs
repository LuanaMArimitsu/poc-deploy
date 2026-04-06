namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Request para endpoints de leads aguardando atendimento e aguardando resposta.
/// Não aplica filtro de período/datas — retorna todos os leads pendentes conforme os demais filtros.
/// </summary>
public class DashboardLeadsAguardandoRequestDTO
{
    /// <summary>Número da página (1-based). Obrigatório.</summary>
    public int? Pagina { get; set; }

    /// <summary>Tamanho da página (1 a 200). Obrigatório.</summary>
    public int? TamanhoPagina { get; set; }

    public List<int>? EmpresaIds { get; set; }
    public List<int>? EquipeIds { get; set; }
    public List<int>? VendedorIds { get; set; }
    public List<int>? OrigemIds { get; set; }
    public string? CampanhaNome { get; set; }
    public List<string>? CampanhaNomes { get; set; }
    public List<int>? StatusLeadIds { get; set; }

    /// <summary>
    /// Campo para ordenação (ex: dataUltimoEvento, nome, nomeOrigem).
    /// </summary>
    public string? OrdenarPor { get; set; }

    /// <summary>
    /// Direção da ordenação: "asc" ou "desc". Se inválido, usa "asc".
    /// </summary>
    public string? DirecaoOrdenacao { get; set; }

    /// <summary>
    /// Converte para FiltrosDashboardDTO com IgnorarFiltroPeriodo = true (sem filtro de datas).
    /// </summary>
    public FiltrosDashboardDTO ToFiltrosDashboardDTO()
    {
        return new FiltrosDashboardDTO
        {
            IgnorarFiltroPeriodo = true,
            EmpresaIds = EmpresaIds,
            EquipeIds = EquipeIds,
            VendedorIds = VendedorIds,
            OrigemIds = OrigemIds,
            CampanhaNome = CampanhaNome,
            CampanhaNomes = CampanhaNomes,
            StatusLeadIds = StatusLeadIds,
            OrdenarPor = OrdenarPor,
            DirecaoOrdenacao = DirecaoOrdenacao
        };
    }
}
