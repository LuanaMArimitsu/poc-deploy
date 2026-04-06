namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de cache de métricas
    /// Responsabilidade: Gerenciar APENAS o cache de métricas de vendedores
    /// </summary>
    public interface IMetricaCacheService
    {
        /// <summary>
        /// Invalida todo o cache de métricas para um vendedor e empresa
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        void InvalidarCacheVendedor(int vendedorId, int empresaId);
        
        /// <summary>
        /// Invalida cache específico de taxa de conversão
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias</param>
        void InvalidarCacheTaxaConversao(int vendedorId, int empresaId, int periodoEmDias = 30);
        
        /// <summary>
        /// Invalida cache específico de velocidade de atendimento
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias</param>
        void InvalidarCacheVelocidadeAtendimento(int vendedorId, int empresaId, int periodoEmDias = 30);
        
        /// <summary>
        /// Invalida cache específico de taxa de perda por inatividade
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias</param>
        void InvalidarCacheTaxaPerdaInatividade(int vendedorId, int empresaId, int periodoEmDias = 30);
    }
}
