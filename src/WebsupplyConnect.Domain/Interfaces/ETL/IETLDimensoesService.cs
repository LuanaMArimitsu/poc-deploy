using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;

namespace WebsupplyConnect.Domain.Interfaces.ETL;

/// <summary>
/// Sincronização das dimensões OLAP a partir das fontes transacionais.
/// Consultas de leitura estão em <see cref="IDimensaoOlapReadService"/>.
/// </summary>
public interface IETLDimensoesService : IDimensaoOlapReadService
{
    Task SincronizarDimensaoTempoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task SincronizarDimensaoTempoAsync(IEnumerable<DateTime> datas,
        CancellationToken cancellationToken = default);
    Task SincronizarDimensaoEmpresaAsync(CancellationToken cancellationToken = default);
    Task SincronizarDimensaoEquipeAsync(CancellationToken cancellationToken = default);
    Task SincronizarDimensaoVendedorAsync(DateTime? ultimaDataProcessada = null,
        CancellationToken cancellationToken = default);
    Task SincronizarDimensaoStatusLeadAsync(CancellationToken cancellationToken = default);
    Task SincronizarDimensaoOrigemAsync(CancellationToken cancellationToken = default);
    Task SincronizarDimensaoCampanhaAsync(CancellationToken cancellationToken = default);
    Task SincronizarDimensaoFunilAsync(CancellationToken cancellationToken = default);
    Task SincronizarDimensaoEtapaFunilAsync(CancellationToken cancellationToken = default);
}
