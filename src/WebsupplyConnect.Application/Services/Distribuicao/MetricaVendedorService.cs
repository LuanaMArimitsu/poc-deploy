using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de métricas de vendedor
    /// Responsabilidade: Gerenciar APENAS métricas de vendedor (CRUD e atualizações)
    /// </summary>
    public class MetricaVendedorService : IMetricaVendedorService
    {
        private readonly IMetricaVendedorRepository _metricaRepository;
        private readonly IVendedorEstatisticasService _estatisticasService;
        private readonly IMetricaCacheService _cacheService;
        private readonly ILogger<MetricaVendedorService> _logger;
        
        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public MetricaVendedorService(
            IMetricaVendedorRepository metricaRepository,
            IVendedorEstatisticasService estatisticasService,
            IMetricaCacheService cacheService,
            ILogger<MetricaVendedorService> logger)
        {
            _metricaRepository = metricaRepository ?? throw new ArgumentNullException(nameof(metricaRepository));
            _estatisticasService = estatisticasService ?? throw new ArgumentNullException(nameof(estatisticasService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Atualiza as métricas de um vendedor após atribuição
        /// </summary>
        public async Task AtualizarMetricasVendedorAposAtribuicaoAsync(int vendedorId, int empresaId)
        {
            _logger.LogDebug("Atualizando métricas após atribuição. Vendedor: {VendedorId}, Empresa: {EmpresaId}", 
                vendedorId, empresaId);
                
            try
            {
                // Buscar métrica atual ou criar nova
                var metrica = await _metricaRepository.GetMetricaVendedorAsync(vendedorId, empresaId);
                if (metrica == null)
                {
                    metrica = await _metricaRepository.InicializarMetricaVendedorAsync(vendedorId, empresaId);
                }

                // Incrementar contador de leads recebidos usando os métodos públicos da entidade
                metrica.IncrementarLeadsRecebidos();
                metrica.IncrementarLeadsAtivos();

                // Salvar
                await _metricaRepository.UpdateMetricaAsync(metrica);
                
                // Invalidar cache através do serviço especializado
                _cacheService.InvalidarCacheVendedor(vendedorId, empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar métricas do vendedor {VendedorId} após atribuição", vendedorId);
                // Não propaga exceção para não interromper o fluxo principal
            }
        }
        
        /// <summary>
        /// Atualiza as métricas de conversão de um vendedor
        /// </summary>
        public async Task AtualizarMetricasConversaoAsync(int vendedorId, int empresaId, bool convertido)
        {
            try
            {
                // Buscar métrica atual ou criar nova
                var metrica = await _metricaRepository.GetMetricaVendedorAsync(vendedorId, empresaId);
                if (metrica == null)
                {
                    metrica = await _metricaRepository.InicializarMetricaVendedorAsync(vendedorId, empresaId);
                }

                // Atualizar contadores - Use os métodos corretos da entidade
                metrica.DecrementarLeadsAtivos(); // Para indicar que o lead não está mais ativo
                
                if (convertido)
                {
                    metrica.IncrementarConversoes(); // Em vez de IncrementarLeadsConvertidos
                }
                else
                {
                    metrica.IncrementarPerdas(); // Em vez de IncrementarLeadsPerdidos
                }

                // Salvar
                await _metricaRepository.UpdateMetricaAsync(metrica);
                
                // Invalidar cache através do serviço especializado
                _cacheService.InvalidarCacheVendedor(vendedorId, empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar métricas de conversão do vendedor {VendedorId}", vendedorId);
                // Não propaga exceção para não interromper o fluxo principal
            }
        }
        
        /// <summary>
        /// Calcula a taxa de conversão de um vendedor (delegação para VendedorEstatisticasService)
        /// </summary>
        public async Task<decimal> CalcularTaxaConversaoAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            return await _estatisticasService.CalcularTaxaConversaoAsync(vendedorId, empresaId, periodoEmDias);
        }
        
        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor (delegação para VendedorEstatisticasService)
        /// </summary>
        public async Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            return await _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedorId, empresaId, periodoEmDias);
        }
        
        /// <summary>
        /// Calcula a taxa de perda por inatividade de um vendedor (delegação para VendedorEstatisticasService)
        /// </summary>
        public async Task<decimal> CalcularTaxaPerdaInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            return await _estatisticasService.CalcularTaxaPerdaInatividadeAsync(vendedorId, empresaId, periodoEmDias);
        }

        /// <summary>
        /// Obtém as métricas de um vendedor
        /// </summary>
        public async Task<WebsupplyConnect.Domain.Entities.Distribuicao.MetricaVendedor?> ObterMetricaVendedorAsync(int vendedorId, int empresaId)
        {
            _logger.LogDebug("Obtendo métricas do vendedor {VendedorId} da empresa {EmpresaId}", vendedorId, empresaId);

            try
            {
                return await _metricaRepository.GetMetricaVendedorAsync(vendedorId, empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas do vendedor {VendedorId} da empresa {EmpresaId}", vendedorId, empresaId);
                return null;
            }
        }
    }
}