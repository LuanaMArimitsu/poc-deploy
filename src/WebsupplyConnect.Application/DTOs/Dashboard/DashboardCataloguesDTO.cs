namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Request do endpoint POST /api/Dashboard/catalogues.
/// <see cref="EmpresaIds"/> vazio ou ausente: todas as empresas no escopo do usuário
/// (ou todas as dimensões, quando o usuário não tem vínculos restritivos).
/// </summary>
public class DashboardCataloguesRequestDTO
{
    public List<int>? EmpresaIds { get; set; }
}

/// <summary>
/// Catálogos para os filtros do Dashboard OLAP.
/// Retornado pelo endpoint POST /api/Dashboard/catalogues.
/// Hierarquia: cada empresa contém equipes; cada equipe contém vendedores.
/// O mesmo vendedor (<see cref="DashboardCatalogoVendedorDTO.Id"/>) pode aparecer em mais de uma equipe
/// quando há vínculos ativos em <c>MembroEquipe</c> para cada equipe.
/// </summary>
public class DashboardCataloguesDTO
{
    public List<DashboardCatalogoEmpresaHierarquiaDTO> Empresas { get; set; } = [];
    public List<DashboardCatalogoStatusLeadDTO> StatusLeads { get; set; } = [];
    public List<DashboardCatalogoOrigemDTO> Origens { get; set; } = [];
    public List<DashboardCatalogoCampanhaDTO> Campanhas { get; set; } = [];
}

public class DashboardCatalogoEmpresaHierarquiaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<DashboardCatalogoEquipeHierarquiaDTO> Equipes { get; set; } = [];
    /// <summary>
    /// Vendedores na dimensão OLAP da empresa que não possuem vínculo ativo como vendedor em nenhuma equipe desta empresa
    /// (status ATIVO, não líder, não bot — mesmo critério das listas por equipe).
    /// </summary>
    public List<DashboardCatalogoVendedorDTO> VendedoresSemEquipe { get; set; } = [];
}

public class DashboardCatalogoEquipeHierarquiaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public List<DashboardCatalogoVendedorDTO> Vendedores { get; set; } = [];
}

public class DashboardCatalogoVendedorDTO
{
    /// <summary>
    /// UsuarioId de origem — usado no filtro do dashboard (responsavelIds).
    /// </summary>
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Nome resumido (primeiro nome + sobrenomes abreviados + último sobrenome, máx. 20 caracteres).
    /// </summary>
    public string NomeResponsavelResumido { get; set; } = string.Empty;

    /// <summary>
    /// Id da equipe de origem (transacional). Zero em <see cref="DashboardCatalogoEmpresaHierarquiaDTO.VendedoresSemEquipe"/>.
    /// </summary>
    public int EquipeId { get; set; }

    /// <summary>
    /// Id da empresa de origem — redundante na árvore, útil para filtros e joins no cliente.
    /// </summary>
    public int EmpresaId { get; set; }
}

public class DashboardCatalogoStatusLeadDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
}

public class DashboardCatalogoOrigemDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class DashboardCatalogoCampanhaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    /// <summary>
    /// Id da empresa transacional da campanha (OLAP <c>DimensaoCampanha.EmpresaId</c>).
    /// </summary>
    public int EmpresaId { get; set; }
}
