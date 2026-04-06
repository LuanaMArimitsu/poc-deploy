using WebsupplyConnect.Application.DTOs.Dashboard;

namespace WebsupplyConnect.Application.Interfaces.OLAP;

/// <summary>
/// Serviço para obter os catálogos (filtros) do Dashboard OLAP.
/// </summary>
public interface IDashboardCatalogoService
{
    /// <summary>
    /// Obtém todos os catálogos para os dropdowns do Dashboard.
    /// </summary>
    /// <param name="empresaIds">
    /// Se informado e não vazio, limita empresas, equipes, vendedores e campanhas a esse conjunto
    /// (já validado em relação ao escopo do usuário no controller).
    /// </param>
    /// <param name="usuarioId">Se informado, restringe empresas às que o usuário tem acesso via UsuarioEmpresa</param>
    Task<DashboardCataloguesDTO> ObterCataloguesAsync(IReadOnlyList<int>? empresaIds = null, int? usuarioId = null);
}
