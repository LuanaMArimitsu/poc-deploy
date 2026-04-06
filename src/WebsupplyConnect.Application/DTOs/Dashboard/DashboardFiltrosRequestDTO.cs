namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardFiltrosRequestDTO
{
    public TipoPeriodoEnum? TipoPeriodo { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int? AnoReferencia { get; set; }
    public int? MesReferencia { get; set; }
    public int? AnoInicioHistorico { get; set; }
    public int? MesInicioHistorico { get; set; }

    public List<int>? EmpresaIds { get; set; }
    public List<int>? EquipeIds { get; set; }
    public List<int>? VendedorIds { get; set; }
    public List<int>? OrigemIds { get; set; }

    /// <inheritdoc cref="FiltrosDashboardDTO.CampanhaNome"/>
    public string? CampanhaNome { get; set; }

    /// <inheritdoc cref="FiltrosDashboardDTO.CampanhaNomes"/>
    public List<string>? CampanhaNomes { get; set; }

    public int? StatusLeadId { get; set; }
    public List<int>? StatusLeadIds { get; set; }
    public int? FunilId { get; set; }
    public List<int>? FunilIds { get; set; }
    public int? EtapaId { get; set; }
    public List<int>? EtapaIds { get; set; }
    public int? VendedorId { get; set; }

    public int? Pagina { get; set; }
    public int? TamanhoPagina { get; set; }

    /// <summary>
    /// Campo para ordenação da listagem de leads (ex: dataUltimoEvento, nome, nomeOrigem).
    /// </summary>
    public string? OrdenarPor { get; set; }

    /// <summary>
    /// Direção da ordenação: "asc" ou "desc". Se inválido, usa "asc".
    /// </summary>
    public string? DirecaoOrdenacao { get; set; }

    public FiltrosDashboardDTO ToFiltrosDashboardDTO()
    {
        return new FiltrosDashboardDTO
        {
            TipoPeriodo = TipoPeriodo,
            DataInicio = DataInicio,
            DataFim = DataFim,
            AnoReferencia = AnoReferencia,
            MesReferencia = MesReferencia,
            AnoInicioHistorico = AnoInicioHistorico,
            MesInicioHistorico = MesInicioHistorico,
            EmpresaIds = EmpresaIds,
            EquipeIds = EquipeIds,
            VendedorIds = VendedorIds,
            OrigemIds = OrigemIds,
            CampanhaNome = CampanhaNome,
            CampanhaNomes = CampanhaNomes,
            StatusLeadId = StatusLeadId,
            StatusLeadIds = StatusLeadIds,
            FunilId = FunilId,
            FunilIds = FunilIds,
            EtapaId = EtapaId,
            EtapaIds = EtapaIds,
            OrdenarPor = OrdenarPor,
            DirecaoOrdenacao = DirecaoOrdenacao
        };
    }
}
