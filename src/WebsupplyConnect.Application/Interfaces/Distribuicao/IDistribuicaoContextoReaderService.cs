using WebsupplyConnect.Application.DTOs.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço responsável por preparar o contexto de distribuição
    /// Responsabilidade: Buscar e consolidar dados necessários para cálculo de distribuição
    /// </summary>
    public interface IDistribuicaoContextoReaderService
    {
        /// <summary>
        /// Prepara o contexto completo para cálculo de distribuição
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="tipoRegra">Tipo da regra (FILA, MERITO, TEMPO)</param>
        /// <returns>Contexto com todos os dados necessários</returns>
        Task<DistribuicaoContextDTO> PrepararContextoAsync(int leadId, int vendedorId, string tipoRegra);
        
        /// <summary>
        /// Verifica se um vendedor pode receber leads baseado no contexto
        /// </summary>
        /// <param name="context">Contexto de distribuição</param>
        /// <param name="tipoRegra">Tipo da regra</param>
        /// <returns>True se pode receber, false caso contrário</returns>
        bool PodeReceberLead(DistribuicaoContextDTO context, string tipoRegra);
    }
}
