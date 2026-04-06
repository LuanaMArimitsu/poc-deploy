namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de métricas de vendedor
    /// </summary>
    public interface IMetricaVendedorService
    {
        /// <summary>
        /// Atualiza as métricas de um vendedor após atribuição
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Task de conclusão</returns>
        Task AtualizarMetricasVendedorAposAtribuicaoAsync(int vendedorId, int empresaId);
        
        /// <summary>
        /// Atualiza as métricas de conversão de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="convertido">Se o lead foi convertido ou não</param>
        /// <returns>Task de conclusão</returns>
        Task AtualizarMetricasConversaoAsync(int vendedorId, int empresaId, bool convertido);
        
        /// <summary>
        /// Calcula a taxa de conversão de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para calcular a taxa (padrão: 30)</param>
        /// <returns>Taxa de conversão (0 a 1)</returns>
        Task<decimal> CalcularTaxaConversaoAsync(int vendedorId, int empresaId, int periodoEmDias = 30);
        
        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para calcular a média (padrão: 30)</param>
        /// <returns>Velocidade média de atendimento em minutos</returns>
        Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias = 30);
        
        /// <summary>
        /// Calcula a taxa de perda por inatividade de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para calcular a taxa (padrão: 30)</param>
        /// <returns>Taxa de perda por inatividade (0 a 1)</returns>
        Task<decimal> CalcularTaxaPerdaInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias = 30);

        /// <summary>
        /// Obtém as métricas de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Métricas do vendedor ou null se não encontrado</returns>
        Task<WebsupplyConnect.Domain.Entities.Distribuicao.MetricaVendedor?> ObterMetricaVendedorAsync(int vendedorId, int empresaId);
    }
}