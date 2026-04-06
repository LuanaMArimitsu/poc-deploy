using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Application.Services.Distribuicao.Strategy
{
    /// <summary>
    /// Estratégia de distribuição baseada na posição na fila
    /// Responsabilidade: APENAS calcular scores baseados em dados da fila já preparados
    /// </summary>
    public class RegraDistribuicaoFilaStrategy : IRegraDistribuicaoStrategy
    {
        private readonly ILogger<RegraDistribuicaoFilaStrategy> _logger;
        
        /// <summary>
        /// Tipo de regra que esta estratégia implementa
        /// </summary>
        public string TipoRegra => "FILA";
        
        /// <summary>
        /// Construtor da estratégia
        /// </summary>
        public RegraDistribuicaoFilaStrategy(ILogger<RegraDistribuicaoFilaStrategy> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calcula o score de um vendedor segundo a regra de fila
        /// </summary>
        public decimal CalcularScore(DistribuicaoContextDTO context, RegraDistribuicao regra)
        {
            _logger.LogDebug("Calculando score de fila para vendedor {VendedorId}, lead {LeadId}", 
                context.VendedorId, context.LeadId);
            
            // Verificar se temos dados de fila
            if (context.PosicaoFila == null)
            {
                _logger.LogWarning("Dados de fila não disponíveis para vendedor {VendedorId}", context.VendedorId);
                return 0;
            }
            
            // Obter parâmetros da regra
            var parametros = regra.Parametros.ToDictionary(p => p.NomeParametro, p => p.ValorParametro);
                
            // Score baseado na posição (menor posição = maior score)
            decimal posicaoMaxima = GetParametroDecimal(parametros, "POSICAO_MAXIMA", 20m);
            decimal scorePosicao = Math.Max(0, 100 - (context.PosicaoFila.PosicaoFila / posicaoMaxima * 100));
            
            // Score baseado no tempo desde último lead
            decimal scoreTempoEspera = CalcularScoreTempoEspera(context.PosicaoFila, parametros);
            
            // Pesos dos componentes
            decimal pesoPosicao = GetParametroDecimal(parametros, "PESO_POSICAO", 70m);
            decimal pesoTempoEspera = GetParametroDecimal(parametros, "PESO_TEMPO_ESPERA", 30m);
            
            // Cálculo final ponderado
            decimal score = (scorePosicao * pesoPosicao / 100) + (scoreTempoEspera * pesoTempoEspera / 100);
                
            _logger.LogDebug("Score de fila calculado: {Score} para vendedor {VendedorId} " +
                           "(posição: {Posicao}, scorePosicao: {ScorePosicao}, scoreTempoEspera: {ScoreTempoEspera})", 
                score, context.VendedorId, context.PosicaoFila.PosicaoFila, scorePosicao, scoreTempoEspera);
            
            return Math.Max(0, Math.Min(score, 100));
        }
        
        /// <summary>
        /// Verifica se a regra de fila pode ser aplicada baseada no contexto
        /// </summary>
        public bool PodeAplicarRegra(DistribuicaoContextDTO context, RegraDistribuicao regra)
        {
            _logger.LogDebug("Verificando se regra de fila pode ser aplicada para vendedor {VendedorId}, lead {LeadId}", 
                context.VendedorId, context.LeadId);

            // Verificar se temos dados de fila e se permite recebimento
            if (context.PosicaoFila == null)
            {
                _logger.LogDebug("Vendedor {VendedorId} não está na fila da empresa {EmpresaId}", 
                    context.VendedorId, context.EmpresaId);
                return false;
            }
                
            var podeReceber = context.PosicaoFila.PermiteRecebimento;
            
            _logger.LogDebug("Regra de fila para vendedor {VendedorId}: pode receber = {PodeReceber}", 
                context.VendedorId, podeReceber);
                
            return podeReceber;
        }
        
        /// <summary>
        /// Calcula o score baseado no tempo de espera desde o último lead
        /// </summary>
        private decimal CalcularScoreTempoEspera(FilaDistribuicaoDTO posicaoFila, Dictionary<string, string> parametros)
        {
            if (posicaoFila.DataUltimoLeadRecebido.HasValue)
            {
                var tempoEspera = (TimeHelper.GetBrasiliaTime() - posicaoFila.DataUltimoLeadRecebido.Value).TotalHours;
                decimal tempoEsperaMaximo = GetParametroDecimal(parametros, "TEMPO_ESPERA_MAXIMO", 24m);
                return Math.Min((decimal)tempoEspera / tempoEsperaMaximo * 100, 100);
            }
            else
            {
                // Se nunca recebeu lead, dá score máximo para tempo de espera
                return 100;
            }
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