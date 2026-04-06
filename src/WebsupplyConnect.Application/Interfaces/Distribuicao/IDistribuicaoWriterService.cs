using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de distribuição de leads
    /// </summary>
    public interface IDistribuicaoWriterService
    {
        /// <summary>
        /// Executa a distribuição automática de leads pendentes para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="maxLeads">Número máximo de leads a distribuir por execução</param>
        /// <param name="usuarioExecutorId">ID do usuário que está executando a distribuição (null para execução automática)</param>
        /// <returns>Resultado da distribuição com estatísticas</returns>
        Task<HistoricoDistribuicao> ExecutarDistribuicaoAutomaticaAsync(
            int empresaId, 
            int maxLeads = 100, 
            int? usuarioExecutorId = null);
            
        /// <summary>
        /// Distribui um lead específico para o melhor vendedor disponível
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Registro de atribuição criado ou null em caso de falha</returns>
        Task<AtribuicaoLead?> DistribuirLeadAsync(
            int leadId,
            int empresaId);
            
        /// <summary>
        /// Realiza a atribuição manual de um lead a um vendedor específico
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="usuarioAtribuiuId">ID do usuário que está realizando a atribuição</param>
        /// <param name="motivo">Motivo da atribuição manual</param>
        /// <returns>Registro de atribuição criado ou null em caso de falha</returns>
        Task<AtribuicaoLead?> AtribuirLeadManualmenteAsync(
            int leadId, 
            int vendedorId, 
            int usuarioAtribuiuId, 
            string motivo);
            
        /// <summary>
        /// Obtém ou atribui um vendedor responsável para um lead específico
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="forcarDistribuicao">Se true, força uma nova distribuição mesmo se o lead já tiver um responsável</param>
        /// <returns>ID do vendedor responsável e detalhes da atribuição</returns>
        Task<(int? VendedorId, AtribuicaoLead? Atribuicao)> ObterOuAtribuirVendedorParaLeadAsync(
            int leadId, 
            bool forcarDistribuicao = false);
           
        /// <summary>
        /// Atribui ou atualiza responsável a uma lead baseado na regra ativa da empresa
        /// </summary>
        /// <param name="leadId">ID da lead</param>
        /// <returns>Registro de atribuição criado ou null em caso de falha</returns>
        Task<AtribuicaoLead?> AtribuirResponsavelParaLeadAsync(int leadId);

        Task<AtribuicaoPorEquipeDTO> AtribuirResponsavelPorEquipe(int leadId, int empresaId);
        Task<(bool sucess, string message, DistribuicaoAutomaticaEquipeResponseDTO? response)> ExecutarDistribuicaoAutomaticaPorEquipe(int leadId, int empresaId, int equipeId);
        Task<(bool sucess, string message, int? responsavelId)> ObterVendedorParaDistribuicaoPorEquipe(int empresaId, int equipeId);
    }
}