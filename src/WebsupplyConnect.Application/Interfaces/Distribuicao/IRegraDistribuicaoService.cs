using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de regras de distribuição
    /// Responsabilidade: Prover lógica de negócio sobre regras de distribuição
    /// </summary>
    public interface IRegraDistribuicaoService
    {
        /// <summary>
        /// Obtém as regras ativas para uma configuração de distribuição
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Lista de regras ativas ordenadas por ordem de execução</returns>
        Task<List<RegraDistribuicao>> GetRegrasAtivasPorConfiguracaoAsync(int configuracaoId);
        
        /// <summary>
        /// Verifica se uma configuração possui regras ativas
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>True se possui regras ativas</returns>
        Task<bool> PossuiRegrasAtivasAsync(int configuracaoId);
        
        /// <summary>
        /// Conta o número de regras ativas para uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Número de regras ativas</returns>
        Task<int> ContarRegrasAtivasAsync(int configuracaoId);
        
        /// <summary>
        /// Valida se as regras de uma configuração estão bem formadas
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Resultado da validação com erros e warnings</returns>
        Task<ValidationResult> ValidarRegrasConfiguracaoAsync(int configuracaoId);
        
        /// <summary>
        /// Obtém estatísticas das regras de uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Estatísticas detalhadas das regras</returns>
        Task<RegrasStatistics> ObterEstatisticasRegrasAsync(int configuracaoId);
    }
}
