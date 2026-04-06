namespace WebsupplyConnect.Application.Configuration;

public class ETLConfig
{
    public int UpdateIntervalMinutes { get; set; } = 30;
    public string CronExpression { get; set; } = "0 */30 * * * *";
    public int DiasEstagnada { get; set; } = 30;
    public int PrimeiraExecucaoDias { get; set; } = 365;
    /// <summary>Gravação intermediária no ETL de fatos (flush do change tracker a cada N registros processados).</summary>
    public int TamanhoBatch { get; set; } = 1000;

    /// <summary>Considera bloqueio ETL expirado após N minutos (permite retomar após falha sem intervenção).</summary>
    public int ExecucaoBloqueioMaximoMinutos { get; set; } = 120;
    public int JanelaSegurancaHoras { get; set; } = 1;

    /// <summary>
    /// Quantidade de dias retroativos processados na execução diária completa (padrão: 90 dias).
    /// </summary>
    public int ExecucaoDiariaDias { get; set; } = 90;

    /// <summary>
    /// Meta percentual de conversão de leads (ex: 0.10 = 10%). Usada no dashboard para calcular a meta diária de conversões.
    /// </summary>
    public decimal MetaConversaoPercentual { get; set; } = 0.10m;

    /// <summary>
    /// Limite máximo de dias aceito por execução de reprocessamento manual do ETL (API e Azure Function HTTP).
    /// </summary>
    public int ReprocessamentoMaximoDias { get; set; } = 5000;
}
