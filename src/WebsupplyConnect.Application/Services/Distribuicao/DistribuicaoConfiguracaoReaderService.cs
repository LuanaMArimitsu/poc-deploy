using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de configuração de distribuição
    /// Responsabilidade: Orquestrar configurações E regras como uma unidade agregada
    /// </summary>
    public class DistribuicaoConfiguracaoReaderService : IDistribuicaoConfiguracaoReaderService
    {
        private readonly IConfiguracaoDistribuicaoRepository _configuracaoRepository;
        private readonly IRegraDistribuicaoService _regraService;
        private readonly ILogger<DistribuicaoConfiguracaoReaderService> _logger;
        
        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public DistribuicaoConfiguracaoReaderService(
            IConfiguracaoDistribuicaoRepository configuracaoRepository,
            IRegraDistribuicaoService regraService,
            ILogger<DistribuicaoConfiguracaoReaderService> logger)
        {
            _configuracaoRepository = configuracaoRepository ?? throw new ArgumentNullException(nameof(configuracaoRepository));
            _regraService = regraService ?? throw new ArgumentNullException(nameof(regraService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém a configuração ativa de distribuição para uma empresa
        /// </summary>

        public async Task<ConfiguracaoDistribuicao?> GetConfiguracaoAtivaAsync(int empresaId)
        {
            try
            {
                var configuracao = await _configuracaoRepository.GetConfiguracaoAtivaAsync(empresaId);
                return configuracao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração ativa para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        /// <summary>
        /// Verifica se uma empresa possui configuração ativa
        /// </summary>
        public async Task<bool> PossuiConfiguracaoAtivaAsync(int empresaId)
        {

            try
            {
                var configuracao = await GetConfiguracaoAtivaAsync(empresaId);
                var possui = configuracao != null;

                return possui;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar configuração ativa para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        /// <summary>
        /// Obtém configuração e regras em uma única consulta
        /// </summary>
        public async Task<DistribuicaoConfigurationContext> GetConfiguracaoComRegrasAsync(int empresaId)
        {            
            try
            {
                var context = new DistribuicaoConfigurationContext();

                // Obter configuração ativa através do serviço especializado
                context.Configuracao = await GetConfiguracaoAtivaAsync(empresaId);
                
                // Se há configuração, obter regras através do serviço especializado
                if (context.Configuracao != null)
                {
                    context.Regras = await _regraService.GetRegrasAtivasPorConfiguracaoAsync(context.Configuracao.Id);
                }               
                
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter contexto de distribuição para empresa {EmpresaId}", empresaId);
                throw;
            }
        }
    }
}
