using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para o repositório de regras de distribuição
    /// </summary>
    public interface IRegraDistribuicaoRepository : IBaseRepository
    {
        /// <summary>
        /// Obtém uma regra de distribuição pelo ID
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Regra encontrada ou null</returns>
        Task<RegraDistribuicao?> GetByIdAsync(int id, bool includeDeleted = false);

        /// <summary>
        /// Lista todas as regras ativas para uma configuração específica
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Lista de regras ativas ordenadas por ordem</returns>
        Task<List<RegraDistribuicao>> ListRegrasAtivasPorConfiguracaoAsync(int configuracaoId, bool includeDeleted = false);

        /// <summary>
        /// Lista todas as regras para uma configuração específica
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Lista de regras ordenadas por ordem</returns>
        Task<List<RegraDistribuicao>> ListRegrasPorConfiguracaoAsync(int configuracaoId, bool includeDeleted = false);

        /// <summary>
        /// Atualiza a ordem de uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="novaOrdem">Nova ordem</param>
        /// <returns>True se atualizada com sucesso, false caso contrário</returns>
        Task<bool> AtualizarOrdemRegraAsync(int id, int novaOrdem);

        /// <summary>
        /// Ativa ou desativa uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="ativo">Novo status de ativação</param>
        /// <returns>True se atualizada com sucesso, false caso contrário</returns>
        Task<bool> AtivarDesativarRegraAsync(int id, bool ativo);

        /// <summary>
        /// Obtém regras por tipo
        /// </summary>
        /// <param name="tipoRegraId">ID do tipo de regra</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Lista de regras do tipo especificado</returns>
        Task<List<RegraDistribuicao>> GetRegrasPorTipoAsync(int tipoRegraId, bool includeDeleted = false);

        /// <summary>
        /// Atualiza os parâmetros de uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="parametros">Lista de parâmetros atualizada</param>
        /// <returns>True se atualizada com sucesso, false caso contrário</returns>
        Task<bool> AtualizarParametrosRegraAsync(int id, List<ParametroRegraDistribuicao> parametros);

        /// <summary>
        /// Cria uma nova regra de distribuição
        /// </summary>
        /// <param name="regra">Entidade RegraDistribuicao a ser criada</param>
        /// <returns>Regra criada com ID gerado</returns>
        Task<RegraDistribuicao> CreateRegraAsync(RegraDistribuicao regra);

        /// <summary>
        /// Exclui logicamente uma regra de distribuição
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <returns>True se excluída com sucesso, false caso contrário</returns>
        Task<bool> DeleteRegraAsync(int id);

        /// <summary>
        /// Lista todos os tipos de regras disponíveis
        /// </summary>
        /// <param name="includeDeleted">Se deve incluir tipos excluídos</param>
        /// <returns>Lista de tipos de regras</returns>
        Task<List<TipoRegraDistribuicao>> ListTiposRegrasAsync(bool includeDeleted = false);

        /// <summary>
        /// Obtém um tipo de regra pelo ID
        /// </summary>
        /// <param name="id">ID do tipo de regra</param>
        /// <param name="includeDeleted">Se deve incluir tipos excluídos</param>
        /// <returns>Tipo de regra encontrado ou null</returns>
        Task<TipoRegraDistribuicao?> GetTipoRegraByIdAsync(int id, bool includeDeleted = false);

        /// <summary>
        /// Obtém um tipo de regra pelo código
        /// </summary>
        /// <param name="codigo">Código do tipo de regra</param>
        /// <param name="includeDeleted">Se deve incluir tipos excluídos</param>
        /// <returns>Tipo de regra encontrado ou null</returns>
        Task<TipoRegraDistribuicao?> GetTipoRegraByCodigoAsync(string codigo, bool includeDeleted = false);
    }
}