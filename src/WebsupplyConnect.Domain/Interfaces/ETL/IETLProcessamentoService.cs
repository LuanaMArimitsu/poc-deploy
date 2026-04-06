namespace WebsupplyConnect.Domain.Interfaces.ETL;

public record ETLResultado(
    DateTime DataInicio,
    DateTime DataFim,
    int Oportunidades,
    int Leads,
    int Eventos,
    long ElapsedMs)
{
    public int Total => Oportunidades + Leads + Eventos;
}

public interface IETLProcessamentoService
{
    /// <summary>
    /// Processamento delta principal
    /// </summary>
    Task<ETLResultado> ProcessarAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reprocessamento completo de período
    /// </summary>
    Task<ETLResultado> ReprocessarCompletoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default);
}
