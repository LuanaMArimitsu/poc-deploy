using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do provider de estratégias de distribuição
    /// Responsabilidade: Centralizar acesso a estratégias de distribuição
    /// </summary>
    public class RegraDistribuicaoProvider : IRegraDistribuicaoProvider
    {
        private readonly Dictionary<string, IRegraDistribuicaoStrategy> _strategies;
        private readonly ILogger<RegraDistribuicaoProvider> _logger;
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// Construtor do provider
        /// </summary>
        public RegraDistribuicaoProvider(
            IEnumerable<IRegraDistribuicaoStrategy> strategies,
            ILogger<RegraDistribuicaoProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Inicializa o dicionário com as estratégias registradas por injeção de dependência
            _strategies = new Dictionary<string, IRegraDistribuicaoStrategy>(StringComparer.OrdinalIgnoreCase);
            
            var strategiesList = strategies?.ToList() ?? throw new ArgumentNullException(nameof(strategies));
            
            if (!strategiesList.Any())
            {
                _logger.LogWarning("Nenhuma estratégia de distribuição foi registrada no DI");
            }
            
            foreach (var strategy in strategiesList)
            {
                if (strategy == null)
                {
                    _logger.LogWarning("Estratégia nula encontrada na coleção de strategies");
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(strategy.TipoRegra))
                {
                    _logger.LogWarning("Estratégia com TipoRegra nulo ou vazio: {StrategyType}", strategy.GetType().Name);
                    continue;
                }
                
                // Registrar tanto pelo nome quanto por possíveis IDs numéricos
                _strategies[strategy.TipoRegra] = strategy;
                _logger.LogDebug("Estratégia registrada: {TipoRegra} -> {StrategyType}", strategy.TipoRegra, strategy.GetType().Name);
                
                // Registro adicional por ID numérico para compatibilidade
                RegisterNumericMapping(strategy);
            }
            
            _logger.LogInformation("Provider inicializado com {Count} estratégias", _strategies.Count);
        }
        
        /// <summary>
        /// Obtém a estratégia adequada para o tipo de regra especificado
        /// </summary>
        public IRegraDistribuicaoStrategy? GetStrategy(string tipoRegra)
        {
            if (string.IsNullOrWhiteSpace(tipoRegra))
            {
                _logger.LogWarning("Tipo de regra nulo ou vazio solicitado");
                return null;
            }
            
            if (_strategies.TryGetValue(tipoRegra, out var strategy))
            {
                _logger.LogDebug("Estratégia encontrada para {TipoRegra}: {StrategyType}", tipoRegra, strategy.GetType().Name);
                return strategy;
            }
            
            _logger.LogWarning("Nenhuma estratégia encontrada para o tipo de regra '{TipoRegra}'. Estratégias disponíveis: [{Estrategias}]", 
                tipoRegra, string.Join(", ", _strategies.Keys));
            return null;
        }
        
        /// <summary>
        /// Registra uma estratégia no provider
        /// </summary>
        public void RegisterStrategy(IRegraDistribuicaoStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }
            
            if (string.IsNullOrWhiteSpace(strategy.TipoRegra))
            {
                throw new ArgumentException("Tipo de regra não pode ser nulo ou vazio", nameof(strategy));
            }
            
            lock (_lockObject)
            {
                _strategies[strategy.TipoRegra] = strategy;
                RegisterNumericMapping(strategy);
            }
            
            _logger.LogDebug("Estratégia registrada manualmente: {TipoRegra} -> {StrategyType}", 
                strategy.TipoRegra, strategy.GetType().Name);
        }
        
        /// <summary>
        /// Obtém todos os tipos de regra disponíveis
        /// </summary>
        public IReadOnlyCollection<string> GetAvailableRuleTypes()
        {
            lock (_lockObject)
            {
                var ruleTypes = _strategies.Keys
                    .Where(key => !char.IsDigit(key[0])) // Filtrar IDs numéricos
                    .OrderBy(key => key)
                    .ToList();
                    
                _logger.LogDebug("Tipos de regra disponíveis: [{RuleTypes}]", string.Join(", ", ruleTypes));
                return ruleTypes.AsReadOnly();
            }
        }
        
        /// <summary>
        /// Verifica se uma estratégia está disponível para o tipo especificado
        /// </summary>
        public bool IsStrategyAvailable(string tipoRegra)
        {
            if (string.IsNullOrWhiteSpace(tipoRegra))
            {
                return false;
            }
            
            bool available = _strategies.ContainsKey(tipoRegra);
            _logger.LogDebug("Estratégia {TipoRegra} está {Status}", 
                tipoRegra, available ? "disponível" : "indisponível");
            return available;
        }
        
        /// <summary>
        /// Registra mapeamentos numéricos para compatibilidade com IDs de banco
        /// </summary>
        private void RegisterNumericMapping(IRegraDistribuicaoStrategy strategy)
        {
            // Mapeamento baseado nos tipos conhecidos para compatibilidade com banco de dados
            var numericMapping = strategy.TipoRegra.ToUpperInvariant() switch
            {
                "MERITO" => "1",
                "FILA" => "2", 
                "TEMPO" => "3",
                _ => null
            };
            
            if (!string.IsNullOrEmpty(numericMapping))
            {
                _strategies[numericMapping] = strategy;
                _logger.LogDebug("Mapeamento numérico criado: {NumericId} -> {TipoRegra}", numericMapping, strategy.TipoRegra);
            }
        }
    }
}