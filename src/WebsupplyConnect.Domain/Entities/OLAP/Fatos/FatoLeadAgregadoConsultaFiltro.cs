namespace WebsupplyConnect.Domain.Entities.OLAP.Fatos;

/// <summary>
/// Filtros para consulta OLAP de fatos de lead (um registro canônico por lead após deduplicação no banco).
/// </summary>
public sealed class FatoLeadAgregadoConsultaFiltro
{
    public required DateTime DataInicio { get; init; }
    public required DateTime DataFim { get; init; }

    /// <summary>Filtro legado por uma empresa (dimensão OLAP).</summary>
    public int? EmpresaDimensaoId { get; init; }

    public bool FiltrarEmpresa { get; init; }
    public bool FiltrarEquipe { get; init; }
    public bool FiltrarVendedor { get; init; }
    public bool FiltrarOrigem { get; init; }
    public bool FiltrarCampanha { get; init; }
    public bool FiltrarStatus { get; init; }
    public bool FiltrarEtapaFunil { get; init; }

    public HashSet<int> EmpresaDimIds { get; init; } = [];
    public HashSet<int> EquipeDimIds { get; init; } = [];
    public HashSet<int> VendedorDimIds { get; init; } = [];
    public HashSet<int> OrigemDimIds { get; init; } = [];
    public HashSet<int> CampanhaDimIds { get; init; } = [];
    public HashSet<int> StatusDimIds { get; init; } = [];
    public HashSet<int> EtapaFunilDimIds { get; init; } = [];
}
