using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    /// <summary>
    /// Implementação do serviço de estatísticas de leads
    /// Responsabilidade: Encapsular APENAS operações estatísticas relacionadas a leads
    /// </summary>
    public class LeadEstatisticasService : ILeadEstatisticasService
    {
        private readonly ILeadRepository _leadRepository;
        private readonly ILogger<LeadEstatisticasService> _logger;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public LeadEstatisticasService(
            ILeadRepository leadRepository,
            ILogger<LeadEstatisticasService> logger)
        {
            _leadRepository = leadRepository ?? throw new ArgumentNullException(nameof(leadRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Conta o total de leads recebidos por um vendedor em um período
        /// </summary>
        public async Task<int> ContarLeadsRecebidosAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            try
            {
                _logger.LogDebug("Contando leads recebidos para vendedor {VendedorId}, empresa {EmpresaId}, período {Periodo} dias", 
                    vendedorId, empresaId, periodoEmDias);

                var total = await _leadRepository.ContarLeadsRecebidosPorVendedorAsync(vendedorId, empresaId, periodoEmDias);
                
                _logger.LogDebug("Total de leads recebidos: {Total} para vendedor {VendedorId}", total, vendedorId);
                
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar leads recebidos do vendedor {VendedorId}", vendedorId);
                throw;
            }
        }

        /// <summary>
        /// Conta o total de leads convertidos por um vendedor em um período
        /// </summary>
        public async Task<int> ContarLeadsConvertidosAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            try
            {
                _logger.LogDebug("Contando leads convertidos para vendedor {VendedorId}, empresa {EmpresaId}, período {Periodo} dias", 
                    vendedorId, empresaId, periodoEmDias);

                var total = await _leadRepository.ContarLeadsConvertidosPorVendedorAsync(vendedorId, empresaId, periodoEmDias);
                
                _logger.LogDebug("Total de leads convertidos: {Total} para vendedor {VendedorId}", total, vendedorId);
                
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar leads convertidos do vendedor {VendedorId}", vendedorId);
                throw;
            }
        }

        /// <summary>
        /// Conta o total de leads perdidos por inatividade por um vendedor em um período
        /// </summary>
        public async Task<int> ContarLeadsPerdidosPorInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            try
            {
                _logger.LogDebug("Contando leads perdidos por inatividade para vendedor {VendedorId}, empresa {EmpresaId}, período {Periodo} dias", 
                    vendedorId, empresaId, periodoEmDias);

                var total = await _leadRepository.ContarLeadsPerdidosPorInatividadeAsync(vendedorId, empresaId, periodoEmDias);
                
                _logger.LogDebug("Total de leads perdidos por inatividade: {Total} para vendedor {VendedorId}", total, vendedorId);
                
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar leads perdidos por inatividade do vendedor {VendedorId}", vendedorId);
                throw;
            }
        }

        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor em um período
        /// </summary>
        public async Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias)
        {
            ValidarParametros(vendedorId, empresaId, periodoEmDias);

            try
            {
                _logger.LogDebug("Calculando velocidade média de atendimento para vendedor {VendedorId}, empresa {EmpresaId}, período {Periodo} dias", 
                    vendedorId, empresaId, periodoEmDias);

                var velocidadeMedia = await _leadRepository.CalcularVelocidadeMediaAtendimentoAsync(vendedorId, empresaId, periodoEmDias);
                
                _logger.LogDebug("Velocidade média de atendimento: {Velocidade} minutos para vendedor {VendedorId}", velocidadeMedia, vendedorId);
                
                return velocidadeMedia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular velocidade média de atendimento do vendedor {VendedorId}", vendedorId);
                throw;
            }
        }

        /// <summary>
        /// Obtém um lead por ID
        /// </summary>
        public async Task<WebsupplyConnect.Domain.Entities.Lead.Lead?> ObterLeadPorIdAsync(int leadId, bool includeRelated = false)
        {
            if (leadId <= 0)
                throw new ArgumentException("ID do lead deve ser maior que zero", nameof(leadId));

            try
            {
                _logger.LogDebug("Obtendo lead {LeadId} com relacionados: {IncludeRelated}", leadId, includeRelated);
                
                var lead = await _leadRepository.GetByIdAsync<WebsupplyConnect.Domain.Entities.Lead.Lead>(leadId, includeRelated);
                
                if (lead == null)
                {
                    _logger.LogDebug("Lead {LeadId} não encontrado", leadId);
                }
                else
                {
                    _logger.LogDebug("Lead {LeadId} obtido com sucesso", leadId);
                }
                
                return lead;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lead {LeadId}", leadId);
                throw;
            }
        }

        /// <summary>
        /// Valida os parâmetros de entrada dos métodos
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
    }
}
