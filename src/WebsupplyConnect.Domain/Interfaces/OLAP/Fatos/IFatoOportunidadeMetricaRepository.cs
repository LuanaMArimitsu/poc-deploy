using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;

public interface IFatoOportunidadeMetricaRepository : IBaseRepository
{
    Task<FatoOportunidadeMetrica?> ObterPorOportunidadeDataReferenciaAsync(
        int oportunidadeId, DateTime dataReferencia, CancellationToken cancellationToken = default);

    /// <summary>Carrega fatos existentes para um conjunto de (oportunidadeId, data referência hora).</summary>
    Task<Dictionary<(int OportunidadeId, DateTime DataReferencia), FatoOportunidadeMetrica>> ObterPorChavesOportunidadeDataReferenciaAsync(
        IReadOnlyList<(int OportunidadeId, DateTime DataReferencia)> chaves, CancellationToken cancellationToken = default);

    Task<List<FatoOportunidadeMetrica>> ObterPorPeriodoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém fatos de oportunidade cujo lead teve último evento no período. Usado nos indicadores de campanha.
    /// </summary>
    Task<List<FatoOportunidadeMetrica>> ObterPorPeriodoDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém oportunidades estagnadas (abertas, sem interação recente).
    /// Filtra apenas oportunidades não ganhas e não perdidas.
    /// </summary>
    Task<List<FatoOportunidadeMetrica>> ObterOportunidadesEstagnadasAsync(
        int diasEstagnacao = 30, int? empresaId = null, CancellationToken cancellationToken = default);

    Task UpsertAsync(FatoOportunidadeMetrica fato, CancellationToken cancellationToken = default);
}
