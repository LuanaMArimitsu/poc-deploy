using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;

/// <summary>
/// Repositório genérico para dimensões OLAP.
/// </summary>
public interface IDimensaoRepository : IBaseRepository
{
    Task<DimensaoTempo?> ObterDimensaoTempoPorDataAsync(DateTime data, CancellationToken cancellationToken = default);
    Task<DimensaoEmpresa?> ObterDimensaoEmpresaPorOrigemIdAsync(int empresaOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoEquipe?> ObterDimensaoEquipePorOrigemIdAsync(int equipeOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoVendedor?> ObterDimensaoVendedorPorOrigemIdAsync(int usuarioOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoStatusLead?> ObterDimensaoStatusLeadPorOrigemIdAsync(int statusOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdAsync(int origemOrigemId, CancellationToken cancellationToken = default);

    /// <summary>Inclui linha mesmo com <c>Excluido</c> (sincronização ETL e fatos históricos).</summary>
    Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(int origemOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoCampanha?> ObterDimensaoCampanhaPorOrigemIdAsync(int campanhaOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoFunil?> ObterDimensaoFunilPorOrigemIdAsync(int funilOrigemId, CancellationToken cancellationToken = default);
    Task<DimensaoEtapaFunil?> ObterDimensaoEtapaFunilPorOrigemIdAsync(int etapaOrigemId, CancellationToken cancellationToken = default);
    Task<List<DimensaoEtapaFunil>> ObterDimensoesEtapaFunilNaoExcluidasAsync(CancellationToken cancellationToken = default);

    /// <summary>Lookup de nome por Id da dimensão funil (evita Include múltiplo em etapas).</summary>
    Task<Dictionary<int, string>> ObterNomesFunilPorDimensaoFunilIdsAsync(IEnumerable<int> dimensaoFunilIds, CancellationToken cancellationToken = default);

    /// <summary>Ids de <see cref="DimensaoOrigem"/> (surrogate) referenciados por fatos não excluídos.</summary>
    Task<HashSet<int>> ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(CancellationToken cancellationToken = default);

    /// <summary>Ids de <see cref="DimensaoStatusLead"/> (surrogate) referenciados por fatos não excluídos.</summary>
    Task<HashSet<int>> ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<DateTime, DimensaoTempo>> ObterDimensoesTempoPorDatasAsync(
        IReadOnlyCollection<DateTime> datas, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoEmpresa>> ObterDimensoesEmpresaPorOrigemIdsAsync(
        IReadOnlyCollection<int> empresaOrigemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default);

    /// <summary>Lookup em lote incluindo dimensões marcadas como excluídas (ETL de fatos).</summary>
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
