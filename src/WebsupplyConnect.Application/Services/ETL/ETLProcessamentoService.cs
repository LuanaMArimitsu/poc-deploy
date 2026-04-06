using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Domain.Entities.OLAP.Controle;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.ETL;
using WebsupplyConnect.Domain.Interfaces.OLAP.Controle;

namespace WebsupplyConnect.Application.Services.ETL;

public class ETLProcessamentoService : IETLProcessamentoService
{
    private readonly IETLDimensoesService _dimensoesService;
    private readonly IETLFatosService _fatosService;
    private readonly IETLControleProcessamentoRepository _controleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ETLProcessamentoService> _logger;
    private readonly ETLConfig _config;

    public ETLProcessamentoService(
        IETLDimensoesService dimensoesService,
        IETLFatosService fatosService,
        IETLControleProcessamentoRepository controleRepository,
        IUnitOfWork unitOfWork,
        ILogger<ETLProcessamentoService> logger,
        IOptions<ETLConfig> config)
    {
        _dimensoesService = dimensoesService;
        _fatosService = fatosService;
        _controleRepository = controleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<ETLResultado> ProcessarAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Iniciando processamento ETL incremental");

        await _unitOfWork.BeginTransactionAsync();

        var (ok, controle) = await _controleRepository.GarantirControleEAdquirirExecucaoAsync(
            "Fatos",
            TimeSpan.FromMinutes(_config.ExecucaoBloqueioMaximoMinutos),
            cancellationToken);

        if (!ok)
        {
            await _unitOfWork.RollbackAsync();
            throw new InvalidOperationException(
                "Já existe processamento ETL em execução. Aguarde a conclusão ou o tempo de bloqueio configurado (ExecucaoBloqueioMaximoMinutos).");
        }

        var ultimaData = controle.UltimaDataProcessada;
        var agora = TimeHelper.GetBrasiliaTime();

        var (inicio, fim) = dataInicio.HasValue && dataFim.HasValue
            ? (dataInicio.Value, dataFim.Value)
            : ObterJanelaProcessamento(ultimaData, agora);

        var modo = dataInicio.HasValue && dataFim.HasValue ? "janela-fixa" : "incremental";
        _logger.LogInformation(
            "[ETL] INÍCIO atualização | Modo={Modo} | Período {Inicio:dd/MM/yyyy HH:mm} → {Fim:dd/MM/yyyy HH:mm} (Brasília) | DisparoUtc={DisparoUtc:o}",
            modo, inicio, fim, DateTime.UtcNow);

        _logger.LogDebug("Período a processar: {DataInicio} a {DataFim}", inicio, fim);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var fontes = await _fatosService.PrepararFontesEtlAsync(inicio, fim, cancellationToken);
            _logger.LogDebug("Datas de referência coletadas das fontes: {Count} horas únicas", fontes.DatasReferencia.Count);

            if (fontes.DatasReferencia.Count > 0)
                await _dimensoesService.SincronizarDimensaoTempoAsync(fontes.DatasReferencia, cancellationToken);
            else
            {
                _logger.LogDebug("Nenhum dado transacional no período. Usando janela completa para dimensão tempo.");
                await _dimensoesService.SincronizarDimensaoTempoAsync(inicio, fim, cancellationToken);
            }

            await _dimensoesService.SincronizarDimensaoEmpresaAsync(cancellationToken);
            await _dimensoesService.SincronizarDimensaoEquipeAsync(cancellationToken);
            await _dimensoesService.SincronizarDimensaoVendedorAsync(ultimaData, cancellationToken);
            await _dimensoesService.SincronizarDimensaoStatusLeadAsync(cancellationToken);
            await _dimensoesService.SincronizarDimensaoOrigemAsync(cancellationToken);
            await _dimensoesService.SincronizarDimensaoCampanhaAsync(cancellationToken);
            await _dimensoesService.SincronizarDimensaoFunilAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            await _dimensoesService.SincronizarDimensaoEtapaFunilAsync(cancellationToken);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogDebug("Dimensões persistidas. Iniciando processamento dos fatos.");

            // Ordem intencional: oportunidade (métricas por oportunidade) → lead agregado → evento agregado.
            // Alterar a ordem pode gerar inconsistências temporárias entre fatos.
            var registrosOportunidade = await _fatosService.ProcessarFatoOportunidadeAsync(
                inicio, fim, fontes.Oportunidades, cancellationToken);
            var registrosLead = await _fatosService.ProcessarFatoLeadAgregadoAsync(inicio, fim, cancellationToken);
            var registrosEvento = await _fatosService.ProcessarFatoEventoAgregadoAsync(inicio, fim, cancellationToken);
            var totalRegistrosProcessados = registrosOportunidade + registrosLead + registrosEvento;

            controle.FinalizarComSucesso(fim, totalRegistrosProcessados, (int)sw.Elapsed.TotalSeconds);
            _controleRepository.Update<ETLControleProcessamento>(controle);
            await _unitOfWork.CommitAsync();

            sw.Stop();

            _logger.LogDebug(
                "Processamento ETL concluído com sucesso em {Elapsed}ms. Registros processados: {Total} (Oportunidades: {Oportunidades}, Leads: {Leads}, Eventos: {Eventos})",
                sw.ElapsedMilliseconds, totalRegistrosProcessados, registrosOportunidade, registrosLead, registrosEvento);

            return new ETLResultado(inicio, fim, registrosOportunidade, registrosLead, registrosEvento, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Erro no processamento ETL. Transação revertida.");

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var controleErro = await _controleRepository.ObterOuCriarAsync("Fatos", cancellationToken);
                controleErro.FinalizarComErro(ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message, (int)sw.Elapsed.TotalSeconds);
                _controleRepository.Update<ETLControleProcessamento>(controleErro);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception exControle)
            {
                _logger.LogError(exControle, "Erro ao registrar falha no controle ETL");
            }

            throw;
        }
    }

    public async Task<ETLResultado> ReprocessarCompletoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Iniciando reprocessamento completo: {DataInicio} a {DataFim}", dataInicio, dataFim);
        return await ProcessarAsync(dataInicio, dataFim, cancellationToken);
    }

    private (DateTime Inicio, DateTime Fim) ObterJanelaProcessamento(DateTime ultimaData, DateTime agora)
    {
        if (ultimaData == default)
        {
            var dias = _config.PrimeiraExecucaoDias;
            return (agora.AddDays(-dias).Date, agora);
        }

        var janelaHoras = _config.JanelaSegurancaHoras;
        return (ultimaData.AddHours(-janelaHoras), agora);
    }
}
