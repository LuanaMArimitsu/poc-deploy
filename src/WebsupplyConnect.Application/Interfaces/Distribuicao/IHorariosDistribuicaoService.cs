using WebsupplyConnect.Application.DTOs.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para serviço de horários de distribuição
    /// </summary>
    public interface IHorariosDistribuicaoService
    {
        /// <summary>
        /// Verifica se um vendedor está disponível para receber leads baseado nos horários configurados
        /// </summary>
        /// <param name="configuracaoId">ID da configuração de distribuição</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="dataHora">Data e hora para verificação (padrão: agora)</param>
        /// <returns>True se o vendedor está disponível</returns>
        Task<bool> VerificarDisponibilidadeVendedorAsync(int configuracaoId, int vendedorId, DateTime? dataHora = null);
        
        /// <summary>
        /// Verifica se a distribuição está ativa baseado nos horários da configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração de distribuição</param>
        /// <param name="dataHora">Data e hora para verificação (padrão: agora)</param>
        /// <returns>True se a distribuição está ativa</returns>
        Task<bool> VerificarDistribuicaoAtivaAsync(int configuracaoId, DateTime? dataHora = null);
        
        /// <summary>
        /// Obtém os próximos horários de disponibilidade para uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração de distribuição</param>
        /// <param name="dias">Número de dias para calcular (padrão: 7)</param>
        /// <returns>Lista de horários de disponibilidade</returns>
        Task<List<HorarioDisponibilidadeDTO>> ObterProximosHorariosDisponibilidadeAsync(int configuracaoId, int dias = 7);
    }
}
