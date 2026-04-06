namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Resposta do endpoint GET /api/Dashboard/agregado.
/// Contém todos os dados das 4 abas do dashboard em uma única resposta.
/// </summary>
public class DashboardAgregadoDTO
{
    public DashboardGeralAgregadoDTO Geral { get; set; } = new();
    public DashboardEquipesAgregadoDTO Equipes { get; set; } = new();
    public DashboardLeadsAgregadoDTO Leads { get; set; } = new();
    public DashboardCampanhasAgregadoDTO Campanhas { get; set; } = new();
    public DashboardUltimaAtualizacaoDTO? UltimaAtualizacao { get; set; }
}

/// <summary>
/// Dados da aba Geral: KPIs, tempo de resposta, conversões da semana e insights.
/// </summary>
public class DashboardGeralAgregadoDTO
{
    public DashboardKPIDTO Kpis { get; set; } = new();
    public DashboardTempoRespostaDTO TempoResposta { get; set; } = new();
    public List<DashboardConversoesSemanaDTO> ConversoesSemana { get; set; } = [];
    public DashboardInsightsDTO Insights { get; set; } = new();
    /// <summary>Oportunidades por etapa do funil (dimensão OLAP).</summary>
    public List<DashboardFunilOportunidadesPorEtapaDTO> FunilOportunidadesPorEtapa { get; set; } = [];
}

/// <summary>
/// Dados da aba Equipes: performance de vendedores, performance de equipes e atividade por horário.
/// </summary>
public class DashboardEquipesAgregadoDTO
{
    public List<DashboardPerformanceVendedorDTO> PerformanceVendedores { get; set; } = [];
    public List<DashboardPerformanceEquipeDTO> PerformanceEquipes { get; set; } = [];
    public List<DashboardAtividadePorHorarioDTO> AtividadePorHorario { get; set; } = [];
}

/// <summary>
/// Dados da aba Leads: por status, origem, campanha e evolução por status.
/// A listagem paginada de leads continua em endpoint separado (GET /listagem-leads).
/// </summary>
public class DashboardLeadsAgregadoDTO
{
    public List<DashboardLeadsPorStatusDTO> LeadsPorStatus { get; set; } = [];
    public List<DashboardLeadsPorOrigemDTO> LeadsPorOrigem { get; set; } = [];
    public List<DashboardLeadsPorCampanhaDTO> LeadsPorCampanha { get; set; } = [];
    public List<DashboardEvolucaoLeadsStatusDTO> EvolucaoLeadsStatus { get; set; } = [];
    public List<DashboardLeadsCriadosPorHorarioDTO> LeadsCriadosPorHorario { get; set; } = [];
}

/// <summary>
/// Dados da aba Campanhas: performance, eventos, leads convertidos, funil, conversão geral e engajamento.
/// </summary>
public class DashboardCampanhasAgregadoDTO
{
    public DashboardCampanhaPerformanceDTO CampanhasPerformance { get; set; } = new();
    public List<DashboardEventosPorCampanhaDTO> EventosPorCampanha { get; set; } = [];
    public List<DashboardLeadsConvertidosPorCampanhaDTO> LeadsConvertidosPorCampanha { get; set; } = [];
    public DashboardFunilCampanhaDTO FunilCampanha { get; set; } = new();
    public DashboardConversaoGeralDTO ConversaoGeral { get; set; } = new();
    public List<DashboardEngajamentoCampanhaDTO> EngajamentoPorCampanha { get; set; } = [];
    public DashboardEventosLeadPorHorarioCampanhaDTO EventosLeadPorHorarioCampanha { get; set; } = new();
}
