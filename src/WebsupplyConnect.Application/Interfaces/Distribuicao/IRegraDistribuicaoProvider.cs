using WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Provider de estratégias de distribuição
    /// Responsabilidade: Centralizar acesso e gerenciamento de strategies de distribuição
    /// </summary>
    public interface IRegraDistribuicaoProvider
    {
        /// <summary>
        /// Obtém a estratégia adequada para o tipo de regra especificado
        /// </summary>
        /// <param name="tipoRegra">Tipo de regra (código/ID)</param>
        /// <returns>Estratégia para o tipo ou null se não encontrar</returns>
        IRegraDistribuicaoStrategy? GetStrategy(string tipoRegra);
        
        /// <summary>
        /// Registra uma estratégia no provider
        /// </summary>
        /// <param name="strategy">Estratégia a ser registrada</param>
        void RegisterStrategy(IRegraDistribuicaoStrategy strategy);
        
        /// <summary>
        /// Obtém todos os tipos de regra disponíveis
        /// </summary>
        /// <returns>Lista com os tipos de regra registrados</returns>
        IReadOnlyCollection<string> GetAvailableRuleTypes();
        
        /// <summary>
        /// Verifica se uma estratégia está disponível para o tipo especificado
        /// </summary>
        /// <param name="tipoRegra">Tipo de regra a verificar</param>
        /// <returns>True se a estratégia estiver disponível, false caso contrário</returns>
        bool IsStrategyAvailable(string tipoRegra);
    }
}