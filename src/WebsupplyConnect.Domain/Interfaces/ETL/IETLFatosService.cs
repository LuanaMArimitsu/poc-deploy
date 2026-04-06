namespace WebsupplyConnect.Domain.Interfaces.ETL;

public interface IETLFatosService
{
    /// <summary>
    /// Carrega fontes do período uma única vez (datas de referência + oportunidades) para evitar leitura duplicada.
    /// </summary>
    Task<(IReadOnlySet<DateTime> DatasReferencia, IReadOnlyList<WebsupplyConnect.Domain.Entities.Oportunidade.Oportunidade> Oportunidades)> PrepararFontesEtlAsync(
        DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);

    Task<int> ProcessarFatoOportunidadeAsync(DateTime dataInicio, DateTime dataFim,
        IReadOnlyList<WebsupplyConnect.Domain.Entities.Oportunidade.Oportunidade>? oportunidadesPreCarregadas, CancellationToken cancellationToken = default);

    Task<int> ProcessarFatoLeadAgregadoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task<int> ProcessarFatoEventoAgregadoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default);
}
