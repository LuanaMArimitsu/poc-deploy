using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface do serviço de configuração de distribuição
    /// Centraliza o acesso a configurações e regras de distribuição
    /// </summary>
    public interface IDistribuicaoConfiguracaoReaderService
    {
        /// <summary>
        /// Obtém a configuração ativa de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Configuração ativa ou null se não encontrada</returns>
        Task<ConfiguracaoDistribuicao?> GetConfiguracaoAtivaAsync(int empresaId);

        Task<bool> PossuiConfiguracaoAtivaAsync(int empresaId);

        /// <summary>
        /// Obtém configuração e regras em uma única consulta
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Contexto completo de distribuição</returns>
        Task<DistribuicaoConfigurationContext> GetConfiguracaoComRegrasAsync(int empresaId);
    }
    
    /// <summary>
    /// Contexto que agrupa configuração e regras de distribuição
    /// </summary>
    public class DistribuicaoConfigurationContext
    {
        public ConfiguracaoDistribuicao? Configuracao { get; set; }
        public List<RegraDistribuicao> Regras { get; set; } = new();
        
        /// <summary>
        /// Indica se a configuração é válida para distribuição
        /// </summary>
        public bool IsValid => Configuracao != null;
        
        /// <summary>
        /// Indica se existem regras configuradas
        /// </summary>
        public bool HasRegras => Regras.Any();
    }
}
