using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface que define operações para manipulação da fila de distribuição
    /// </summary>
    public interface IFilaDistribuicaoRepository : IBaseRepository
    {
        /// <summary>
        /// Obtém a posição de um vendedor na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <returns>Objeto com a posição do vendedor ou null se não estiver na fila</returns>
        Task<FilaDistribuicao?> GetPosicaoVendedorAsync(int empresaId, int vendedorId);
        Task<FilaDistribuicao?> GetPosicaoVendedorExcluidoAsync(int empresaId, int vendedorId);

        /// <summary>
        /// Obtém o próximo vendedor da fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="apenasAtivos">Indica se deve retornar apenas vendedores com status ativo</param>
        /// <returns>Objeto com a posição do próximo vendedor ou null se não houver</returns>
        Task<FilaDistribuicao?> GetProximoVendedorFilaAsync(int empresaId, bool apenasAtivos = true);

        /// <summary>
        /// Registra a atribuição de um lead a um vendedor
        /// </summary>
        /// <param name="posicaoFilaId">ID da posição na fila</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        Task<bool> RegistrarAtribuicaoLeadAsync(int posicaoFilaId);

        /// <summary>
        /// Reorganiza a fila após a distribuição de um lead
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor que recebeu o lead</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        Task<bool> ReorganizarFilaAposDistribuicaoAsync(int empresaId, int vendedorId);

        /// <summary>
        /// Adiciona um vendedor à fila de distribuição
        /// </summary>
        /// <param name="filaDistribuicao">Objeto com as informações da posição na fila</param>
        /// <returns>Objeto adicionado com ID gerado</returns>
        Task<FilaDistribuicao> AdicionarVendedorFilaAsync(FilaDistribuicao filaDistribuicao);

        /// <summary>
        /// Atualiza o status de um vendedor na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="statusId">ID do novo status</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        Task<bool> AtualizarStatusVendedorAsync(int empresaId, int vendedorId, int statusId);

        /// <summary>
        /// Remove um vendedor da fila de distribuição
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <returns>True se removido com sucesso, false caso contrário</returns>
        Task<bool> RemoverVendedorFilaAsync(int empresaId, int vendedorId);

        /// <summary>
        /// Obtém a última posição atual na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Valor da última posição ou 0 se a fila estiver vazia</returns>
        Task<int> GetUltimaPosicaoFilaAsync(int empresaId);

        /// <summary>
        /// Obtém o ID do status da fila pelo código
        /// </summary>
        /// <param name="codigo">Código do status (ex: "ATIVO", "PAUSADO")</param>
        /// <returns>ID do status ou 0 se não encontrado</returns>
        Task<int> GetStatusFilaIdPorCodigoAsync(string codigo);

        /// <summary>
        /// Lista todos os vendedores na fila de uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="apenasAtivos">Indica se deve retornar apenas vendedores com status ativo</param>
        /// <returns>Lista de vendedores na fila</returns>
        Task<List<FilaDistribuicao>> ListarVendedoresFilaAsync(int empresaId, bool apenasAtivos = false);

        /// <summary>
        /// Obtém o status da fila pelo ID
        /// </summary>
        /// <param name="statusId">ID do status</param>
        /// <returns>Entidade de status da fila ou null se não encontrado</returns>
        Task<StatusFilaDistribuicao?> GetStatusFilaByIdAsync(int statusId);

        /// <summary>
        /// Registra a atribuição de um lead a um vendedor
        /// </summary>
        /// <param name="posicaoFilaId">ID da posição na fila</param>
        /// <param name="leadId">ID do lead atribuído</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        Task<bool> RegistrarAtribuicaoLeadAsync(int posicaoFilaId, int leadId);

        /// <summary>
        /// Adiciona uma nova posição na fila
        /// </summary>
        /// <param name="filaDistribuicao">Objeto com as informações da posição na fila</param>
        /// <returns>Objeto adicionado com ID gerado</returns>
        Task<FilaDistribuicao> AddPosicaoFilaAsync(FilaDistribuicao filaDistribuicao);

        /// <summary>
        /// Obtém a próxima posição disponível na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Próxima posição disponível</returns>
        Task<int> GetProximaPosicaoFilaAsync(int empresaId);
        
        /// <summary>
        /// Obtém todos os vendedores na fila de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Lista de posições dos vendedores na fila</returns>
        Task<List<FilaDistribuicao>> GetVendedoresNaFilaAsync(int empresaId);
        Task<bool> RemoverTodosVendedoresFilaAsync(List<MembroEquipe> membrosEquipe);
        Task RestaurarVendedorNaFilaAsync(int empresaId, int vendedorId);
    }
}