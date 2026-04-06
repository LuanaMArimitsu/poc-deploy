using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

namespace WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;

/// <summary>
/// Consultas de leitura às dimensões OLAP (lookups para ETL e dashboard).
/// Separado da sincronização ETL (<see cref="WebsupplyConnect.Domain.Interfaces.ETL.IETLDimensoesService"/>) para clareza de responsabilidade.
/// </summary>
public interface IDimensaoOlapReadService
{
    Task<DimensaoTempo?> ObterDimensaoTempoPorDataAsync(DateTime data, CancellationToken cancellationToken = default);
    Task<DimensaoEmpresa?> ObterDimensaoEmpresaPorOrigemIdAsync(int empresaOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoEquipe?> ObterDimensaoEquipePorOrigemIdAsync(int equipeOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoEquipe?> ObterDimensaoEquipePorIdAsync(int dimensaoEquipeId, CancellationToken cancellationToken = default);
    Task<DimensaoVendedor?> ObterDimensaoVendedorPorOrigemIdAsync(int usuarioOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoStatusLead?> ObterDimensaoStatusLeadPorOrigemIdAsync(int statusOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdAsync(int origemOrigemId, CancellationToken cancellationToken = default);

    /// <summary>Inclui dimensão excluída (ETL de fatos e sincronização da dimensão).</summary>
    Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(int origemOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoCampanha?> ObterDimensaoCampanhaPorOrigemIdAsync(int campanhaOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoFunil?> ObterDimensaoFunilPorOrigemIdAsync(int funilOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoEtapaFunil?> ObterDimensaoEtapaFunilPorOrigemIdAsync(int etapaOrigemId, CancellationToken cancellationToken = default);
    Task<List<DimensaoEtapaFunil>> ObterDimensoesEtapaFunilNaoExcluidasAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<int, string>> ObterNomesFunilPorDimensaoFunilIdsAsync(IEnumerable<int> dimensaoFunilIds, CancellationToken cancellationToken = default);
    Task<List<DimensaoStatusLead>> ObterDimensoesStatusNaoExcluidasAsync(CancellationToken cancellationToken = default);
    Task<List<DimensaoOrigem>> ObterDimensoesOrigemNaoExcluidasAsync(CancellationToken cancellationToken = default);
    Task<List<DimensaoCampanha>> ObterDimensoesCampanhaNaoExcluidasAsync(CancellationToken cancellationToken = default);
    Task<List<DimensaoEmpresa>> ObterDimensoesEmpresaNaoExcluidasAsync(CancellationToken cancellationToken = default);
    Task<List<DimensaoEquipe>> ObterDimensoesEquipeNaoExcluidasAsync(CancellationToken cancellationToken = default);
    Task<List<DimensaoVendedor>> ObterDimensoesVendedorNaoExcluidasAsync(CancellationToken cancellationToken = default);

    Task<HashSet<int>> ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(CancellationToken cancellationToken = default);
    Task<HashSet<int>> ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(CancellationToken cancellationToken = default);

    Task<(bool ok, string? detalhe)> ValidarFiltroOrigemOrigemIdsParaDashboardAsync(
        IReadOnlyList<int> origemOrigemIds, CancellationToken cancellationToken = default);

    Task<(bool ok, string? detalhe)> ValidarFiltroStatusLeadOrigemIdsParaDashboardAsync(
        IReadOnlyList<int> statusOrigemIds, CancellationToken cancellationToken = default);

    Task<List<DimensaoOrigem>> ObterDimensoesOrigemParaFiltroDashboardPorOrigemIdsAsync(
        IReadOnlyList<int> origemOrigemIds, CancellationToken cancellationToken = default);

    Task<List<DimensaoStatusLead>> ObterDimensoesStatusLeadParaFiltroDashboardPorStatusOrigemIdsAsync(
        IReadOnlyList<int> statusOrigemIds, CancellationToken cancellationToken = default);

    Task<List<DimensaoOrigem>> ObterDimensoesOrigemPorDimensaoIdsIncluindoExcluidasAsync(
        IReadOnlyList<int> dimensaoIds, CancellationToken cancellationToken = default);

    Task<List<DimensaoStatusLead>> ObterDimensoesStatusLeadPorDimensaoIdsIncluindoExcluidasAsync(
        IReadOnlyList<int> dimensaoIds, CancellationToken cancellationToken = default);

    /// <summary>Lookups em lote para o ETL (reduz round-trips).</summary>
    Task<IReadOnlyDictionary<DateTime, DimensaoTempo>> ObterDimensoesTempoPorDatasAsync(
        IReadOnlyCollection<DateTime> datas, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoEmpresa>> ObterDimensoesEmpresaPorOrigemIdsAsync(
        IReadOnlyCollection<int> empresaOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsIncluindoExcluidasAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoEquipe>> ObterDimensoesEquipePorOrigemIdsAsync(
        IReadOnlyCollection<int> equipeOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoVendedor>> ObterDimensoesVendedorPorOrigemIdsAsync(
        IReadOnlyCollection<int> usuarioOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoStatusLead>> ObterDimensoesStatusLeadPorOrigemIdsAsync(
        IReadOnlyCollection<int> statusOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoCampanha>> ObterDimensoesCampanhaPorOrigemIdsAsync(
        IReadOnlyCollection<int> campanhaOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoEtapaFunil>> ObterDimensoesEtapaFunilPorOrigemIdsAsync(
        IReadOnlyCollection<int> etapaOrigemIds, CancellationToken cancellationToken = default);
}
