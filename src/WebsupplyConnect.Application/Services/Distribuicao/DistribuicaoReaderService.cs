using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    public class DistribuicaoReaderService(ILogger<DistribuicaoReaderService> logger, IDistribuicaoRepository distribuicaoRepository) : IDistribuicaoReaderService
    {
        private readonly ILogger<DistribuicaoReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IDistribuicaoRepository _distribuicaoRepository = distribuicaoRepository ?? throw new ArgumentNullException(nameof(distribuicaoRepository));

        public Task<decimal> GetTempoMedioDistribuicaoAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            try
            {
                return _distribuicaoRepository.GetTempoMedioDistribuicaoAsync(empresaId, dataInicio, dataFim);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tempo médio de distribuição. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}",
                    empresaId, dataInicio, dataFim);
                throw new ApplicationException($"Erro ao obter tempo médio de distribuição: {ex.Message}", ex);
            }
        }
    }
}
