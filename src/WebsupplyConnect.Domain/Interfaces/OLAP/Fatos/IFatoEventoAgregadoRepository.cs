using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;

public interface IFatoEventoAgregadoRepository : IBaseRepository
{
    Task<FatoEventoAgregado?> ObterPorLeadEventoDataReferenciaAsync(
        int leadEventoId, DateTime dataReferencia, CancellationToken cancellationToken = default);

    Task<List<FatoEventoAgregado>> ObterPorPeriodoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém fatos de evento cujo lead teve último evento no período. Usado nos indicadores de campanha.
    /// </summary>
    Task<List<FatoEventoAgregado>> ObterPorPeriodoDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default);

    Task UpsertAsync(FatoEventoAgregado fato, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui logicamente todos os fatos de evento do OLAP vinculados ao lead.
    /// </summary>
    Task ExcluirTodosPorLeadIdAsync(int leadId, CancellationToken cancellationToken = default);
}
