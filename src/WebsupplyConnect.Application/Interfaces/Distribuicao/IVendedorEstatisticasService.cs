namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de estatísticas de vendedor
    /// Responsabilidade: Calcular estatísticas baseadas no histórico de leads
    /// </summary>
    public interface IVendedorEstatisticasService
    {
        /// <summary>
        /// Calcula a taxa de conversão de um vendedor baseada no histórico de leads
        /// </summary>
        /// <param name="vendedorId">ID do vendedor (deve ser maior que zero)</param>
        /// <param name="empresaId">ID da empresa (deve ser maior que zero)</param>
        /// <param name="periodoEmDias">Período em dias para calcular a taxa (padrão: 30, deve ser maior que zero)</param>
        /// <returns>Taxa de conversão em percentual (0 a 100)</returns>
        Task<decimal> CalcularTaxaConversaoAsync(int vendedorId, int empresaId, int periodoEmDias = 30);
        
        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor (deve ser maior que zero)</param>
        /// <param name="empresaId">ID da empresa (deve ser maior que zero)</param>
        /// <param name="periodoEmDias">Período em dias para calcular a média (padrão: 30, deve ser maior que zero)</param>
        /// <returns>Velocidade média de atendimento em minutos</returns>
        Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias = 30);
        
        /// <summary>
        /// Calcula a taxa de perda por inatividade de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor (deve ser maior que zero)</param>
        /// <param name="empresaId">ID da empresa (deve ser maior que zero)</param>
        /// <param name="periodoEmDias">Período em dias para calcular a taxa (padrão: 30, deve ser maior que zero)</param>
        /// <returns>Taxa de perda por inatividade em percentual (0 a 100)</returns>
        Task<decimal> CalcularTaxaPerdaInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias = 30);
    }
}
