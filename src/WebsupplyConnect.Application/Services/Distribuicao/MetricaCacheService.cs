using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de cache de métricas usando Redis
    /// Responsabilidade: Gerenciar APENAS o cache de métricas de vendedores
    /// </summary>
    public class MetricaCacheService : IMetricaCacheService
    {
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<MetricaCacheService> _logger;
        
        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public MetricaCacheService(
            IRedisCacheService redisCacheService,
            ILogger<MetricaCacheService> logger)
        {
            _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Invalida todo o cache de métricas para um vendedor e empresa
        /// </summary>
        public void InvalidarCacheVendedor(int vendedorId, int empresaId)
        {
            _logger.LogDebug("Invalidando cache Redis de métricas para vendedor {VendedorId}, empresa {EmpresaId}", 
                vendedorId, empresaId);
            
            try
            {
                // Executa invalidação de forma assíncrona sem bloquear
                _ = Task.Run(async () =>
                {
                    // Invalida os caches mais comuns (30 dias)
                    await InvalidarCacheTaxaConversaoAsync(vendedorId, empresaId, 30);
                    await InvalidarCacheVelocidadeAtendimentoAsync(vendedorId, empresaId, 30);
                    await InvalidarCacheTaxaPerdaInatividadeAsync(vendedorId, empresaId, 30);
                    
                    // Também invalida outros períodos comuns
                    var periodosComuns = new[] { 7, 15, 60, 90 };
                    foreach (var periodo in periodosComuns)
                    {
                        await InvalidarCacheTaxaConversaoAsync(vendedorId, empresaId, periodo);
                        await InvalidarCacheVelocidadeAtendimentoAsync(vendedorId, empresaId, periodo);
                        await InvalidarCacheTaxaPerdaInatividadeAsync(vendedorId, empresaId, periodo);
                    }
                });
                
                _logger.LogDebug("Cache Redis invalidado com sucesso para vendedor {VendedorId}", vendedorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao invalidar cache Redis para vendedor {VendedorId}", vendedorId);
            }
        }
        
        /// <summary>
        /// Invalida cache específico de taxa de conversão
        /// </summary>
        public void InvalidarCacheTaxaConversao(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            _ = Task.Run(() => InvalidarCacheTaxaConversaoAsync(vendedorId, empresaId, periodoEmDias));
        }
        
        /// <summary>
        /// Invalida cache específico de velocidade de atendimento
        /// </summary>
        public void InvalidarCacheVelocidadeAtendimento(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            _ = Task.Run(() => InvalidarCacheVelocidadeAtendimentoAsync(vendedorId, empresaId, periodoEmDias));
        }
        
        /// <summary>
        /// Invalida cache específico de taxa de perda por inatividade
        /// </summary>
        public void InvalidarCacheTaxaPerdaInatividade(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            _ = Task.Run(() => InvalidarCacheTaxaPerdaInatividadeAsync(vendedorId, empresaId, periodoEmDias));
        }
        
        /// <summary>
        /// Método assíncrono para invalidar cache de taxa de conversão no Redis
        /// </summary>
        private async Task InvalidarCacheTaxaConversaoAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            try
            {
                string cacheKey = $"metrica:taxa_conversao:{vendedorId}:{empresaId}:{periodoEmDias}";
                await _redisCacheService.RemoveAsync(cacheKey);
                _logger.LogDebug("Cache Redis de taxa de conversão invalidado: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao invalidar cache Redis de taxa de conversão para vendedor {VendedorId}", vendedorId);
            }
        }
        
        /// <summary>
        /// Método assíncrono para invalidar cache de velocidade de atendimento no Redis
        /// </summary>
        private async Task InvalidarCacheVelocidadeAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            try
            {
                string cacheKey = $"metrica:velocidade_atendimento:{vendedorId}:{empresaId}:{periodoEmDias}";
                await _redisCacheService.RemoveAsync(cacheKey);
                _logger.LogDebug("Cache Redis de velocidade de atendimento invalidado: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao invalidar cache Redis de velocidade de atendimento para vendedor {VendedorId}", vendedorId);
            }
        }
        
        /// <summary>
        /// Método assíncrono para invalidar cache de taxa de perda por inatividade no Redis
        /// </summary>
        private async Task InvalidarCacheTaxaPerdaInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            try
            {
                string cacheKey = $"metrica:taxa_perda_inatividade:{vendedorId}:{empresaId}:{periodoEmDias}";
                await _redisCacheService.RemoveAsync(cacheKey);
                _logger.LogDebug("Cache Redis de taxa de perda por inatividade invalidado: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao invalidar cache Redis de taxa de perda por inatividade para vendedor {VendedorId}", vendedorId);
            }
        }
    }
}
