using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    public class HistoricoDistribuicaoReaderService (ILogger<HistoricoDistribuicaoReaderService> logger, IDistribuicaoRepository distribuicaoRepository) : IHistoricoDistribuicaoReaderService
    {
        private readonly ILogger<HistoricoDistribuicaoReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IDistribuicaoRepository _distribuicaoRepository = distribuicaoRepository ?? throw new ArgumentNullException(nameof(distribuicaoRepository));


        public async Task<HistoricoDistribuicao?> GetUltimaDistribuicaoAsync(int empresaId)
        {
            try
            {
                return await _distribuicaoRepository.GetUltimaDistribuicaoAsync(empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter última distribuição. Empresa: {EmpresaId}", empresaId);
                throw new ApplicationException($"Erro ao obter última distribuição: {ex.Message}", ex);
            }
        }

        public async Task<List<HistoricoDistribuicao>> ListHistoricoDistribuicaoAsync(
        int empresaId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int pagina = 1,
        int tamanhoPagina = 20)
        {
            try
            {
                return await _distribuicaoRepository.ListHistoricoDistribuicaoAsync(empresaId, dataInicio, dataFim, pagina, tamanhoPagina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar histórico de distribuição. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}, Página: {Pagina}",
                    empresaId, dataInicio, dataFim, pagina);
                throw new ApplicationException($"Erro ao listar histórico de distribuição: {ex.Message}", ex);
            }
        }

        public Task<int> CountHistoricoDistribuicaoAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            try
            {
                return _distribuicaoRepository.CountHistoricoDistribuicaoAsync(empresaId, dataInicio, dataFim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar histórico de distribuição. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}",
                    empresaId, dataInicio, dataFim);
                throw new ApplicationException($"Erro ao contar histórico de distribuição: {ex.Message}", ex);
            }
        }

        public async Task<HistoricoDistribuicao?> GetHistoricoByIdAsync(int id)
        {
            try
            {
                return await _distribuicaoRepository.GetHistoricoByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter histórico de distribuição por ID: {Id}", id);
                throw new ApplicationException($"Erro ao obter histórico de distribuição: {ex.Message}", ex);
            }
        }

    }
}
