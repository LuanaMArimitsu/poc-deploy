using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de gerenciamento de fila de distribuição
    /// </summary>
    public interface IFilaDistribuicaoService
    {
        /// <summary>
        /// Obtém o próximo vendedor na fila de distribuição
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="apenasAtivos">Se true, considera apenas vendedores com status ativo</param>
        /// <returns>Posição na fila do próximo vendedor ou null se não houver</returns>
        Task<FilaDistribuicao?> ObterProximoVendedorFilaAsync(int empresaId, bool apenasAtivos = true);
        Task<FilaDistribuicao?> ObterPosicaoVendedorExcluidoAsync(int empresaId, int vendedorId);

        /// <summary>
        /// Obtém o próximo vendedor na fila considerando disponibilidade real (horários)
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="apenasAtivos">Se true, considera apenas vendedores com status ativo</param>
        /// <returns>Tupla com posição na fila do próximo vendedor disponível, flag de fallback aplicado e detalhes do fallback</returns>
        Task<(FilaDistribuicao? Vendedor, bool FallbackAplicado, string? DetalhesFallback)> ObterProximoVendedorDisponivelAsync(int empresaId, bool apenasAtivos = true);

        /// <summary>
        /// Atualiza a posição do vendedor na fila após receber um lead
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="leadId">ID do lead recebido (opcional)</param>
        /// <returns>True se a operação foi concluída com sucesso</returns>
        Task<bool> AtualizarPosicaoFilaAposAtribuicaoAsync(int empresaId, int vendedorId, int? leadId);

        /// <summary>
        /// Atribui um lead pelo método de fila simples (round-robin)
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="vendedoresDisponiveis">Lista de vendedores disponíveis</param>
        /// <param name="configuracaoId">ID da configuração de distribuição</param>
        /// <param name="fallbackHorarioAplicado">Indica se o fallback de horário foi aplicado</param>
        /// <param name="detalhesFallbackHorario">Detalhes do fallback de horário</param>
        /// <returns>Registro de atribuição criado ou null em caso de falha</returns>
        Task<AtribuicaoLead?> AtribuirPorFilaSimplesAsync(
            int leadId,
            List<MembroEquipe> vendedoresDisponiveis,
            int configuracaoId,
            bool fallbackHorarioAplicado = false,
            string? detalhesFallbackHorario = null,
            int? empresaId = null);

        /// <summary>
        /// Reorganiza a fila após uma distribuição
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor que recebeu um lead</param>
        /// <returns>True se a operação foi concluída com sucesso</returns>
        Task<bool> ReorganizarFilaAposDistribuicaoAsync(int empresaId, int vendedorId);

        /// <summary>
        /// Inicializa a fila de distribuição para um novo vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Posição na fila criada</returns>
        Task<FilaDistribuicao> InicializarPosicaoFilaVendedorAsync(int vendedorId, int empresaId);

        /// <summary>
        /// Obtém a posição de um vendedor na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <returns>Posição do vendedor na fila ou null</returns>
        Task<FilaDistribuicao?> ObterPosicaoVendedorAsync(int empresaId, int vendedorId);

        /// <summary>
        /// Obtém o status da fila por ID
        /// </summary>
        /// <param name="statusFilaId">ID do status da fila</param>
        /// <returns>Status da fila ou null se não encontrado</returns>
        Task<StatusFilaDistribuicao?> ObterStatusFilaAsync(int statusFilaId);

        /// <summary>
        /// Obtém todos os vendedores na fila de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Lista de posições dos vendedores na fila</returns>
        Task<List<FilaDistribuicao>> ObterVendedoresNaFilaAsync(int empresaId);

        /// <summary>
        /// Atualiza o status de um vendedor na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="statusId">ID do novo status</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        Task<bool> AtualizarStatusVendedorAsync(int empresaId, int vendedorId, int statusId);
        Task RemoverVendedorFilaAsync(int empresaId, int vendedorId);
        Task RemoverTodosVendedorFilaAsync(List<MembroEquipe> membroEquipes);
        Task RestaurarVendedorNaFilaAsync(int empresaId, int vendedorId);
        Task<int> ObterStatusFilaPorCodigoAsync(string codigo);
        Task<int?> ObterVendedorPorFilaSimples(
         List<MembroEquipe> vendedoresDisponiveis,
         int configuracaoId,
         int empresaId);
    }
}