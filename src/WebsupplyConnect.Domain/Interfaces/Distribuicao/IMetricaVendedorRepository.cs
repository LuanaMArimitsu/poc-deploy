using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para o repositório de métricas de vendedor
    /// </summary>
    public interface IMetricaVendedorRepository
    {
        /// <summary>
        /// Obtém a métrica de um vendedor para uma empresa
        /// </summary>
        /// <param name="usuarioId">ID do usuário (vendedor)</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Métrica do vendedor ou null se não encontrada</returns>
        Task<MetricaVendedor?> GetMetricaVendedorAsync(int usuarioId, int empresaId);
        
        /// <summary>
        /// Inicializa uma nova métrica para um vendedor
        /// </summary>
        /// <param name="usuarioId">ID do usuário (vendedor)</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Nova métrica inicializada</returns>
        Task<MetricaVendedor> InicializarMetricaVendedorAsync(int usuarioId, int empresaId);
        
        /// <summary>
        /// Atualiza a métrica de um vendedor
        /// </summary>
        /// <param name="metrica">Métrica a ser atualizada</param>
        /// <returns>Métrica atualizada</returns>
        Task<MetricaVendedor> UpdateMetricaAsync(MetricaVendedor metrica);
        
        /// <summary>
        /// Lista as métricas de todos os vendedores de uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Lista de métricas dos vendedores</returns>
        Task<List<MetricaVendedor>> ListMetricasPorEmpresaAsync(int empresaId);
    }
}