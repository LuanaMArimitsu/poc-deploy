using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;

public interface IFatoLeadAgregadoRepository : IBaseRepository
{
    /// <summary>
    /// Para listagem paginada: aplica período (último evento), filtros de dimensão e retorna
    /// um fato por lead (o mais recente por DataUltimoEvento/DataReferencia), sem excluir bots
    /// (o filtro de bot permanece na camada de aplicação).
    /// </summary>
    Task<List<FatoLeadAgregado>> ObterUnicosPorLeadParaListagemAsync(
        FatoLeadAgregadoConsultaFiltro filtro, CancellationToken cancellationToken = default);

    Task<FatoLeadAgregado?> ObterPorLeadIdAsync(
        int leadId, CancellationToken cancellationToken = default);

    Task<FatoLeadAgregado?> ObterPorLeadDataReferenciaAsync(
        int leadId, DateTime dataReferencia, CancellationToken cancellationToken = default);

    Task<List<FatoLeadAgregado>> ObterPorPeriodoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém fatos de lead cujo último evento ocorreu no período. Usa DataUltimoEvento quando disponível,
    /// caso contrário usa DataReferencia como fallback (leads sem eventos).
    /// </summary>
    Task<List<FatoLeadAgregado>> ObterPorPeriodoDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default);

    Task UpsertAsync(FatoLeadAgregado fato, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca registros duplicados de um lead como excluídos, mantendo apenas o mais recente.
    /// </summary>
    Task LimparDuplicatasAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui logicamente todos os fatos de lead do OLAP para o lead (ex.: responsável é bot).
    /// </summary>
    Task ExcluirTodosPorLeadIdAsync(int leadId, CancellationToken cancellationToken = default);
}
