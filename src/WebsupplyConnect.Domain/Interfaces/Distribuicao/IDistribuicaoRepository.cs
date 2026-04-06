using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para o repositório de distribuição
    /// </summary>
    public interface IDistribuicaoRepository : IBaseRepository
    {                
        /// <summary>
        /// Salva o histórico de uma distribuição de leads
        /// </summary>
        /// <param name="historico">Dados do histórico a salvar</param>
        /// <returns>Histórico salvo com ID atualizado</returns>
        Task<HistoricoDistribuicao> SalvarHistoricoDistribuicaoAsync(HistoricoDistribuicao historico);

        /// <summary>
        /// Lista o histórico de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <param name="pagina">Número da página</param>
        /// <param name="tamanhoPagina">Tamanho da página</param>
        /// <returns>Lista de históricos de distribuição</returns>
        Task<List<HistoricoDistribuicao>> ListHistoricoDistribuicaoAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null, 
            int pagina = 1, 
            int tamanhoPagina = 20);

        /// <summary>
        /// Conta o total de históricos de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Total de históricos</returns>
        Task<int> CountHistoricoDistribuicaoAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null);

        /// <summary>
        /// Obtém um histórico de distribuição pelo ID
        /// </summary>
        /// <param name="id">ID do histórico</param>
        /// <returns>Histórico encontrado ou null</returns>
        Task<HistoricoDistribuicao?> GetHistoricoByIdAsync(int id);

        /// <summary>
        /// Obtém o tempo médio de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Tempo médio em segundos</returns>
        Task<decimal> GetTempoMedioDistribuicaoAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null);

        /// <summary>
        /// Obtém a última distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Último histórico de distribuição ou null</returns>
        Task<HistoricoDistribuicao?> GetUltimaDistribuicaoAsync(int empresaId);
    }
}