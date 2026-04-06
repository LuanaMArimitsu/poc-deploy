using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy
{
    /// <summary>
    /// Interface para estratégia de distribuição de leads
    /// Responsabilidade: APENAS calcular scores baseados em dados já preparados
    /// </summary>
    public interface IRegraDistribuicaoStrategy
    {
        /// <summary>
        /// Tipo de regra que esta estratégia implementa
        /// </summary>
        string TipoRegra { get; }
        
        /// <summary>
        /// Calcula o score de um vendedor segundo a regra específica
        /// </summary>
        /// <param name="context">Contexto com dados necessários para o cálculo</param>
        /// <param name="regra">Regra de distribuição com parâmetros</param>
        /// <returns>Score calculado entre 0 e 100</returns>
        decimal CalcularScore(DistribuicaoContextDTO context, RegraDistribuicao regra);
        
        /// <summary>
        /// Verifica se a regra pode ser aplicada baseada no contexto
        /// </summary>
        /// <param name="context">Contexto de distribuição</param>
        /// <param name="regra">Regra de distribuição</param>
        /// <returns>True se a regra pode ser aplicada, false caso contrário</returns>
        bool PodeAplicarRegra(DistribuicaoContextDTO context, RegraDistribuicao regra);
    }
}