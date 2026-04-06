using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do repositório de configurações de distribuição
    /// </summary>
    public interface IConfiguracaoDistribuicaoRepository
    {
        /// <summary>
        /// Obtém a configuração de distribuição ativa para a empresa especificada
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="includeDeleted">Se deve incluir configurações excluídas</param>
        /// <returns>Configuração ativa ou null se não existir</returns>
        Task<ConfiguracaoDistribuicao?> GetConfiguracaoAtivaAsync(int? empresaId, bool includeDeleted = false);
        
        /// <summary>
        /// Obtém uma configuração de distribuição pelo ID
        /// </summary>
        /// <param name="id">ID da configuração</param>
        /// <param name="includeDeleted">Se deve incluir configurações excluídas</param>
        /// <returns>Configuração encontrada ou null</returns>
        Task<ConfiguracaoDistribuicao?> GetByIdAsync(int id, bool includeDeleted = false);
        
        /// <summary>
        /// Lista todas as configurações de distribuição para a empresa especificada
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="includeDeleted">Se deve incluir configurações excluídas</param>
        /// <returns>Lista de configurações de distribuição</returns>
        Task<List<ConfiguracaoDistribuicao>> ListConfiguracoesAsync(int empresaId, bool includeDeleted = false);
        
        /// <summary>
        /// Cria uma nova configuração de distribuição
        /// </summary>
        /// <param name="configuracao">Configuração a ser criada</param>
        /// <returns>Configuração criada com ID gerado</returns>
        Task<ConfiguracaoDistribuicao> CreateAsync(ConfiguracaoDistribuicao configuracao);
        
        /// <summary>
        /// Atualiza uma configuração de distribuição existente
        /// </summary>
        /// <param name="configuracao">Configuração a ser atualizada</param>
        /// <returns>Configuração atualizada</returns>
        Task<ConfiguracaoDistribuicao> UpdateAsync(ConfiguracaoDistribuicao configuracao);
        
        /// <summary>
        /// Ativa uma configuração de distribuição e desativa outras da mesma empresa
        /// </summary>
        /// <param name="id">ID da configuração a ser ativada</param>
        /// <returns>True se ativada com sucesso</returns>
        Task<bool> AtivarConfiguracaoAsync(int id);
        
        /// <summary>
        /// Desativa outras configurações da mesma empresa, mantendo apenas uma ativa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="configuracaoIdManter">ID da configuração que deve permanecer ativa</param>
        /// <returns>True se a operação foi bem-sucedida</returns>
        Task<bool> DesativarOutrasConfiguracoesAsync(int empresaId, int configuracaoIdManter);
        
        /// <summary>
        /// Desativa todas as configurações de distribuição de uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>True se operação foi bem-sucedida</returns>
        Task<bool> DesativarTodasAsync(int empresaId);
        
        /// <summary>
        /// Exclui logicamente uma configuração de distribuição
        /// </summary>
        /// <param name="id">ID da configuração</param>
        /// <returns>True se excluída com sucesso</returns>
        Task<bool> DeleteAsync(int id);
        
        /// <summary>
        /// Verifica se uma configuração de distribuição já foi utilizada em distribuições
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>True se existirem distribuições associadas</returns>
        Task<bool> TemHistoricoDistribuicaoAsync(int configuracaoId);
        
        /// <summary>
        /// Associa regras de distribuição a uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="regrasIds">Lista de IDs das regras</param>
        /// <returns>True se associação foi bem-sucedida</returns>
        Task<bool> AssociarRegrasAsync(int configuracaoId, List<int> regrasIds);
        
        /// <summary>
        /// Atualiza as regras associadas a uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="regrasIds">Nova lista de IDs das regras</param>
        /// <returns>True se atualização foi bem-sucedida</returns>
        Task<bool> AtualizarRegrasAsync(int configuracaoId, List<int> regrasIds);
        
        /// <summary>
        /// Verifica se existe uma configuração ativa para a empresa especificada
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>True se existir uma configuração ativa</returns>
        Task<bool> ExisteConfiguracaoAtivaAsync(int empresaId);
    }
}