using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Interfaces.Distribuicao;

namespace WebsupplyConnect.API.Controllers.Distribuicao
{
    /// <summary>
    /// Controller para gerenciamento de métricas de vendedores
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MetricaVendedorController : ControllerBase
    {
        private readonly IMetricaVendedorService _metricaService;
        private readonly IVendedorEstatisticasService _estatisticasService;
        private readonly ILogger<MetricaVendedorController> _logger;

        /// <summary>
        /// Construtor do controller
        /// </summary>
        public MetricaVendedorController(
            IMetricaVendedorService metricaService,
            IVendedorEstatisticasService estatisticasService,
            ILogger<MetricaVendedorController> logger)
        {
            _metricaService = metricaService ?? throw new ArgumentNullException(nameof(metricaService));
            _estatisticasService = estatisticasService ?? throw new ArgumentNullException(nameof(estatisticasService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Atualiza as métricas de um vendedor após atribuição de lead
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AtualizarMetricasAposAtribuicao")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarMetricasAposAtribuicao(
            [FromQuery] int vendedorId,
            [FromQuery] int empresaId)
        {
            try
            {
                _logger.LogInformation("Atualizando métricas do vendedor {VendedorId} após atribuição", vendedorId);
                
                await _metricaService.AtualizarMetricasVendedorAposAtribuicaoAsync(vendedorId, empresaId);
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { vendedorId, empresaId }, 
                    $"Métricas do vendedor {vendedorId} atualizadas com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar métricas do vendedor {VendedorId}", vendedorId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao atualizar métricas do vendedor", ex.Message));
            }
        }

        /// <summary>
        /// Atualiza as métricas de conversão de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="convertido">Indica se o lead foi convertido (true) ou perdido (false)</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AtualizarMetricasConversao")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarMetricasConversao(
            [FromQuery] int vendedorId,
            [FromQuery] int empresaId,
            [FromQuery] bool convertido)
        {
            try
            {
                _logger.LogInformation("Atualizando métricas de conversão do vendedor {VendedorId}. Convertido: {Convertido}", 
                    vendedorId, convertido);
                
                await _metricaService.AtualizarMetricasConversaoAsync(vendedorId, empresaId, convertido);
                
                string resultado = convertido ? "conversão" : "perda";
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { vendedorId, empresaId, convertido, resultado }, 
                    $"Métricas de {resultado} do vendedor {vendedorId} atualizadas com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar métricas de conversão do vendedor {VendedorId}", vendedorId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao atualizar métricas de conversão", ex.Message));
            }
        }

        /// <summary>
        /// Calcula a taxa de conversão de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para o cálculo (padrão: 30)</param>
        /// <returns>Taxa de conversão calculada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("CalcularTaxaConversao")]
        public async Task<ActionResult<ApiResponse<decimal>>> CalcularTaxaConversao(
            [FromQuery] int vendedorId,
            [FromQuery] int empresaId,
            [FromQuery] int periodoEmDias = 30)
        {
            try
            {
                _logger.LogInformation("Calculando taxa de conversão do vendedor {VendedorId} para período de {Dias} dias", 
                    vendedorId, periodoEmDias);
                
                var taxaConversao = await _metricaService.CalcularTaxaConversaoAsync(vendedorId, empresaId, periodoEmDias);
                
                return Ok(ApiResponse<decimal>.SuccessResponse(
                    taxaConversao, 
                    $"Taxa de conversão do vendedor {vendedorId}: {taxaConversao:F2}%"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular taxa de conversão do vendedor {VendedorId}", vendedorId);
                return StatusCode(500, ApiResponse<decimal>.ErrorResponse(
                    "Erro ao calcular taxa de conversão", ex.Message));
            }
        }

        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para o cálculo (padrão: 30)</param>
        /// <returns>Velocidade média de atendimento calculada (em minutos)</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("CalcularVelocidadeMediaAtendimento")]
        public async Task<ActionResult<ApiResponse<decimal>>> CalcularVelocidadeMediaAtendimento(
            [FromQuery] int vendedorId,
            [FromQuery] int empresaId,
            [FromQuery] int periodoEmDias = 30)
        {
            try
            {
                _logger.LogInformation("Calculando velocidade média de atendimento do vendedor {VendedorId} para período de {Dias} dias", 
                    vendedorId, periodoEmDias);
                
                var velocidadeMedia = await _metricaService.CalcularVelocidadeMediaAtendimentoAsync(
                    vendedorId, empresaId, periodoEmDias);
                
                return Ok(ApiResponse<decimal>.SuccessResponse(
                    velocidadeMedia, 
                    $"Velocidade média de atendimento do vendedor {vendedorId}: {velocidadeMedia:F2} minutos"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular velocidade média de atendimento do vendedor {VendedorId}", vendedorId);
                return StatusCode(500, ApiResponse<decimal>.ErrorResponse(
                    "Erro ao calcular velocidade média de atendimento", ex.Message));
            }
        }

        /// <summary>
        /// Calcula a taxa de perda por inatividade de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para o cálculo (padrão: 30)</param>
        /// <returns>Taxa de perda por inatividade calculada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("CalcularTaxaPerdaInatividade")]
        public async Task<ActionResult<ApiResponse<decimal>>> CalcularTaxaPerdaInatividade(
            [FromQuery] int vendedorId,
            [FromQuery] int empresaId,
            [FromQuery] int periodoEmDias = 30)
        {
            try
            {
                _logger.LogInformation("Calculando taxa de perda por inatividade do vendedor {VendedorId} para período de {Dias} dias", 
                    vendedorId, periodoEmDias);
                
                var taxaPerda = await _metricaService.CalcularTaxaPerdaInatividadeAsync(
                    vendedorId, empresaId, periodoEmDias);
                
                return Ok(ApiResponse<decimal>.SuccessResponse(
                    taxaPerda, 
                    $"Taxa de perda por inatividade do vendedor {vendedorId}: {taxaPerda:P2}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular taxa de perda por inatividade do vendedor {VendedorId}", vendedorId);
                return StatusCode(500, ApiResponse<decimal>.ErrorResponse(
                    "Erro ao calcular taxa de perda por inatividade", ex.Message));
            }
        }

        /// <summary>
        /// Obtém estatísticas completas de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para as estatísticas (padrão: 30)</param>
        /// <returns>Estatísticas completas do vendedor</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("EstatisticasCompletas")]
        public async Task<ActionResult<ApiResponse<object>>> ObterEstatisticasCompletas(
            [FromQuery] int vendedorId,
            [FromQuery] int empresaId,
            [FromQuery] int periodoEmDias = 30)
        {
            try
            {
                _logger.LogInformation("Obtendo estatísticas completas do vendedor {VendedorId} para empresa {EmpresaId} nos últimos {PeriodoEmDias} dias", 
                    vendedorId, empresaId, periodoEmDias);

                // Calcular todas as estatísticas em paralelo
                var taskTaxaConversao = _estatisticasService.CalcularTaxaConversaoAsync(vendedorId, empresaId, periodoEmDias);
                var taskVelocidadeMedia = _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedorId, empresaId, periodoEmDias);
                var taskTaxaPerda = _estatisticasService.CalcularTaxaPerdaInatividadeAsync(vendedorId, empresaId, periodoEmDias);

                await Task.WhenAll(taskTaxaConversao, taskVelocidadeMedia, taskTaxaPerda);

                var estatisticas = new
                {
                    VendedorId = vendedorId,
                    EmpresaId = empresaId,
                    PeriodoEmDias = periodoEmDias,
                    DataCalculoUtc = DateTime.UtcNow,
                    TaxaConversao = new
                    {
                        Percentual = taskTaxaConversao.Result,
                        Formatado = $"{taskTaxaConversao.Result:P2}"
                    },
                    VelocidadeMediaAtendimento = new
                    {
                        Minutos = taskVelocidadeMedia.Result,
                        Formatado = $"{taskVelocidadeMedia.Result:F1} min"
                    },
                    TaxaPerdaInatividade = new
                    {
                        Percentual = taskTaxaPerda.Result,
                        Formatado = $"{taskTaxaPerda.Result:P2}"
                    },
                    Score = new
                    {
                        Performance = CalcularScorePerformance(taskTaxaConversao.Result, taskVelocidadeMedia.Result, taskTaxaPerda.Result),
                        Nivel = ClassificarNivelVendedor(taskTaxaConversao.Result, taskTaxaPerda.Result)
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    estatisticas, 
                    $"Estatísticas completas do vendedor {vendedorId} obtidas com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas completas do vendedor {VendedorId}", vendedorId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao obter estatísticas completas", ex.Message));
            }
        }

        /// <summary>
        /// Compara as estatísticas de múltiplos vendedores
        /// </summary>
        /// <param name="vendedorIds">IDs dos vendedores para comparar</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para as estatísticas (padrão: 30)</param>
        /// <returns>Comparativo de estatísticas dos vendedores</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("CompararEstatisticas")]
        public async Task<ActionResult<ApiResponse<object>>> CompararEstatisticas(
            [FromBody] List<int> vendedorIds,
            [FromQuery] int empresaId,
            [FromQuery] int periodoEmDias = 30)
        {
            try
            {
                if (vendedorIds == null || !vendedorIds.Any())
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Lista de vendedores inválida", 
                        "É necessário fornecer pelo menos um ID de vendedor"));
                }

                _logger.LogInformation("Comparando estatísticas de {Count} vendedores para empresa {EmpresaId}", 
                    vendedorIds.Count, empresaId);

                var comparativo = new List<object>();

                foreach (var vendedorId in vendedorIds)
                {
                    var taskTaxaConversao = _estatisticasService.CalcularTaxaConversaoAsync(vendedorId, empresaId, periodoEmDias);
                    var taskVelocidadeMedia = _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedorId, empresaId, periodoEmDias);
                    var taskTaxaPerda = _estatisticasService.CalcularTaxaPerdaInatividadeAsync(vendedorId, empresaId, periodoEmDias);

                    await Task.WhenAll(taskTaxaConversao, taskVelocidadeMedia, taskTaxaPerda);

                    comparativo.Add(new
                    {
                        VendedorId = vendedorId,
                        TaxaConversao = taskTaxaConversao.Result,
                        VelocidadeMedia = taskVelocidadeMedia.Result,
                        TaxaPerda = taskTaxaPerda.Result,
                        Score = CalcularScorePerformance(taskTaxaConversao.Result, taskVelocidadeMedia.Result, taskTaxaPerda.Result),
                        Nivel = ClassificarNivelVendedor(taskTaxaConversao.Result, taskTaxaPerda.Result)
                    });
                }

                var resultado = new
                {
                    EmpresaId = empresaId,
                    PeriodoEmDias = periodoEmDias,
                    DataComparativo = DateTime.UtcNow,
                    TotalVendedores = vendedorIds.Count,
                    Vendedores = comparativo.OrderByDescending(v => ((dynamic)v).Score).ToList(),
                    Resumo = new
                    {
                        MelhorTaxaConversao = comparativo.Max(v => ((dynamic)v).TaxaConversao),
                        MelhorVelocidade = comparativo.Min(v => ((dynamic)v).VelocidadeMedia),
                        MenorTaxaPerda = comparativo.Min(v => ((dynamic)v).TaxaPerda)
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    $"Comparativo de {vendedorIds.Count} vendedores realizado com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao comparar estatísticas dos vendedores");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao comparar estatísticas", ex.Message));
            }
        }

        /// <summary>
        /// Calcula score de performance baseado nas métricas
        /// </summary>
        private static decimal CalcularScorePerformance(decimal taxaConversao, decimal velocidadeMedia, decimal taxaPerda)
        {
            // Normaliza as métricas (0-100)
            var scoreConversao = Math.Min(taxaConversao * 100, 100);
            var scoreVelocidade = Math.Max(0, 100 - (velocidadeMedia / 60 * 10)); // Penaliza velocidade > 60min
            var scorePerda = Math.Max(0, 100 - (taxaPerda * 100));

            // Peso: 40% conversão, 30% velocidade, 30% perda
            return (scoreConversao * 0.4m + scoreVelocidade * 0.3m + scorePerda * 0.3m);
        }

        /// <summary>
        /// Classifica o nível do vendedor baseado nas métricas
        /// </summary>
        private static string ClassificarNivelVendedor(decimal taxaConversao, decimal taxaPerda)
        {
            var conversaoPercent = taxaConversao * 100;
            var perdaPercent = taxaPerda * 100;

            if (conversaoPercent >= 30 && perdaPercent <= 10)
                return "Excelente";
            if (conversaoPercent >= 20 && perdaPercent <= 20)
                return "Bom";
            if (conversaoPercent >= 10 && perdaPercent <= 30)
                return "Regular";
            
            return "Necessita Melhoria";
        }
    }
}