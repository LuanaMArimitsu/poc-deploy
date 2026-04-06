using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Dashboard;

namespace WebsupplyConnect.Application.Interfaces.OLAP;

public interface IOLAPConsultaService
{
    Task<DashboardKPIDTO> ObterKPIsAsync(FiltrosDashboardDTO filtros);

    Task<List<DashboardFunilOportunidadesPorEtapaDTO>> ObterFunilOportunidadesPorEtapaAsync(FiltrosDashboardDTO filtros);

    Task<List<DashboardLeadsPorStatusDTO>> ObterLeadsPorStatusAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardLeadsPorOrigemDTO>> ObterLeadsPorOrigemAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardLeadsPorCampanhaDTO>> ObterLeadsPorCampanhaAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardCampanhaDisponivelDTO>> ObterCampanhasDisponiveisAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardEvolucaoLeadsStatusDTO>> ObterEvolucaoLeadsPorStatusAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardLeadsCriadosPorHorarioDTO>> ObterLeadsCriadosPorHorarioAsync(FiltrosDashboardDTO filtros);
    Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterListagemLeadsAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);

    Task<List<DashboardPerformanceEquipeDTO>> ObterPerformanceEquipesAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardPerformanceVendedorDTO>> ObterPerformanceVendedoresAsync(FiltrosDashboardDTO filtros);
    Task<DashboardRankingVendedoresResponseDTO> ObterRankingVendedoresAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);
    Task<PagedResultDTO<DashboardRankingLeadsEmpresaDTO>> ObterRankingLeadsPorEmpresaAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);
    Task<PagedResultDTO<DashboardRankingLeadsNomeCampanhaDTO>> ObterRankingLeadsPorNomeCampanhaAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);
    Task<PagedResultDTO<DashboardRankingLeadsNomeCampanhaEmpresaDTO>> ObterRankingLeadsPorNomeCampanhaEEmpresaAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);
    Task<PagedResultDTO<DashboardRankingOportunidadesEmpresaDTO>> ObterRankingOportunidadesPorEmpresaAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);
    Task<PagedResultDTO<DashboardRankingOportunidadesTipoInteresseProdutoDTO>> ObterRankingOportunidadesPorTipoInteresseEProdutoAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);
    Task<List<DashboardAtividadePorHorarioDTO>> ObterAtividadePorHorarioAsync(FiltrosDashboardDTO filtros);

    Task<DashboardTempoRespostaDTO> ObterDistribuicaoTempoRespostaAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardConversoesSemanaDTO>> ObterConversoesSemanaAsync(FiltrosDashboardDTO filtros);

    Task<DashboardInsightsDTO> ObterInsightsAsync(FiltrosDashboardDTO filtros);

    Task<DashboardCampanhaPerformanceDTO> ObterPerformanceCampanhasAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardEventosPorCampanhaDTO>> ObterEventosPorCampanhaAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardLeadsConvertidosPorCampanhaDTO>> ObterLeadsConvertidosPorCampanhaAsync(FiltrosDashboardDTO filtros);
    Task<DashboardFunilCampanhaDTO> ObterFunilCampanhaAsync(FiltrosDashboardDTO filtros);
    Task<DashboardConversaoGeralDTO> ObterConversaoGeralAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardEngajamentoCampanhaDTO>> ObterEngajamentoPorCampanhaAsync(FiltrosDashboardDTO filtros);

    Task<DashboardEventosLeadPorHorarioCampanhaDTO> ObterEventosLeadPorHorarioCampanhaAsync(FiltrosDashboardDTO filtros);

    Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoPrimeiroAtendimentoAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);

    Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoRespostaAsync(
        FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);

    Task<List<DashboardLeadPendenteCanonicoDTO>> ObterLeadsPendentesCanonicosAsync(FiltrosDashboardDTO filtros);
    Task<List<DashboardConversaAtivaCanonicaDTO>> ObterConversasAtivasCanonicasAsync(FiltrosDashboardDTO filtros);

    Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsPorVendedorAsync(
        int vendedorId, FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina);

    Task<DashboardUltimaAtualizacaoDTO> ObterUltimaAtualizacaoAsync(CancellationToken cancellationToken = default);
}
