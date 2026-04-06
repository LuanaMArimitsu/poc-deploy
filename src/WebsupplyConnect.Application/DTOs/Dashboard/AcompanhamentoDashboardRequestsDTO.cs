namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class AcompanhamentoDashboardAgregadoRequestDTO
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
    public List<int>? OrigemIds { get; set; }

    public string? CampanhaNome { get; set; }
    public List<string>? CampanhaNomes { get; set; }

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
            OrigemIds = OrigemIds,
            CampanhaNome = CampanhaNome,
            CampanhaNomes = CampanhaNomes
        };
    }
}

public class AcompanhamentoDashboardLeadsPendentesRequestDTO : AcompanhamentoDashboardAgregadoRequestDTO
{
    public int? Pagina { get; set; }
    public int? TamanhoPagina { get; set; }
}

public class AcompanhamentoDashboardConversasAtivasRequestDTO : AcompanhamentoDashboardAgregadoRequestDTO
{
    public int? Pagina { get; set; }
    public int? TamanhoPagina { get; set; }
}

public class AcompanhamentoDashboardConversaClassificacaoRequestDTO
{
    public int ConversaId { get; set; }
}
