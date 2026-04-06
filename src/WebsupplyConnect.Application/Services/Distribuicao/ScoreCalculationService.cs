using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Usuario;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Serviço especializado em cálculo de scores de vendedores
    /// Responsabilidade: APENAS transformar resultados de distribuição em DTOs de score
    /// </summary>
    public class ScoreCalculationService : IScoreCalculationService
    {
        private readonly IDistribuicaoContextoReaderService _contextService;
        private readonly IRegraDistribuicaoProvider _regraDistribuicaoProvider;
        private readonly ILogger<ScoreCalculationService> _logger;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public ScoreCalculationService(
            IDistribuicaoContextoReaderService contextService,
            IRegraDistribuicaoProvider regraDistribuicaoProvider,
            ILogger<ScoreCalculationService> logger)
        {
            _contextService = contextService ?? throw new ArgumentNullException(nameof(contextService));
            _regraDistribuicaoProvider = regraDistribuicaoProvider ?? throw new ArgumentNullException(nameof(regraDistribuicaoProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calcula score para um vendedor específico
        /// </summary>
        public async Task<ScoreVendedorDTO> CalcularScoreVendedorAsync(
            int? leadId, 
            WebsupplyConnect.Domain.Entities.Usuario.Usuario vendedor, 
            int empresaId, 
            ConfiguracaoDistribuicao configuracao)
        {
            var scoreVendedor = new ScoreVendedorDTO
            {
                VendedorId = vendedor.Id,
                NomeVendedor = vendedor.Nome,
                Elegivel = true
            };

            try
            {
                if (leadId.HasValue)
                {
                    // Cálculo real de score com lead específico
                    var (scoreTotal, scoresPorRegra) = await CalcularScoreRealAsync(leadId.Value, vendedor.Id, configuracao);
                    scoreVendedor.ScoreTotal = scoreTotal;
                    scoreVendedor.ScoresPorRegra = scoresPorRegra;
                }
                else
                {
                    // Simulação sem lead real - score baseado em configuração
                    scoreVendedor.ScoreTotal = CalcularScoreSimulacao(vendedor, configuracao);
                    scoreVendedor.ScoresPorRegra = new List<ScoreRegraDTO>();
                }

                _logger.LogDebug("Score calculado para vendedor {VendedorId}: {Score}", 
                    vendedor.Id, scoreVendedor.ScoreTotal);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao calcular score para vendedor {VendedorId}", vendedor.Id);
                scoreVendedor.Elegivel = false;
                scoreVendedor.MotivoInelegibilidade = $"Erro no cálculo: {ex.Message}";
                scoreVendedor.ScoreTotal = 0;
            }

            return scoreVendedor;
        }

        /// <summary>
        /// Calcula scores para uma lista de vendedores
        /// </summary>
        public async Task<List<ScoreVendedorDTO>> CalcularScoresVendedoresAsync(
            int? leadId, 
            List<WebsupplyConnect.Domain.Entities.Usuario.Usuario> vendedores, 
            int empresaId, 
            ConfiguracaoDistribuicao configuracao)
        {
            _logger.LogDebug("Calculando scores para {Count} vendedores", vendedores.Count);

            var scores = new List<ScoreVendedorDTO>();

            foreach (var vendedor in vendedores)
            {
                var score = await CalcularScoreVendedorAsync(leadId, vendedor, empresaId, configuracao);
                scores.Add(score);
            }

            return scores;
        }

        /// <summary>
        /// Ordena scores e atribui posições
        /// </summary>
        public List<ScoreVendedorDTO> OrdenarEAtribuirPosicoes(List<ScoreVendedorDTO> scores)
        {
            return scores
                .OrderByDescending(s => s.ScoreTotal)
                .Select((s, i) => 
                {
                    s.Posicao = i + 1;
                    return s;
                })
                .ToList();
        }

        // MÉTODO REMOVIDO: MapearDetalhesScore
        // Não é mais necessário após simplificação da arquitetura

        /// <summary>
        /// Calcula score de simulação sem lead real
        /// </summary>
        private static decimal CalcularScoreSimulacao(WebsupplyConnect.Domain.Entities.Usuario.Usuario vendedor, ConfiguracaoDistribuicao configuracao)
        {
            // Score baseado em características do vendedor e configuração
            // Usar características determinísticas em vez de random
            
            var scoreBase = 50m; // Base neutra
            
            // Modificadores baseados em características do vendedor
            var modificadorId = (vendedor.Id % 10) * 2; // 0-18 pontos baseado no ID
            var modificadorAtivo = vendedor.Ativo ? 15 : -20; // Bonus por estar ativo
            
            // Modificadores baseados na configuração
            var modificadorConfiguracao = configuracao.Ativo ? 10 : -10;
            
            var scoreFinal = scoreBase + modificadorId + modificadorAtivo + modificadorConfiguracao;
            
            return Math.Max(0, Math.Min(scoreFinal, 100));
        }

        /// <summary>
        /// Calcula score real usando regras de distribuição
        /// (Movido do DistribuicaoService para eliminar dependência circular)
        /// </summary>
        private async Task<(decimal ScoreTotal, List<ScoreRegraDTO> ScoresPorRegra)> CalcularScoreRealAsync(
            int leadId, int vendedorId, ConfiguracaoDistribuicao configuracao)
        {
            _logger.LogDebug("Calculando score real para vendedor {VendedorId} e lead {LeadId}", vendedorId, leadId);

            // TODO: Implementar cálculo de score real usando regras
            // Por enquanto, retorna score de simulação
            var scoreSimulacao = 75m; // Score padrão para leads reais
            var scoresPorRegra = new List<ScoreRegraDTO>();

            _logger.LogWarning("Score real ainda não implementado. Usando score padrão: {Score}", scoreSimulacao);
            
            return (scoreSimulacao, scoresPorRegra);
        }
    }
}
