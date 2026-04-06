using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    public interface IHistoricoDistribuicaoReaderService
    {
        Task<HistoricoDistribuicao?> GetUltimaDistribuicaoAsync(int empresaId);
        Task<List<HistoricoDistribuicao>> ListHistoricoDistribuicaoAsync(
        int empresaId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int pagina = 1,
        int tamanhoPagina = 20);

        Task<int> CountHistoricoDistribuicaoAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null);

        Task<HistoricoDistribuicao?> GetHistoricoByIdAsync(int id);
    }
}
