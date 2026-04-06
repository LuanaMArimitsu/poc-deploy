using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.ETL;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;

namespace WebsupplyConnect.Application.Services.ETL;

public class ETLFatosService : IETLFatosService
{
    private readonly IOportunidadeReaderService _oportunidadeReaderService;
    private readonly ILeadReaderService _leadReaderService;
    private readonly ILeadEventoReaderService _leadEventoReaderService;
    private readonly IConversaReaderService _conversaReaderService;
    private readonly IMensagemReaderService _mensagemReaderService;
    private readonly IFatoOportunidadeMetricaRepository _fatoOportunidadeRepository;
    private readonly IFatoLeadAgregadoRepository _fatoLeadRepository;
    private readonly IFatoEventoAgregadoRepository _fatoEventoRepository;
    private readonly IDimensaoOlapReadService _dimensoesOlap;
    private readonly IETLCalculosService _calculosService;
    private readonly IUsuarioReaderService _usuarioReaderService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ETLFatosService> _logger;
    private readonly ETLConfig _config;

    public ETLFatosService(
        IOportunidadeReaderService oportunidadeReaderService,
        ILeadReaderService leadReaderService,
        ILeadEventoReaderService leadEventoReaderService,
        IConversaReaderService conversaReaderService,
        IMensagemReaderService mensagemReaderService,
        IFatoOportunidadeMetricaRepository fatoOportunidadeRepository,
        IFatoLeadAgregadoRepository fatoLeadRepository,
        IFatoEventoAgregadoRepository fatoEventoRepository,
        IDimensaoOlapReadService dimensoesOlap,
        IETLCalculosService calculosService,
        IUsuarioReaderService usuarioReaderService,
        IUnitOfWork unitOfWork,
        ILogger<ETLFatosService> logger,
        IOptions<ETLConfig> config)
    {
        _oportunidadeReaderService = oportunidadeReaderService;
        _leadReaderService = leadReaderService;
        _leadEventoReaderService = leadEventoReaderService;
        _conversaReaderService = conversaReaderService;
        _mensagemReaderService = mensagemReaderService;
        _fatoOportunidadeRepository = fatoOportunidadeRepository;
        _fatoLeadRepository = fatoLeadRepository;
        _fatoEventoRepository = fatoEventoRepository;
        _dimensoesOlap = dimensoesOlap;
        _calculosService = calculosService;
        _usuarioReaderService = usuarioReaderService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<(IReadOnlySet<DateTime> DatasReferencia, IReadOnlyList<WebsupplyConnect.Domain.Entities.Oportunidade.Oportunidade> Oportunidades)> PrepararFontesEtlAsync(
        DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var datas = new HashSet<DateTime>();

        var oportunidades = await _oportunidadeReaderService.ObterOportunidadesPorPeriodoParaETLAsync(dataInicio, dataFim);
        foreach (var op in oportunidades)
            datas.Add(TruncarParaHora(op.DataCriacao));

        var leadsIds = await IdentificarLeadsAfetadosAsync(dataInicio, dataFim);
        var leads = await _leadReaderService.ObterLeadsPorIdsAsync(leadsIds, includeDeleted: true);
        foreach (var lead in leads)
            datas.Add(TruncarParaHora(lead.DataCriacao));

        var eventos = await _leadEventoReaderService.ObterEventosPorPeriodoParaETLAsync(dataInicio, dataFim);
        foreach (var ev in eventos)
            datas.Add(TruncarParaHora(ev.DataEvento));

        return (datas, oportunidades);
    }

    public async Task<int> ProcessarFatoOportunidadeAsync(DateTime dataInicio, DateTime dataFim,
        IReadOnlyList<WebsupplyConnect.Domain.Entities.Oportunidade.Oportunidade>? oportunidadesPreCarregadas, CancellationToken cancellationToken = default)
    {
        var oportunidades = oportunidadesPreCarregadas ??
                            await _oportunidadeReaderService.ObterOportunidadesPorPeriodoParaETLAsync(dataInicio, dataFim);
        var botUserIds = await _usuarioReaderService.ObterBotUserIdsAsync();
        var horariosVendedores = await CarregarHorariosVendedoresAsync();

        _logger.LogDebug("Processando FatoOportunidadeMetrica: {Count} oportunidades no período", oportunidades.Count);
        var skipOportunidade = 0;
        var criadosOportunidade = 0;
        var processados = 0;
        var lote = 0;
        var batchSize = Math.Max(1, _config.TamanhoBatch);

        for (var offset = 0; offset < oportunidades.Count; offset += batchSize)
        {
            var chunk = oportunidades.Skip(offset).Take(batchSize).ToList();
            lote++;
            var leadIdsChunk = chunk.Select(o => o.LeadId).Distinct().ToList();
            var leadsLista = await _leadReaderService.ObterLeadsComResponsavelUsuarioPorIdsAsync(leadIdsChunk, includeDeleted: true);
            var leadsDict = leadsLista.ToDictionary(l => l.Id);

            var eventosPorLead = await _leadEventoReaderService.ObterEventosAgrupadosPorLeadIdsParaETLAsync(leadIdsChunk);

            var datasRef = chunk.Select(o => TruncarParaHora(o.DataCriacao)).Distinct().ToList();
            var empresaIds = chunk.Select(o => o.EmpresaId).Distinct().ToList();
            var origemIds = chunk.Select(o => o.OrigemId).Distinct().ToList();
            var respIds = chunk.Select(o => o.ResponsavelId).Distinct().ToList();
            var etapaIds = chunk.Select(o => o.EtapaId).Distinct().ToList();

            var statusIds = new List<int>();
            var equipeOrigemIds = new List<int>();
            foreach (var lid in leadIdsChunk)
            {
                if (!leadsDict.TryGetValue(lid, out var ld)) continue;
                statusIds.Add(ld.LeadStatusId);
                if (ld.EquipeId.HasValue)
                    equipeOrigemIds.Add(ld.EquipeId.Value);
            }

            var campanhaOrigemIds = new List<int>();
            foreach (var lid in leadIdsChunk)
            {
                if (!eventosPorLead.TryGetValue(lid, out var evs)) continue;
                var c = evs.Where(e => e.CampanhaId.HasValue).OrderByDescending(e => e.DataEvento).FirstOrDefault()?.CampanhaId;
                if (c.HasValue)
                    campanhaOrigemIds.Add(c.Value);
            }

            var mapTempo = await _dimensoesOlap.ObterDimensoesTempoPorDatasAsync(datasRef, cancellationToken);
            var mapEmpresa = await _dimensoesOlap.ObterDimensoesEmpresaPorOrigemIdsAsync(empresaIds, cancellationToken);
            var mapOrigem = await _dimensoesOlap.ObterDimensoesOrigemPorOrigemIdsIncluindoExcluidasAsync(origemIds, cancellationToken);
            var mapVendedor = await _dimensoesOlap.ObterDimensoesVendedorPorOrigemIdsAsync(respIds, cancellationToken);
            var mapEtapa = await _dimensoesOlap.ObterDimensoesEtapaFunilPorOrigemIdsAsync(etapaIds, cancellationToken);
            var mapStatus = await _dimensoesOlap.ObterDimensoesStatusLeadPorOrigemIdsAsync(statusIds.Distinct().ToList(), cancellationToken);
            var mapEquipe = await _dimensoesOlap.ObterDimensoesEquipePorOrigemIdsAsync(equipeOrigemIds.Distinct().ToList(), cancellationToken);
            var mapCampanha = campanhaOrigemIds.Count > 0
                ? await _dimensoesOlap.ObterDimensoesCampanhaPorOrigemIdsAsync(campanhaOrigemIds.Distinct().ToList(), cancellationToken)
                : new Dictionary<int, DimensaoCampanha>();

            var chavesFato = chunk
                .Select(o => (o.Id, TruncarParaHora(o.DataCriacao)))
                .ToList();
            var fatosExistentes = await _fatoOportunidadeRepository.ObterPorChavesOportunidadeDataReferenciaAsync(chavesFato, cancellationToken);

            foreach (var op in chunk)
            {
                try
                {
                    var dataRef = TruncarParaHora(op.DataCriacao);
                    fatosExistentes.TryGetValue((op.Id, dataRef), out var fatoExistente);
                    if (!leadsDict.TryGetValue(op.LeadId, out var lead))
                        continue;

                    if (!eventosPorLead.TryGetValue(op.LeadId, out var eventosDoLead))
                        eventosDoLead = [];

                    var dataUltimoEventoLead = eventosDoLead.OrderByDescending(e => e.DataEvento).FirstOrDefault()?.DataEvento;

                    mapStatus.TryGetValue(lead.LeadStatusId, out var statusLeadDim);
                    var statusLeadId = statusLeadDim?.Id;

                    mapTempo.TryGetValue(dataRef, out var tempo);
                    mapEmpresa.TryGetValue(op.EmpresaId, out var empresa);
                    mapOrigem.TryGetValue(op.OrigemId, out var origem);
                    if (tempo == null || empresa == null || origem == null)
                    {
                        skipOportunidade++;
                        if (skipOportunidade <= 5)
                            _logger.LogDebug("Oportunidade {Id} ignorada: Tempo={HasTempo}, Empresa={HasEmpresa}, Origem={HasOrigem}", op.Id, tempo != null, empresa != null, origem != null);
                        continue;
                    }

                    mapVendedor.TryGetValue(op.ResponsavelId, out var vendedorDim);
                    DimensaoEquipe? equipeDim = null;
                    if (lead.EquipeId.HasValue)
                        mapEquipe.TryGetValue(lead.EquipeId.Value, out equipeDim);
                    var equipeId = equipeDim?.Id;
                    var vendedorId = vendedorDim?.Id;

                    var ehGanha = op.Convertida == true;
                    var ehPerdida = op.Convertida == false;

                    var duracaoCiclo = await _calculosService.CalcularDuracaoCicloVendasAsync(op.Id);
                    var tempoEmEtapa = await _calculosService.CalcularTempoEmEtapaAtualAsync(op.Id);
                    var valorEsperado = await _calculosService.CalcularValorEsperadoPipelineAsync(op.Id);
                    var agora = TimeHelper.GetBrasiliaTime();
                    var diasDesdeInteracao = op.DataUltimaInteracao.HasValue ? (int?)(agora - op.DataUltimaInteracao.Value).TotalDays : null;
                    var ehEstagnada = op.Convertida == null && diasDesdeInteracao >= _config.DiasEstagnada;

                    var tempoMedioResposta = await _calculosService.CalcularTempoMedioRespostaAsync(op.LeadId, botUserIds, horariosVendedores, cancellationToken);
                    var tempoMedioPrimeiroAtend = await _calculosService.CalcularTempoMedioPrimeiroAtendimentoAsync(op.LeadId, botUserIds, cancellationToken);

                    DimensaoCampanha? campanhaDim = null;
                    var campanhaOrigemId = eventosDoLead.Where(e => e.CampanhaId.HasValue).OrderByDescending(e => e.DataEvento).FirstOrDefault()?.CampanhaId;
                    if (campanhaOrigemId.HasValue)
                        mapCampanha.TryGetValue(campanhaOrigemId.Value, out campanhaDim);

                    mapEtapa.TryGetValue(op.EtapaId, out var etapaDim);
                    var dimensaoEtapaFunilId = etapaDim?.Id;

                    if (fatoExistente != null)
                    {
                        fatoExistente.AtualizarDimensoes(equipeId, vendedorId, campanhaDim?.Id, dimensaoEtapaFunilId);
                        fatoExistente.AtualizarMetricas(op.Valor ?? 0, op.ValorFinal, op.Probabilidade, ehGanha, ehPerdida, op.DataFechamento);
                        fatoExistente.AtualizarMetricasCicloVendas(duracaoCiclo, tempoEmEtapa, diasDesdeInteracao, ehEstagnada, valorEsperado);
                        fatoExistente.AtualizarMetricasLead(tempoMedioResposta, tempoMedioPrimeiroAtend, 0, 0);
                        fatoExistente.AtualizarDataUltimoEvento(dataUltimoEventoLead);
                        await _fatoOportunidadeRepository.UpsertAsync(fatoExistente, cancellationToken);
                    }
                    else
                    {
                        var novoFato = new FatoOportunidadeMetrica(
                            op.Id, op.LeadId, op.LeadEventoId,
                            tempo.Id, empresa.Id,
                            equipeId, vendedorId, statusLeadId,
                            origem.Id, campanhaDim?.Id,
                            dimensaoEtapaFunilId,
                            op.Valor ?? 0, op.ValorFinal, op.Probabilidade,
                            ehGanha, ehPerdida, op.DataFechamento, dataRef);
                        novoFato.AtualizarMetricasCicloVendas(duracaoCiclo, tempoEmEtapa, diasDesdeInteracao, ehEstagnada, valorEsperado);
                        novoFato.AtualizarMetricasLead(tempoMedioResposta, tempoMedioPrimeiroAtend, 0, 0);
                        novoFato.AtualizarDataUltimoEvento(dataUltimoEventoLead);
                        await _fatoOportunidadeRepository.CreateAsync(novoFato);
                        criadosOportunidade++;
                    }

                    processados++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar OportunidadeId={Id}", op.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogDebug("FatoOportunidadeMetrica: lote {Lote} concluído (batch {Batch})", lote, batchSize);
        }

        _logger.LogDebug("FatoOportunidadeMetrica: {Criados} novos fatos, {Skip} ignorados (dimensão ausente), {Processados} processados", criadosOportunidade, skipOportunidade, processados);

        return processados;
    }

    public async Task<int> ProcessarFatoLeadAgregadoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        var leadsIds = await IdentificarLeadsAfetadosAsync(dataInicio, dataFim);
        _logger.LogDebug("Processando FatoLeadAgregado: {Count} leads no período", leadsIds.Count);
        var botUserIds = await _usuarioReaderService.ObterBotUserIdsAsync();
        var horariosVendedores = await CarregarHorariosVendedoresAsync();
        var processados = 0;
        var duplicatasLimpas = 0;
        foreach (var leadId in leadsIds)
        {
            try
            {
                var lead = await _leadReaderService.ObterLeadComResponsavelParaETLAsync(leadId, includeDeleted: true);
                if (lead == null) continue;

                if (_leadReaderService.LeadPertenceAoBot(lead))
                {
                    await _fatoLeadRepository.ExcluirTodosPorLeadIdAsync(leadId, cancellationToken);
                    continue;
                }

                await _fatoLeadRepository.LimparDuplicatasAsync(leadId, cancellationToken);

                var dataRef = TruncarParaHora(lead.DataCriacao);
                var fatoExistente = await _fatoLeadRepository.ObterPorLeadIdAsync(leadId, cancellationToken);

                var tempo = await _dimensoesOlap.ObterDimensaoTempoPorDataAsync(dataRef, cancellationToken);
                var empresa = await _dimensoesOlap.ObterDimensaoEmpresaPorOrigemIdAsync(lead.EmpresaId, cancellationToken);
                var origem = await _dimensoesOlap.ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(lead.OrigemId, cancellationToken);
                if (tempo == null || empresa == null || origem == null) continue;

                var statusDim = await _dimensoesOlap.ObterDimensaoStatusLeadPorOrigemIdAsync(lead.LeadStatusId, cancellationToken);
                var statusAtualId = statusDim?.Id;

                var equipeDim = lead.EquipeId.HasValue ? await _dimensoesOlap.ObterDimensaoEquipePorOrigemIdAsync(lead.EquipeId.Value, cancellationToken) : null;

                var oportunidades = await _oportunidadeReaderService.ObterOportunidadesPorLeadIdParaETLAsync(leadId);
                int? usuarioOrigemId = null;
                if (oportunidades.Count > 0)
                {
                    var opMaisRecente = oportunidades.OrderByDescending(o => o.DataModificacao).First();
                    usuarioOrigemId = opMaisRecente.ResponsavelId;
                }
                else if (lead.Responsavel != null)
                {
                    usuarioOrigemId = lead.Responsavel.UsuarioId;
                }
                var vendedorDim = usuarioOrigemId.HasValue ? await _dimensoesOlap.ObterDimensaoVendedorPorOrigemIdAsync(usuarioOrigemId.Value, cancellationToken) : null;
                var eventos = await _leadEventoReaderService.ObterEventosPorLeadIdParaETLAsync(leadId);
                var campanhaOrigemId = eventos.Where(e => e.CampanhaId.HasValue).OrderByDescending(e => e.DataEvento).FirstOrDefault()?.CampanhaId;
                var campanhaDim = campanhaOrigemId.HasValue ? await _dimensoesOlap.ObterDimensaoCampanhaPorOrigemIdAsync(campanhaOrigemId.Value, cancellationToken) : null;
                var totalOportunidades = oportunidades.Count;
                var oportunidadesGanhas = oportunidades.Count(o => o.Convertida == true);
                var oportunidadesPerdidas = oportunidades.Count(o => o.Convertida == false);
                var valorTotalGanhas = oportunidades.Where(o => o.Convertida == true).Sum(o => o.ValorFinal ?? o.Valor ?? 0);

                var tempoMedioResposta = await _calculosService.CalcularTempoMedioRespostaAsync(leadId, botUserIds, horariosVendedores);
                var tempoMedioPrimeiroAtend = await _calculosService.CalcularTempoMedioPrimeiroAtendimentoAsync(leadId, botUserIds);
                var duracaoCiclo = await _calculosService.CalcularDuracaoCicloCompletoAsync(leadId);
                var tempoAtePrimeiraOp = await _calculosService.CalcularTempoAtePrimeiraOportunidadeAsync(leadId);

                const int statusMensagemNaoLida = 25;
                const int statusConversaAtiva = 28;
                const int statusConversaAguardandoResposta = 29;

                var conversasDoLead = await _conversaReaderService.ObterConversasPorLeadIdParaETLAsync(leadId);
                var totalConversas = conversasDoLead.Count;
                var totalMensagens = 0;
                var mensagensNaoLidas = 0;
                var teveInteracaoVendedorHumano = false;
                var possuiConversaAssociada = conversasDoLead.Any(c => !c.Excluido);
                var possuiConversaAtiva = conversasDoLead.Any(c => !c.Excluido &&
                    (c.StatusId == statusConversaAtiva || c.StatusId == statusConversaAguardandoResposta));
                var teveInteracaoVendedorHumanoConversaAtiva = false;
                var possuiConversaAtivaComPrimeiraMensagemTemplateBot = false;
                DateTime? ultimaMsgNaoBotDataAtiva = null;
                char? ultimaMsgNaoBotSentidoAtiva = null;
                foreach (var conversa in conversasDoLead)
                {
                    var mensagens = await _mensagemReaderService.ObterMensagensPorConversaIdParaETLAsync(conversa.Id);
                    totalMensagens += mensagens.Count;
                    mensagensNaoLidas += mensagens.Count(m => m.StatusId == statusMensagemNaoLida && m.Sentido == 'R');

                    var conversaAtiva = !conversa.Excluido &&
                        (conversa.StatusId == statusConversaAtiva || conversa.StatusId == statusConversaAguardandoResposta);

                    if (conversaAtiva)
                    {
                        var mensagensOrdenadas = mensagens
                            .OrderBy(m => m.DataEnvio ?? m.DataCriacao)
                            .ThenBy(m => m.Id)
                            .ToList();

                        var primeiraMensagem = mensagensOrdenadas.FirstOrDefault();
                        if (primeiraMensagem != null)
                        {
                            var primeiraMensagemEhTemplateBot = primeiraMensagem.TemplateId.HasValue
                                && primeiraMensagem.Sentido == 'E'
                                && primeiraMensagem.UsuarioId.HasValue
                                && botUserIds.Contains(primeiraMensagem.UsuarioId.Value);
                            if (primeiraMensagemEhTemplateBot)
                                possuiConversaAtivaComPrimeiraMensagemTemplateBot = true;
                        }
                    }

                    foreach (var msg in mensagens)
                    {
                        var ehMsgBot = msg.Sentido == 'E' && msg.UsuarioId.HasValue && botUserIds.Contains(msg.UsuarioId.Value);
                        if (ehMsgBot) continue;

                        if (msg.Sentido == 'E' && msg.UsuarioId.HasValue)
                        {
                            teveInteracaoVendedorHumano = true;
                            if (conversaAtiva)
                                teveInteracaoVendedorHumanoConversaAtiva = true;
                        }

                        if (conversaAtiva)
                        {
                            var dataMsg = msg.DataEnvio ?? msg.DataCriacao;
                            if (ultimaMsgNaoBotDataAtiva == null || dataMsg > ultimaMsgNaoBotDataAtiva)
                            {
                                ultimaMsgNaoBotDataAtiva = dataMsg;
                                ultimaMsgNaoBotSentidoAtiva = msg.Sentido;
                            }
                        }
                    }
                }
                var responsavelEhBot = lead.Responsavel != null && botUserIds.Contains(lead.Responsavel.UsuarioId);
                var ehInativo = statusDim?.Codigo == "INATIVO";
                var aguardandoRespostaVendedor = CalcularAguardandoRespostaVendedor(
                    ehInativo,
                    responsavelEhBot,
                    possuiConversaAssociada,
                    possuiConversaAtiva,
                    teveInteracaoVendedorHumanoConversaAtiva,
                    possuiConversaAtivaComPrimeiraMensagemTemplateBot);
                var aguardandoRespostaAtendimento = !ehInativo && teveInteracaoVendedorHumano
                    && possuiConversaAtiva
                    && ultimaMsgNaoBotSentidoAtiva == 'R';

                string? produtoInteresse = null;
                if (oportunidades.Count > 0)
                {
                    var opMaisRecenteProduto = oportunidades.OrderByDescending(o => o.DataModificacao).First();
                    produtoInteresse = await _oportunidadeReaderService.ObterNomeProdutoOportunidadeParaETLAsync(opMaisRecenteProduto.Id);
                }

                var equipeId = equipeDim?.Id;
                var vendedorId = vendedorDim?.Id;
                var campanhaId = campanhaDim?.Id;

                var dataUltimoEvento = eventos.OrderByDescending(e => e.DataEvento).FirstOrDefault()?.DataEvento
                    ?? ultimaMsgNaoBotDataAtiva
                    ?? lead.DataCriacao;

                if (fatoExistente != null)
                {
                    fatoExistente.AtualizarDimensoesCompletas(
                        tempo.Id, empresa.Id, equipeId, vendedorId,
                        statusAtualId, origem.Id, campanhaId, dataRef);
                    fatoExistente.AtualizarMetricasAgregadas(eventos.Count, totalOportunidades, oportunidadesGanhas, oportunidadesPerdidas, valorTotalGanhas);
                    fatoExistente.AtualizarMetricasConversao(lead.DataConversaoCliente.HasValue, false, lead.DataConversaoCliente);
                    fatoExistente.AtualizarMetricasCicloVendas(duracaoCiclo, tempoAtePrimeiraOp, null);
                    fatoExistente.AtualizarMetricasAtendimento(tempoMedioResposta, tempoMedioPrimeiroAtend, totalConversas, totalMensagens, mensagensNaoLidas, aguardandoRespostaVendedor, aguardandoRespostaAtendimento);
                    fatoExistente.AtualizarProdutoInteresse(produtoInteresse);
                    fatoExistente.AtualizarDataUltimoEvento(dataUltimoEvento);
                    await _fatoLeadRepository.UpsertAsync(fatoExistente, cancellationToken);
                }
                else
                {
                    var novoFato = new FatoLeadAgregado(
                        leadId, tempo.Id, empresa.Id,
                        equipeId, vendedorId, statusAtualId, origem.Id, campanhaId, dataRef);
                    novoFato.AtualizarMetricasAgregadas(eventos.Count, totalOportunidades, oportunidadesGanhas, oportunidadesPerdidas, valorTotalGanhas);
                    novoFato.AtualizarMetricasConversao(lead.DataConversaoCliente.HasValue, false, lead.DataConversaoCliente);
                    novoFato.AtualizarMetricasCicloVendas(duracaoCiclo, tempoAtePrimeiraOp, null);
                    novoFato.AtualizarMetricasAtendimento(tempoMedioResposta, tempoMedioPrimeiroAtend, totalConversas, totalMensagens, mensagensNaoLidas, aguardandoRespostaVendedor, aguardandoRespostaAtendimento);
                    novoFato.AtualizarProdutoInteresse(produtoInteresse);
                    novoFato.AtualizarDataUltimoEvento(dataUltimoEvento);
                    await _fatoLeadRepository.CreateAsync(novoFato);
                }

                processados++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar LeadId={Id}", leadId);
            }
        }

        if (duplicatasLimpas > 0)
            _logger.LogInformation("FatoLeadAgregado: {DuplicatasLimpas} registros duplicados removidos", duplicatasLimpas);

        return processados;
    }

    public async Task<int> ProcessarFatoEventoAgregadoAsync(DateTime dataInicio, DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        var eventos = await _leadEventoReaderService.ObterEventosPorPeriodoParaETLAsync(dataInicio, dataFim);
        var botUserIds = await _usuarioReaderService.ObterBotUserIdsAsync();
        var horariosVendedores = await CarregarHorariosVendedoresAsync();

        _logger.LogDebug("Processando FatoEventoAgregado: {Count} eventos no período", eventos.Count);
        var processados = 0;
        foreach (var ev in eventos)
        {
            try
            {
                var dataRef = TruncarParaHora(ev.DataEvento);
                var fatoExistente = await _fatoEventoRepository.ObterPorLeadEventoDataReferenciaAsync(ev.Id, dataRef, cancellationToken);

                var tempo = await _dimensoesOlap.ObterDimensaoTempoPorDataAsync(dataRef, cancellationToken);
                var lead = await _leadReaderService.ObterLeadComResponsavelParaETLAsync(ev.LeadId, includeDeleted: true);
                if (lead == null || tempo == null) continue;

                if (_leadReaderService.LeadPertenceAoBot(lead))
                {
                    await _fatoEventoRepository.ExcluirTodosPorLeadIdAsync(ev.LeadId, cancellationToken);
                    continue;
                }

                var statusEventoDim = await _dimensoesOlap.ObterDimensaoStatusLeadPorOrigemIdAsync(lead.LeadStatusId, cancellationToken);
                var statusAtualIdEv = statusEventoDim?.Id;

                var empresa = await _dimensoesOlap.ObterDimensaoEmpresaPorOrigemIdAsync(lead.EmpresaId, cancellationToken);
                var origem = await _dimensoesOlap.ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(ev.OrigemId, cancellationToken);
                if (empresa == null || origem == null) continue;

                var eventosDoLead = await _leadEventoReaderService.ObterEventosPorLeadIdParaETLAsync(ev.LeadId);
                var dataUltimoEventoLead = eventosDoLead.OrderByDescending(e => e.DataEvento).FirstOrDefault()?.DataEvento;

                var oportunidadesDoEvento = await _oportunidadeReaderService.ObterOportunidadesPorLeadEventoIdParaETLAsync(ev.Id);
                var totalOportunidades = oportunidadesDoEvento.Count;
                var oportunidadesGanhas = oportunidadesDoEvento.Count(o => o.Convertida == true);
                var oportunidadesPerdidas = oportunidadesDoEvento.Count(o => o.Convertida == false);
                var valorTotalGanhas = oportunidadesDoEvento.Where(o => o.Convertida == true).Sum(o => o.ValorFinal ?? o.Valor ?? 0);

                int? usuarioOrigemIdEv = null;
                if (oportunidadesDoEvento.Count > 0)
                {
                    var opEv = oportunidadesDoEvento.OrderByDescending(o => o.DataModificacao).First();
                    usuarioOrigemIdEv = opEv.ResponsavelId;
                }
                else if (lead.Responsavel != null)
                {
                    usuarioOrigemIdEv = lead.Responsavel.UsuarioId;
                }
                var vendedorDimEv = usuarioOrigemIdEv.HasValue ? await _dimensoesOlap.ObterDimensaoVendedorPorOrigemIdAsync(usuarioOrigemIdEv.Value, cancellationToken) : null;
                var equipeDimEv = lead.EquipeId.HasValue ? await _dimensoesOlap.ObterDimensaoEquipePorOrigemIdAsync(lead.EquipeId.Value, cancellationToken) : null;
                var campanhaDimEv = ev.CampanhaId.HasValue ? await _dimensoesOlap.ObterDimensaoCampanhaPorOrigemIdAsync(ev.CampanhaId.Value, cancellationToken) : null;

                var tempoMedioResposta = await _calculosService.CalcularTempoMedioRespostaAsync(ev.LeadId, botUserIds, horariosVendedores, cancellationToken);
                var tempoMedioPrimeiroAtend = await _calculosService.CalcularTempoMedioPrimeiroAtendimentoAsync(ev.LeadId, botUserIds, cancellationToken);
                var duracaoCiclo = await _calculosService.CalcularDuracaoCicloCompletoAsync(ev.LeadId);
                var tempoAtePrimeiraOp = await _calculosService.CalcularTempoAtePrimeiraOportunidadeAsync(ev.LeadId);

                const int statusMensagemNaoLida = 25;
                var conversasDoLead = await _conversaReaderService.ObterConversasPorLeadIdParaETLAsync(ev.LeadId);
                var totalConversas = conversasDoLead.Count;
                var totalMensagens = 0;
                var mensagensNaoLidas = 0;
                foreach (var conversa in conversasDoLead)
                {
                    var mensagens = await _mensagemReaderService.ObterMensagensPorConversaIdParaETLAsync(conversa.Id);
                    totalMensagens += mensagens.Count;
                    mensagensNaoLidas += mensagens.Count(m => m.StatusId == statusMensagemNaoLida && m.Sentido == 'R');
                }

                string? produtoInteresse = null;
                if (oportunidadesDoEvento.Count > 0)
                {
                    var opMaisRecente = oportunidadesDoEvento.OrderByDescending(o => o.DataModificacao).First();
                    produtoInteresse = await _oportunidadeReaderService.ObterNomeProdutoOportunidadeParaETLAsync(opMaisRecente.Id);
                }

                if (fatoExistente != null)
                {
                    fatoExistente.AtualizarDimensoes(equipeDimEv?.Id, vendedorDimEv?.Id, campanhaDimEv?.Id);
                    fatoExistente.AtualizarMetricas(totalOportunidades, oportunidadesGanhas, oportunidadesPerdidas, valorTotalGanhas);
                    fatoExistente.AtualizarMetricasConversao(lead.DataConversaoCliente.HasValue, lead.DataConversaoCliente);
                    fatoExistente.AtualizarMetricasCicloVendas(duracaoCiclo, tempoAtePrimeiraOp);
                    fatoExistente.AtualizarMetricasAtendimento(tempoMedioResposta, tempoMedioPrimeiroAtend, totalConversas, totalMensagens, mensagensNaoLidas);
                    fatoExistente.AtualizarProdutoInteresse(produtoInteresse);
                    fatoExistente.AtualizarDataUltimoEvento(dataUltimoEventoLead);
                    await _fatoEventoRepository.UpsertAsync(fatoExistente, cancellationToken);
                }
                else
                {
                    var novoFato = new FatoEventoAgregado(
                        ev.Id, ev.LeadId, tempo.Id, empresa.Id,
                        equipeDimEv?.Id, vendedorDimEv?.Id, statusAtualIdEv, origem.Id, campanhaDimEv?.Id, dataRef);
                    novoFato.AtualizarMetricas(totalOportunidades, oportunidadesGanhas, oportunidadesPerdidas, valorTotalGanhas);
                    novoFato.AtualizarMetricasConversao(lead.DataConversaoCliente.HasValue, lead.DataConversaoCliente);
                    novoFato.AtualizarMetricasCicloVendas(duracaoCiclo, tempoAtePrimeiraOp);
                    novoFato.AtualizarMetricasAtendimento(tempoMedioResposta, tempoMedioPrimeiroAtend, totalConversas, totalMensagens, mensagensNaoLidas);
                    novoFato.AtualizarProdutoInteresse(produtoInteresse);
                    novoFato.AtualizarDataUltimoEvento(dataUltimoEventoLead);
                    await _fatoEventoRepository.CreateAsync(novoFato);
                }

                processados++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar LeadEventoId={Id}", ev.Id);
            }
        }

        return processados;
    }

    private async Task<List<int>> IdentificarLeadsAfetadosAsync(DateTime dataInicio, DateTime dataFim)
    {
        var leads = await _leadReaderService.ObterLeadsPorPeriodoModificacaoParaETLAsync(dataInicio, dataFim);
        var oportunidades = await _oportunidadeReaderService.ObterOportunidadesPorPeriodoParaETLAsync(dataInicio, dataFim);
        var eventos = await _leadEventoReaderService.ObterEventosPorPeriodoParaETLAsync(dataInicio, dataFim);
        var conversas = await _conversaReaderService.ObterConversasPorPeriodoModificacaoParaETLAsync(dataInicio, dataFim);

        return leads.Select(l => l.Id)
            .Union(oportunidades.Select(o => o.LeadId))
            .Union(eventos.Select(e => e.LeadId))
            .Union(conversas.Select(c => c.LeadId))
            .Distinct()
            .ToList();
    }

    private async Task<Dictionary<int, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>>> CarregarHorariosVendedoresAsync()
    {
        var usuarios = await _usuarioReaderService.ObterUsuariosAtivosNaoBotParaETLAsync();
        var ids = usuarios.Select(u => u.Id).ToList();
        var horariosDto = await _usuarioReaderService.ObterHorariosMultiplosUsuariosAsync(ids);
        return ConverterHorariosParaSchedule(horariosDto);
    }

    internal static Dictionary<int, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>> ConverterHorariosParaSchedule(
        Dictionary<int, List<DTOs.Usuario.UsuarioHorarioDTO>> horariosDto)
    {
        var result = new Dictionary<int, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>>();

        foreach (var (usuarioId, horarios) in horariosDto)
        {
            var diasComExpediente = horarios.Where(h => !h.SemExpediente && h.HorarioInicio.HasValue && h.HorarioFim.HasValue).ToList();
            if (diasComExpediente.Count == 0)
                continue;

            var schedule = new Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>();
            foreach (var h in diasComExpediente)
            {
                var dayOfWeek = (DayOfWeek)(h.DiaSemanaId - 1);
                schedule[dayOfWeek] = (h.HorarioInicio!.Value, h.HorarioFim!.Value);
            }
            result[usuarioId] = schedule;
        }

        return result;
    }

    private static DateTime TruncarParaHora(DateTime data) =>
        new(data.Year, data.Month, data.Day, data.Hour, 0, 0);

    internal static bool CalcularAguardandoRespostaVendedor(
        bool ehInativo,
        bool responsavelEhBot,
        bool possuiConversaAssociada,
        bool possuiConversaAtiva,
        bool teveInteracaoVendedorHumanoConversaAtiva,
        bool possuiConversaAtivaComPrimeiraMensagemTemplateBot)
    {
        if (ehInativo || responsavelEhBot)
            return false;

        if (!possuiConversaAssociada)
            return true;

        // Não aguarda primeiro contato quando só existem conversas encerradas/excluídas.
        if (!possuiConversaAtiva)
            return false;

        if (teveInteracaoVendedorHumanoConversaAtiva)
            return false;

        // Conversa iniciada por template de bot continua aguardando até existir interação humana.
        if (possuiConversaAtivaComPrimeiraMensagemTemplateBot)
            return true;

        return true;
    }
}
