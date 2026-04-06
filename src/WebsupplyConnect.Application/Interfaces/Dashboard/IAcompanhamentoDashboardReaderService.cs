using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Dashboard;

namespace WebsupplyConnect.Application.Interfaces.Dashboard;

public interface IAcompanhamentoDashboardReaderService
{
    Task<AcompanhamentoDashboardAgregadoResponseDTO> ObterAcompanhamentoAgregadoAsync(FiltrosDashboardDTO filtros, int usuarioId);

    Task<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>> ObterLeadsPendentesAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina);

    Task<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>> ObterLeadsPrimeiroAtendimentoAguardandoClienteAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina);

    Task<PagedResultDTO<AcompanhamentoDashboardConversaAtivaItemDTO>> ObterConversasAtivasAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina);

    Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoAtendimentoAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina);

    Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoRespostaAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina);

    Task<AcompanhamentoDashboardConversaClassificacaoResponseDTO?> ObterConversaClassificacaoSobDemandaAsync(int conversaId);
}
