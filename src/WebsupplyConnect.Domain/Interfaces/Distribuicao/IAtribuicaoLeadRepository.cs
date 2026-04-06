using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para o repositório de atribuição de leads
    /// </summary>
    public interface IAtribuicaoLeadRepository : IBaseRepository
    {
        /// <summary>
        /// Cria um novo registro de atribuição de lead
        /// </summary>
        /// <param name="atribuicao">Atribuição a ser criada</param>
        /// <returns>Atribuição criada com ID atualizado</returns>
        Task<AtribuicaoLead> CriarAtribuicaoAsync(AtribuicaoLead atribuicao);
        
        /// <summary>
        /// Atualiza um registro de atribuição existente
        /// </summary>
        /// <param name="atribuicao">Atribuição a ser atualizada</param>
        /// <returns>Atribuição atualizada</returns>
        Task<AtribuicaoLead> UpdateAsync(AtribuicaoLead atribuicao);
        
        /// <summary>
        /// Obtém a última atribuição para um lead
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <returns>Última atribuição ou null se não existir</returns>
        Task<AtribuicaoLead?> ObterUltimaAtribuicaoLeadAsync(int leadId);
        
        /// <summary>
        /// Obtém o histórico completo de atribuições de um lead
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <returns>Lista de atribuições ordenadas por data</returns>
        Task<List<AtribuicaoLead>> ListAtribuicoesPorLeadAsync(int leadId);
        
        /// <summary>
        /// Verifica se um lead já tem um responsável
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <returns>True se o lead já tem responsável, false caso contrário</returns>
        Task<bool> LeadPossuiResponsavelAsync(int leadId);

        /// <summary>
        /// Lista as atribuições para um vendedor específico
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <param name="pagina">Número da página</param>
        /// <param name="tamanhoPagina">Tamanho da página</param>
        /// <returns>Lista de atribuições do vendedor</returns>
        Task<List<AtribuicaoLead>> ListAtribuicoesPorVendedorAsync(
            int vendedorId, 
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null, 
            int pagina = 1, 
            int tamanhoPagina = 20);

        /// <summary>
        /// Conta o total de atribuições para um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Total de atribuições</returns>
        Task<int> CountAtribuicoesPorVendedorAsync(
            int vendedorId, 
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null);

        /// <summary>
        /// Lista as atribuições para uma empresa específica
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Lista de atribuições da empresa</returns>
        Task<List<AtribuicaoLead>> ListAtribuicoesPorEmpresaAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null);

        /// <summary>
        /// Obtém estatísticas de distribuição por vendedor
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Lista de estatísticas por vendedor</returns>
        Task<List<object>> GetDistribuicoesPorVendedorAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null);
    }
}