using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.ETL;

namespace WebsupplyConnect.ETLProcessamento;

public class ETLProcessamentoFunction(
    ILoggerFactory loggerFactory,
    IETLProcessamentoService etlProcessamentoService,
    IOptions<ETLConfig> etlConfig)
{
    private const int LimiteMaximoDiasReprocessamentoPadrao = 5000;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger _logger = loggerFactory.CreateLogger<ETLProcessamentoFunction>();
    private readonly IETLProcessamentoService _etlProcessamentoService = etlProcessamentoService;
    private readonly ETLConfig _config = etlConfig.Value;

    /// <summary>
    /// Execução diária completa às 05:00 BRT (08:00 UTC).
    /// Reprocessa os últimos N dias (configurável via ETL:ExecucaoDiariaDias, padrão 90).
    /// </summary>
    [Function("ETLProcessamentoDiario")]
    public async Task RunDiario([TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo)
    {
        if (!await _semaphore.WaitAsync(TimeSpan.Zero))
        {
            _logger.LogWarning("[ETL DIÁRIO] Ignorado — outro processamento ETL já está em execução.");
            return;
        }

        try
        {
            var agora = TimeHelper.GetBrasiliaTime();
            var dataInicio = agora.AddDays(-_config.ExecucaoDiariaDias).Date;
            var dataFim = agora;

            LogDisparoInicio("DIÁRIO",
                $"janela-fixa (últimos {_config.ExecucaoDiariaDias} dias) | {dataInicio:dd/MM/yyyy} → {dataFim:dd/MM/yyyy HH:mm} BRT");

            var resultado = await _etlProcessamentoService.ProcessarAsync(dataInicio, dataFim);
            LogResumo("DIÁRIO", resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ETL DIÁRIO] Erro ao executar");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Execução incremental (overlap) a cada 30 minutos.
    /// Processa dados desde a última execução com janela de segurança.
    /// </summary>
    [Function("ETLProcessamento")]
    public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo timerInfo)
    {
        if (!await _semaphore.WaitAsync(TimeSpan.Zero))
        {
            _logger.LogWarning("[ETL INCREMENTAL] Ignorado — outro processamento ETL já está em execução.");
            return;
        }

        try
        {
            LogDisparoInicio("INCREMENTAL", "período calculado no serviço (overlap / última data processada)");

            var resultado = await _etlProcessamentoService.ProcessarAsync();
            LogResumo("INCREMENTAL", resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ETL INCREMENTAL] Erro ao executar");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void LogDisparoInicio(string gatilho, string detalhe)
    {
        var utc = DateTime.UtcNow;
        var brt = TimeHelper.GetBrasiliaTime();
        _logger.LogInformation(
            "[ETL] >>> INÍCIO gatilho={Gatilho} | UTC={Utc:o} | Brasília={Brt} | {Detalhe}",
            gatilho,
            utc,
            brt.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            detalhe);
    }

    private void LogResumo(string tipo, ETLResultado resultado)
    {
        _logger.LogInformation(
            "[ETL] <<< RESUMO gatilho={tipo} | Período {DataInicio:dd/MM/yyyy HH:mm} → {DataFim:dd/MM/yyyy HH:mm} (Brasília) | " +
            "Duração(ms)={Duracao} | Oportunidades={Oportunidades} | Leads={Leads} | Eventos={Eventos} | Total={Total}",
            tipo,
            resultado.DataInicio,
            resultado.DataFim,
            resultado.ElapsedMs,
            resultado.Oportunidades,
            resultado.Leads,
            resultado.Eventos,
            resultado.Total);
    }

    /// <summary>
    /// HTTP trigger para reprocessamento completo do ETL com período customizado.
    /// POST /api/ETLReprocessar?dataInicio=2024-01-01&dataFim=2025-02-07
    /// </summary>
    [Function("ETLReprocessar")]
    public async Task<HttpResponseData> Reprocessar(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ETLReprocessar")] HttpRequestData req)
    {
        var query = QueryHelpers.ParseQuery(req.Url.Query);
        var dataInicioStr = query["dataInicio"].FirstOrDefault();
        var dataFimStr = query["dataFim"].FirstOrDefault();

        if (string.IsNullOrEmpty(dataInicioStr) || string.IsNullOrEmpty(dataFimStr))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { erro = "Parâmetros obrigatórios: dataInicio e dataFim (formato: yyyy-MM-dd)" });
            return badRequest;
        }

        if (!DateTime.TryParse(dataInicioStr, out var dataInicio) || !DateTime.TryParse(dataFimStr, out var dataFim))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { erro = "Datas inválidas. Use formato yyyy-MM-dd" });
            return badRequest;
        }

        if (dataFim <= dataInicio)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { erro = "dataFim deve ser posterior a dataInicio" });
            return badRequest;
        }

        var limiteDias = ObterLimiteDiasReprocessamento();
        if ((dataFim - dataInicio).TotalDays > limiteDias)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { erro = $"Período máximo: {limiteDias} dias. Use um intervalo menor." });
            return badRequest;
        }

        if (!await _semaphore.WaitAsync(TimeSpan.Zero))
        {
            _logger.LogWarning("[ETL REPROCESSAR] Ignorado — outro processamento ETL já está em execução.");
            var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
            await conflictResponse.WriteAsJsonAsync(new { erro = "Outro processamento ETL já está em execução. Tente novamente mais tarde." });
            return conflictResponse;
        }

        try
        {
            LogDisparoInicio("REPROCESSAR_HTTP",
                $"janela-fixa (HTTP) | {dataInicio:dd/MM/yyyy} → {dataFim:dd/MM/yyyy}");

            var resultado = await _etlProcessamentoService.ReprocessarCompletoAsync(dataInicio, dataFim);
            LogResumo("REPROCESSAR", resultado);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                mensagem = "ETL reprocessado com sucesso.",
                periodo = new { dataInicio = resultado.DataInicio, dataFim = resultado.DataFim },
                registros = new
                {
                    oportunidades = resultado.Oportunidades,
                    leads = resultado.Leads,
                    eventos = resultado.Eventos,
                    total = resultado.Total
                },
                duracaoMs = resultado.ElapsedMs
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ETL REPROCESSAR] Erro ao executar");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { erro = ex.Message });
            return response;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private int ObterLimiteDiasReprocessamento()
        => _config.ReprocessamentoMaximoDias > 0
            ? _config.ReprocessamentoMaximoDias
            : LimiteMaximoDiasReprocessamentoPadrao;
}
