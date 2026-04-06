using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy;
using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Estratégia de distribuição baseada no mérito do vendedor (SIMPLIFICADA)
    /// Responsabilidade: APENAS calcular scores baseados em métricas já preparadas
    /// Utiliza apenas taxa de conversão e tempo médio de resposta
    /// </summary>
    public class RegraDistribuicaoMeritoStrategy : IRegraDistribuicaoStrategy
    {
        private readonly ILogger<RegraDistribuicaoMeritoStrategy> _logger;
        
        /// <summary>
        /// Constantes para parâmetros padrão (SIMPLIFICADOS)
        /// </summary>
        private const decimal TEMPO_RESPOSTA_IDEAL_PADRAO = 5m; // 5 minutos
        private const decimal TEMPO_RESPOSTA_MAXIMO_PADRAO = 60m; // 60 minutos
        private const decimal FATOR_CONVERSAO = 2.5m;
        
        /// <summary>
        /// Tipo de regra que esta estratégia implementa
        /// </summary>
        public string TipoRegra => "MERITO";
        
        /// <summary>
        /// Construtor da estratégia
        /// </summary>
        public RegraDistribuicaoMeritoStrategy(ILogger<RegraDistribuicaoMeritoStrategy> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calcula o score de um vendedor segundo a regra de mérito (SIMPLIFICADO)
        /// </summary>
        public decimal CalcularScore(DistribuicaoContextDTO context, RegraDistribuicao regra)
        {
            _logger.LogDebug("Calculando score de mérito para vendedor {VendedorId}, lead {LeadId}", 
                context.VendedorId, context.LeadId);
            
            // Verificar se temos dados de métricas
            if (context.MetricaVendedor == null)
            {
                _logger.LogWarning("Dados de métricas não disponíveis para vendedor {VendedorId}", context.VendedorId);
                return 0;
            }
            
            // Obter parâmetros da regra
            var parametros = regra.Parametros.ToDictionary(p => p.NomeParametro, p => p.ValorParametro);
            
            // Score baseado na taxa de conversão
            decimal scoreConversao = Math.Min(context.MetricaVendedor.TaxaConversao * FATOR_CONVERSAO, 100);
            
            // Score baseado no tempo médio de resposta
            decimal tempoIdeal = GetParametroDecimal(parametros, "TEMPO_RESPOSTA_IDEAL", TEMPO_RESPOSTA_IDEAL_PADRAO);
            decimal tempoMaximo = GetParametroDecimal(parametros, "TEMPO_RESPOSTA_MAXIMO", TEMPO_RESPOSTA_MAXIMO_PADRAO);
            
            decimal scoreTempoResposta = 100;
            if (context.MetricaVendedor.VelocidadeAtendimento > tempoIdeal)
            {
                decimal penalidade = Math.Min(
                    (context.MetricaVendedor.VelocidadeAtendimento - tempoIdeal) / 
                    (tempoMaximo - tempoIdeal),
                    1) * 100;
                
                scoreTempoResposta = 100 - penalidade;
            }
            
            // Score final: média simples entre conversão e tempo de resposta (SIMPLIFICADO)
            decimal score = (scoreConversao + scoreTempoResposta) / 2;
                
            _logger.LogDebug("Score de mérito calculado: {Score} para vendedor {VendedorId} " +
                           "(conversão: {Conversao}, tempoResposta: {TempoResposta})", 
                score, context.VendedorId, scoreConversao, scoreTempoResposta);
            
            return Math.Max(0, Math.Min(score, 100));
        }
        
        /// <summary>
        /// Verifica se a regra de mérito pode ser aplicada baseada no contexto
        /// </summary>
        public bool PodeAplicarRegra(DistribuicaoContextDTO context, RegraDistribuicao regra)
        {
            _logger.LogDebug("Verificando se regra de mérito pode ser aplicada para vendedor {VendedorId}, lead {LeadId}", 
                context.VendedorId, context.LeadId);

            // Verificar se temos dados de métricas
            if (context.MetricaVendedor == null)
            {
                _logger.LogDebug("Vendedor {VendedorId} não possui métricas disponíveis", context.VendedorId);
                return false;
            }
                
            // Verificar score mínimo, se definido
            if (regra.PontuacaoMinima.HasValue)
            {
                decimal score = CalcularScore(context, regra);
                if (score < regra.PontuacaoMinima.Value)
                {
                    _logger.LogDebug("Score {Score} abaixo do mínimo {Minimo} para vendedor {VendedorId}", 
                        score, regra.PontuacaoMinima.Value, context.VendedorId);
                    return false;
                }
            }
            
            _logger.LogDebug("Regra de mérito pode ser aplicada para vendedor {VendedorId}", context.VendedorId);
            return true;
        }
        
        /// <summary>
        /// Obtém um parâmetro decimal do dicionário de parâmetros
        /// </summary>
        private static decimal GetParametroDecimal(Dictionary<string, string> parametros, string nome, decimal valorPadrao)
        {
            if (parametros.TryGetValue(nome, out var valor) && decimal.TryParse(valor, out var resultado))
            {
                return resultado;
            }
            return valorPadrao;
        }
    }
}