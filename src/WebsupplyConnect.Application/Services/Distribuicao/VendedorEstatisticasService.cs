using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de estatísticas de vendedor
    /// Responsabilidade: Calcular APENAS estatísticas baseadas no histórico de leads
    /// Utiliza cache Redis para otimização de performance
    /// Segue DIP: Depende de ILeadEstatisticasService em vez de ILeadRepository
    /// </summary>
    public class VendedorEstatisticasService : IVendedorEstatisticasService
    {
        private readonly ILeadEstatisticasService _leadEstatisticasService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<VendedorEstatisticasService> _logger;

        /// <summary>
        /// Constantes para configuração do serviço
        /// </summary>
        private const int PERIODO_PADRAO_DIAS = 30;
        private const int CACHE_TTL_MINUTOS = 5;
        private const string CACHE_PREFIX = "metrica";
        private const string CACHE_TAXA_CONVERSAO = "taxa_conversao";
        private const string CACHE_VELOCIDADE_ATENDIMENTO = "velocidade_atendimento";
        private const string CACHE_TAXA_PERDA_INATIVIDADE = "taxa_perda_inatividade";
        
        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public VendedorEstatisticasService(
            ILeadEstatisticasService leadEstatisticasService,
            IRedisCacheService redisCacheService,
            ILogger<VendedorEstatisticasService> logger)
        {
            _leadEstatisticasService = leadEstatisticasService ?? throw new ArgumentNullException(nameof(leadEstatisticasService));
            _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calcula a taxa de conversão de um vendedor baseada no histórico de leads
        /// </summary>
        public async Task<decimal> CalcularTaxaConversaoAsync(int vendedorId, int empresaId, int periodoEmDias = PERIODO_PADRAO_DIAS)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            return await ExecutarComCacheAsync(
                CACHE_TAXA_CONVERSAO,
                vendedorId,
                empresaId,
                periodoEmDias,
                async () =>
                {
                    // Cálculo real da taxa de conversão baseado no histórico
                    int totalRecebidos = await _leadEstatisticasService.ContarLeadsRecebidosAsync(
                        vendedorId, empresaId, periodoEmDias);
                        
                    int totalConvertidos = await _leadEstatisticasService.ContarLeadsConvertidosAsync(
                        vendedorId, empresaId, periodoEmDias);
                    
                    var taxaConversao = totalRecebidos > 0 
                        ? (decimal)totalConvertidos / totalRecebidos * 100 // Percentual
                        : 0;
                    
                    _logger.LogDebug("Taxa de conversão calculada: {Taxa}% para vendedor {VendedorId} ({Convertidos}/{Recebidos})", 
                        taxaConversao, vendedorId, totalConvertidos, totalRecebidos);
                    
                    return taxaConversao;
                },
                "taxa de conversão"
            );
        }
        
        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor
        /// </summary>
        public async Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias = PERIODO_PADRAO_DIAS)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            return await ExecutarComCacheAsync(
                CACHE_VELOCIDADE_ATENDIMENTO,
                vendedorId,
                empresaId,
                periodoEmDias,
                async () =>
                {
                    // Cálculo real da velocidade média de atendimento baseado no histórico
                    var velocidadeMedia = await _leadEstatisticasService.CalcularVelocidadeMediaAtendimentoAsync(
                        vendedorId, empresaId, periodoEmDias);
                    
                    _logger.LogDebug("Velocidade média calculada: {Velocidade} minutos para vendedor {VendedorId}", 
                        velocidadeMedia, vendedorId);
                    
                    return velocidadeMedia;
                },
                "velocidade média de atendimento"
            );
        }
        
        /// <summary>
        /// Calcula a taxa de perda por inatividade de um vendedor
        /// </summary>
        public async Task<decimal> CalcularTaxaPerdaInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias = PERIODO_PADRAO_DIAS)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            return await ExecutarComCacheAsync(
                CACHE_TAXA_PERDA_INATIVIDADE,
                vendedorId,
                empresaId,
                periodoEmDias,
                async () =>
                {
                    // Cálculo real da taxa de perda por inatividade baseado no histórico
                    int totalPerdidos = await _leadEstatisticasService.ContarLeadsPerdidosPorInatividadeAsync(
                        vendedorId, empresaId, periodoEmDias);
                        
                    int totalRecebidos = await _leadEstatisticasService.ContarLeadsRecebidosAsync(
                        vendedorId, empresaId, periodoEmDias);
                    
                    var taxaPerda = totalRecebidos > 0 
                        ? (decimal)totalPerdidos / totalRecebidos * 100 // Percentual
                        : 0;
                    
                    _logger.LogDebug("Taxa de perda por inatividade calculada: {Taxa}% para vendedor {VendedorId} ({Perdidos}/{Recebidos})", 
                        taxaPerda, vendedorId, totalPerdidos, totalRecebidos);
                    
                    return taxaPerda;
                },
                "taxa de perda por inatividade"
            );
        }

        /// <summary>
        /// Valida os parâmetros de entrada dos métodos de cálculo
        /// </summary>
        private static void ValidarParametros(int vendedorId, int empresaId, int periodoEmDias)
        {
            if (vendedorId <= 0)
                throw new ArgumentException("ID do vendedor deve ser maior que zero", nameof(vendedorId));
            
            if (empresaId <= 0)
                throw new ArgumentException("ID da empresa deve ser maior que zero", nameof(empresaId));
            
            if (periodoEmDias <= 0)
                throw new ArgumentException("Período em dias deve ser maior que zero", nameof(periodoEmDias));
        }

        /// <summary>
        /// Executa um cálculo com estratégia de cache Redis
        /// </summary>
        private async Task<decimal> ExecutarComCacheAsync(
            string tipoMetrica,
            int vendedorId,
            int empresaId,
            int periodoEmDias,
            Func<Task<decimal>> calcularMetrica,
            string nomeMetrica)
        {
            string cacheKey = GerarChaveCache(tipoMetrica, vendedorId, empresaId, periodoEmDias);
            
            // Tentar obter do cache primeiro
            var valorCache = await _redisCacheService.GetAsync<decimal?>(cacheKey);
            if (valorCache.HasValue)
            {
                _logger.LogDebug("{NomeMetrica} obtida do Redis cache para vendedor {VendedorId}", 
                    nomeMetrica, vendedorId);
                return valorCache.Value;
            }
            
            _logger.LogDebug("Calculando {NomeMetrica} para vendedor {VendedorId}, empresa {EmpresaId}, período {Periodo} dias", 
                nomeMetrica, vendedorId, empresaId, periodoEmDias);
            
            try
            {
                // Executar o cálculo
                var resultado = await calcularMetrica();
                
                // Armazenar no cache
                var ttl = TimeSpan.FromMinutes(CACHE_TTL_MINUTOS);
                await _redisCacheService.SetAsync(cacheKey, resultado, ttl);
                
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular {NomeMetrica} do vendedor {VendedorId}", 
                    nomeMetrica, vendedorId);
                return 0;
            }
        }

        /// <summary>
        /// Gera uma chave de cache padronizada
        /// </summary>
        private static string GerarChaveCache(string tipoMetrica, int vendedorId, int empresaId, int periodoEmDias)
        {
            return $"{CACHE_PREFIX}:{tipoMetrica}:{vendedorId}:{empresaId}:{periodoEmDias}";
        }
    }
}
