using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de regras de distribuição
    /// Responsabilidade: Prover lógica de negócio sobre regras de distribuição
    /// </summary>
    public class RegraDistribuicaoService : IRegraDistribuicaoService
    {
        private readonly IRegraDistribuicaoRepository _regraRepository;
        private readonly ILogger<RegraDistribuicaoService> _logger;
        
        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public RegraDistribuicaoService(
            IRegraDistribuicaoRepository regraRepository,
            ILogger<RegraDistribuicaoService> logger)
        {
            _regraRepository = regraRepository ?? throw new ArgumentNullException(nameof(regraRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Obtém as regras ativas para uma configuração de distribuição
        /// </summary>
        public async Task<List<RegraDistribuicao>> GetRegrasAtivasPorConfiguracaoAsync(int configuracaoId)
        {
            _logger.LogDebug("Obtendo regras ativas para configuração {ConfiguracaoId}", configuracaoId);
            
            try
            {
                var regras = await _regraRepository.ListRegrasAtivasPorConfiguracaoAsync(configuracaoId);
                
                _logger.LogDebug("Encontradas {Count} regras ativas para configuração {ConfiguracaoId}", 
                    regras.Count, configuracaoId);
                
                return regras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter regras ativas para configuração {ConfiguracaoId}", configuracaoId);
                throw;
            }
        }
        
        /// <summary>
        /// Verifica se uma configuração possui regras ativas
        /// Otimizado: reutiliza cache de regras quando disponível
        /// </summary>
        public async Task<bool> PossuiRegrasAtivasAsync(int configuracaoId)
        {
            _logger.LogDebug("Verificando se configuração {ConfiguracaoId} possui regras ativas", configuracaoId);
            
            try
            {
                var count = await ContarRegrasAtivasAsync(configuracaoId);
                var possui = count > 0;
                
                _logger.LogDebug("Configuração {ConfiguracaoId} {Status} regras ativas", 
                    configuracaoId, possui ? "possui" : "não possui");
                
                return possui;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar regras ativas para configuração {ConfiguracaoId}", configuracaoId);
                throw;
            }
        }
        
        /// <summary>
        /// Conta o número de regras ativas para uma configuração
        /// Otimizado: reutiliza resultados quando possível
        /// </summary>
        public async Task<int> ContarRegrasAtivasAsync(int configuracaoId)
        {
            _logger.LogDebug("Contando regras ativas para configuração {ConfiguracaoId}", configuracaoId);
            
            try
            {
                // Buscar as regras uma única vez
                var regras = await GetRegrasAtivasPorConfiguracaoAsync(configuracaoId);
                var count = regras.Count;
                
                _logger.LogDebug("Configuração {ConfiguracaoId} possui {Count} regras ativas", 
                    configuracaoId, count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar regras ativas para configuração {ConfiguracaoId}", configuracaoId);
                throw;
            }
        }
        
        /// <summary>
        /// Valida se as regras de uma configuração estão bem formadas
        /// Adiciona valor de negócio ao serviço
        /// </summary>
        public async Task<ValidationResult> ValidarRegrasConfiguracaoAsync(int configuracaoId)
        {
            _logger.LogDebug("Validando regras da configuração {ConfiguracaoId}", configuracaoId);
            
            try
            {
                var regras = await GetRegrasAtivasPorConfiguracaoAsync(configuracaoId);
                var result = new ValidationResult();
                
                if (!regras.Any())
                {
                    result.AddError("Nenhuma regra ativa encontrada para a configuração");
                    return result;
                }
                
                // Validar ordem das regras
                var ordens = regras.Select(r => r.Ordem).ToList();
                if (ordens.Distinct().Count() != ordens.Count)
                {
                    result.AddWarning("Existem regras com ordens duplicadas");
                }
                
                // Validar soma dos pesos
                var somaPesos = regras.Sum(r => r.Peso);
                if (Math.Abs(somaPesos - 100) > 0.01m)
                {
                    result.AddWarning($"Soma dos pesos ({somaPesos:F2}%) difere de 100%");
                }
                
                // Validar se há regras obrigatórias
                var tiposRegras = regras.Select(r => r.TipoRegraId).Distinct().ToList();
                if (!tiposRegras.Any())
                {
                    result.AddError("Nenhum tipo de regra definido");
                }
                
                _logger.LogDebug("Validação da configuração {ConfiguracaoId} concluída: {IsValid}", 
                    configuracaoId, result.IsValid);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar regras da configuração {ConfiguracaoId}", configuracaoId);
                throw;
            }
        }
        
        /// <summary>
        /// Obtém estatísticas das regras de uma configuração
        /// Adiciona valor analítico ao serviço
        /// </summary>
        public async Task<RegrasStatistics> ObterEstatisticasRegrasAsync(int configuracaoId)
        {
            _logger.LogDebug("Obtendo estatísticas das regras da configuração {ConfiguracaoId}", configuracaoId);
            
            try
            {
                var regras = await GetRegrasAtivasPorConfiguracaoAsync(configuracaoId);
                
                var stats = new RegrasStatistics
                {
                    TotalRegras = regras.Count,
                    SomaPesos = regras.Sum(r => r.Peso),
                    TiposRegrasDistintos = regras.Select(r => r.TipoRegraId).Distinct().Count(),
                    TemOrdemDuplicada = regras.GroupBy(r => r.Ordem).Any(g => g.Count() > 1),
                    RegrasComParametros = regras.Count(r => r.Parametros?.Any() == true),
                    PesoMinimo = regras.Any() ? regras.Min(r => r.Peso) : 0,
                    PesoMaximo = regras.Any() ? regras.Max(r => r.Peso) : 0
                };
                
                _logger.LogDebug("Estatísticas da configuração {ConfiguracaoId}: {TotalRegras} regras, {SomaPesos}% peso total", 
                    configuracaoId, stats.TotalRegras, stats.SomaPesos);
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas da configuração {ConfiguracaoId}", configuracaoId);
                throw;
            }
        }
    }
}
