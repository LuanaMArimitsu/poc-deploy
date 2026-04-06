using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para serviço especializado em cálculo de scores
    /// Responsabilidade: APENAS cálculos e transformações de dados de score
    /// </summary>
    public interface IScoreCalculationService
    {
        /// <summary>
        /// Calcula score para um vendedor específico
        /// </summary>
        /// <param name="leadId">ID do lead (pode ser nulo para simulação)</param>
        /// <param name="vendedor">Dados do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="configuracao">Configuração de distribuição</param>
        /// <returns>Score calculado do vendedor</returns>
        Task<ScoreVendedorDTO> CalcularScoreVendedorAsync(
            int? leadId, 
            WebsupplyConnect.Domain.Entities.Usuario.Usuario vendedor, 
            int empresaId, 
            ConfiguracaoDistribuicao configuracao);

        /// <summary>
        /// Calcula scores para uma lista de vendedores
        /// </summary>
        /// <param name="leadId">ID do lead (pode ser nulo para simulação)</param>
        /// <param name="vendedores">Lista de vendedores</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="configuracao">Configuração de distribuição</param>
        /// <returns>Lista de scores calculados</returns>
        Task<List<ScoreVendedorDTO>> CalcularScoresVendedoresAsync(
            int? leadId, 
            List<WebsupplyConnect.Domain.Entities.Usuario.Usuario> vendedores, 
            int empresaId, 
            ConfiguracaoDistribuicao configuracao);

        /// <summary>
        /// Ordena scores e atribui posições
        /// </summary>
        /// <param name="scores">Lista de scores para ordenar</param>
        /// <returns>Lista ordenada com posições atribuídas</returns>
        List<ScoreVendedorDTO> OrdenarEAtribuirPosicoes(List<ScoreVendedorDTO> scores);
    }
}
