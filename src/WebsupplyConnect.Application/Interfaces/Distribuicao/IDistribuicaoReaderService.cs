namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    public interface IDistribuicaoReaderService
    {
        Task<decimal> GetTempoMedioDistribuicaoAsync(
                    int empresaId,
                    DateTime? dataInicio = null,
                    DateTime? dataFim = null);

    }
}
