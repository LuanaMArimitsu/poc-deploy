namespace WebsupplyConnect.Application.Interfaces.Lead
{
    /// <summary>
    /// Interface do serviço de estatísticas de leads
    /// Responsabilidade: Fornecer dados estatísticos de leads para outros serviços da aplicação
    /// </summary>
    public interface ILeadEstatisticasService
    {
        /// <summary>
        /// Conta o total de leads recebidos por um vendedor em um período
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para contagem</param>
        /// <returns>Quantidade de leads recebidos</returns>
        Task<int> ContarLeadsRecebidosAsync(int vendedorId, int empresaId, int periodoEmDias);
        
        /// <summary>
        /// Conta o total de leads convertidos por um vendedor em um período
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para contagem</param>
        /// <returns>Quantidade de leads convertidos</returns>
        Task<int> ContarLeadsConvertidosAsync(int vendedorId, int empresaId, int periodoEmDias);
        
        /// <summary>
        /// Conta o total de leads perdidos por inatividade por um vendedor em um período
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para contagem</param>
        /// <returns>Quantidade de leads perdidos por inatividade</returns>
        Task<int> ContarLeadsPerdidosPorInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias);
        
        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor em um período
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para cálculo</param>
        /// <returns>Velocidade média de atendimento em minutos</returns>
        Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias);

        /// <summary>
        /// Obtém um lead por ID
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="includeRelated">Se deve incluir entidades relacionadas</param>
        /// <returns>Lead encontrado ou null</returns>
        Task<WebsupplyConnect.Domain.Entities.Lead.Lead?> ObterLeadPorIdAsync(int leadId, bool includeRelated = false);
    }
}
