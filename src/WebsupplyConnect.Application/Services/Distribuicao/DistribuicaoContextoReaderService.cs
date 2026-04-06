using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Serviço responsável por preparar o contexto de distribuição
    /// Responsabilidade: Buscar e consolidar dados necessários para cálculo de distribuição
    /// Implementa métodos específicos para evitar dependência direta de repositórios
    /// </summary>
    public class DistribuicaoContextoReaderService : IDistribuicaoContextoReaderService
    {
        private readonly ILeadEstatisticasService _leadEstatisticasService;
        private readonly IMetricaVendedorService _metricaVendedorService;
        private readonly IFilaDistribuicaoService _filaDistribuicaoService;
        private readonly ILogger<DistribuicaoContextoReaderService> _logger;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public DistribuicaoContextoReaderService(
            ILeadEstatisticasService leadEstatisticasService,
            IMetricaVendedorService metricaVendedorService,
            IFilaDistribuicaoService filaDistribuicaoService,
            ILogger<DistribuicaoContextoReaderService> logger)
        {
            _leadEstatisticasService = leadEstatisticasService ?? throw new ArgumentNullException(nameof(leadEstatisticasService));
            _metricaVendedorService = metricaVendedorService ?? throw new ArgumentNullException(nameof(metricaVendedorService));
            _filaDistribuicaoService = filaDistribuicaoService ?? throw new ArgumentNullException(nameof(filaDistribuicaoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Prepara o contexto completo para cálculo de distribuição
        /// </summary>
        public async Task<DistribuicaoContextDTO> PrepararContextoAsync(int leadId, int vendedorId, string tipoRegra)
        {
            try
            {
                // 1. Buscar dados básicos do lead
                var lead = await _leadEstatisticasService.ObterLeadPorIdAsync(leadId, false) ?? throw new ArgumentException($"Lead {leadId} não encontrado");
                var context = new DistribuicaoContextDTO
                {
                    LeadId = leadId,
                    EmpresaId = lead.EmpresaId,
                    VendedorId = vendedorId
                };

                // 2. Buscar dados específicos baseados no tipo de regra
                await PreencherContextoEspecificoAsync(context, tipoRegra);

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar contexto de distribuição para lead {LeadId}, vendedor {VendedorId}", 
                    leadId, vendedorId);
                throw;
            }
        }

        /// <summary>
        /// Verifica se um vendedor pode receber leads baseado no contexto
        /// </summary>
        public bool PodeReceberLead(DistribuicaoContextDTO context, string tipoRegra)
        {
            try
            {
                return tipoRegra switch
                {
                    "FILA" => PodeReceberPorFila(context),
                    "MERITO" => PodeReceberPorMetrica(context),
                    "TEMPO" => PodeReceberPorTempo(context),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se vendedor {VendedorId} pode receber lead {LeadId}",
                    context.VendedorId, context.LeadId);
                return false;
            }
        }

        /// <summary>
        /// Preenche contexto específico baseado no tipo de regra
        /// </summary>
        private async Task PreencherContextoEspecificoAsync(DistribuicaoContextDTO context, string tipoRegra)
        {
            switch (tipoRegra)
            {
                case "FILA":
                    await PreencherContextoFilaAsync(context);
                    break;
                case "MERITO":
                    await PreencherContextoMeritoAsync(context);
                    break;
                case "TEMPO":
                    // Para regra de tempo, não precisamos de dados específicos além dos básicos
                    break;
                default:                    
                    break;
            }
        }

        /// <summary>
        /// Preenche contexto específico para regra de fila
        /// </summary>
        private async Task PreencherContextoFilaAsync(DistribuicaoContextDTO context)
        {
            var posicaoFila = await _filaDistribuicaoService.ObterPosicaoVendedorAsync(context.EmpresaId, context.VendedorId);
            
            if (posicaoFila != null)
            {
                var statusFila = await _filaDistribuicaoService.ObterStatusFilaAsync(posicaoFila.StatusFilaDistribuicaoId);
                
                context.PosicaoFila = new FilaDistribuicaoDTO
                {
                    PosicaoFila = posicaoFila.PosicaoFila,
                    DataUltimoLeadRecebido = posicaoFila.DataUltimoLeadRecebido,
                    StatusFilaDistribuicaoId = posicaoFila.StatusFilaDistribuicaoId,
                    PermiteRecebimento = statusFila?.PermiteRecebimento ?? false
                };
            }
        }

        /// <summary>
        /// Preenche contexto específico para regra de mérito
        /// </summary>
        private async Task PreencherContextoMeritoAsync(DistribuicaoContextDTO context)
        {
            var metricas = await _metricaVendedorService.ObterMetricaVendedorAsync(context.VendedorId, context.EmpresaId);
            
            if (metricas != null)
            {
                context.MetricaVendedor = new MetricaVendedorDTO
                {
                    TaxaConversao = metricas.TaxaConversao,
                    VelocidadeAtendimento = metricas.VelocidadeMediaAtendimento,
                    TaxaPerdaInatividade = metricas.TaxaPerdaInatividade,
                    QuantidadeLeadsAtivos = metricas.LeadsAtivosAtual
                };
            }
        }

        /// <summary>
        /// Verifica se pode receber lead por fila
        /// </summary>
        private static bool PodeReceberPorFila(DistribuicaoContextDTO context)
        {
            return context.PosicaoFila?.PermiteRecebimento ?? false;
        }

        /// <summary>
        /// Verifica se pode receber lead por métricas
        /// </summary>
        private static bool PodeReceberPorMetrica(DistribuicaoContextDTO context)
        {
            // Exemplo: vendedor pode receber se não tem muitos leads ativos
            return context.MetricaVendedor?.QuantidadeLeadsAtivos < 50;
        }

        /// <summary>
        /// Verifica se pode receber lead por tempo
        /// </summary>
        private static bool PodeReceberPorTempo(DistribuicaoContextDTO context)
        {
            // Para regra de tempo, sempre pode receber (seria verificado na strategy)
            return true;
        }
    }
}
