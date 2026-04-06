using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Estratégia de distribuição baseada no tempo e carga de trabalho (SIMPLIFICADA)
    /// Responsabilidade: APENAS calcular scores baseados em carga máxima e horário comercial
    /// Considera: carga atual do vendedor e horário comercial
    /// </summary>
    public class RegraDistribuicaoTempoStrategy : IRegraDistribuicaoStrategy
    {
        private readonly ILogger<RegraDistribuicaoTempoStrategy> _logger;
        
        /// <summary>
        /// Constantes para parâmetros padrão (SIMPLIFICADOS)
        /// </summary>
        private const decimal CARGA_MAXIMA_PADRAO = 10m;
        private const decimal HORA_INICIO_PADRAO = 8m; // 8h
        private const decimal HORA_FIM_PADRAO = 18m; // 18h
        private const decimal SCORE_HORARIO_COMERCIAL = 100m;
        private const decimal SCORE_FORA_HORARIO = 50m;
        
        /// <summary>
        /// Tipo de regra que esta estratégia implementa
        /// </summary>
        public string TipoRegra => "TEMPO";
        
        /// <summary>
        /// Construtor da estratégia
        /// </summary>
        public RegraDistribuicaoTempoStrategy(ILogger<RegraDistribuicaoTempoStrategy> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calcula o score de um vendedor segundo a regra de tempo (SIMPLIFICADO)
        /// </summary>
        public decimal CalcularScore(DistribuicaoContextDTO context, RegraDistribuicao regra)
        {
            _logger.LogDebug("Calculando score de tempo para vendedor {VendedorId}, lead {LeadId}", 
                context.VendedorId, context.LeadId);
            
            // Obter parâmetros da regra
            var parametros = regra.Parametros.ToDictionary(p => p.NomeParametro, p => p.ValorParametro);
            decimal cargaMaxima = GetParametroDecimal(parametros, "CARGA_MAXIMA", CARGA_MAXIMA_PADRAO);
            
            // Score baseado na carga de trabalho (menor carga = maior score)
            var quantidadeLeadsAtivos = context.MetricaVendedor?.QuantidadeLeadsAtivos ?? 0;
            decimal scoreCarga = Math.Max(0, 100 - (quantidadeLeadsAtivos / cargaMaxima * 100));
            
            // Score baseado no horário comercial
            var agora = TimeHelper.GetBrasiliaTime();
            decimal scoreHorario = CalcularScoreHorario(agora, parametros);
            
            // Score final: média simples entre carga e horário (SIMPLIFICADO)
            decimal score = (scoreCarga + scoreHorario) / 2;
                
            _logger.LogDebug("Score de tempo calculado: {Score} para vendedor {VendedorId} " +
                           "(carga: {QuantidadeLeads}, scoreCarga: {ScoreCarga}, scoreHorario: {ScoreHorario})", 
                score, context.VendedorId, quantidadeLeadsAtivos, scoreCarga, scoreHorario);
            
            return Math.Max(0, Math.Min(score, 100));
        }
        
        /// <summary>
        /// Verifica se a regra de tempo pode ser aplicada baseada no contexto
        /// </summary>
        public bool PodeAplicarRegra(DistribuicaoContextDTO context, RegraDistribuicao regra)
        {
            _logger.LogDebug("Verificando se regra de tempo pode ser aplicada para vendedor {VendedorId}, lead {LeadId}", 
                context.VendedorId, context.LeadId);

            // Obter parâmetros da regra
            var parametros = regra.Parametros.ToDictionary(p => p.NomeParametro, p => p.ValorParametro);
            
            // Verificar carga máxima
            decimal cargaMaxima = GetParametroDecimal(parametros, "CARGA_MAXIMA", CARGA_MAXIMA_PADRAO);
            var quantidadeLeadsAtivos = context.MetricaVendedor?.QuantidadeLeadsAtivos ?? 0;
            
            if (quantidadeLeadsAtivos >= cargaMaxima)
            {
                _logger.LogDebug("Vendedor {VendedorId} com carga {Carga} acima do máximo {Maximo}", 
                    context.VendedorId, quantidadeLeadsAtivos, cargaMaxima);
                return false;
            }
                
            _logger.LogDebug("Regra de tempo pode ser aplicada para vendedor {VendedorId}", context.VendedorId);
            return true;
        }

        /// <summary>
        /// Calcula o score baseado no horário atual
        /// </summary>
        private static decimal CalcularScoreHorario(DateTime agora, Dictionary<string, string> parametros)
        {
            var horaInicio = GetParametroDecimal(parametros, "HORA_INICIO", HORA_INICIO_PADRAO);
            var horaFim = GetParametroDecimal(parametros, "HORA_FIM", HORA_FIM_PADRAO);
            var horaAtual = agora.Hour + (agora.Minute / 60m);
            
            // Se estiver dentro do horário comercial, score máximo
            if (horaAtual >= horaInicio && horaAtual <= horaFim)
            {
                return SCORE_HORARIO_COMERCIAL;
            }
            
            // Fora do horário, score reduzido
            return SCORE_FORA_HORARIO;
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