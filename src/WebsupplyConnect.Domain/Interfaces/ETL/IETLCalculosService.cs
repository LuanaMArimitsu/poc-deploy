namespace WebsupplyConnect.Domain.Interfaces.ETL;

public interface IETLCalculosService
{
    // Cálculos de Tempo de Resposta
    Task<decimal?> CalcularTempoMedioRespostaAsync(
        int leadId,
        HashSet<int> botUserIds,
        Dictionary<int, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>>? horariosVendedores = null,
        CancellationToken cancellationToken = default);
    Task<decimal?> CalcularTempoMedioPrimeiroAtendimentoAsync(int leadId, HashSet<int> botUserIds, CancellationToken cancellationToken = default);

    // Cálculos de Ciclo de Vendas
    Task<int?> CalcularDuracaoCicloVendasAsync(int oportunidadeId);
    Task<int?> CalcularTempoEmEtapaAtualAsync(int oportunidadeId);
    Task<int?> CalcularDuracaoCicloCompletoAsync(int leadId);
    Task<int?> CalcularTempoAtePrimeiraOportunidadeAsync(int leadId);
    Task<decimal?> CalcularValorEsperadoPipelineAsync(int oportunidadeId);
}
