using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Dashboard;
using WebsupplyConnect.Application.Interfaces.OLAP;
using WebsupplyConnect.Application.Interfaces.Dashboard;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.ETL;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;

namespace WebsupplyConnect.API.Controllers.Dashboard;

/// <summary>
/// Controller para endpoints do Dashboard OLAP - gestão de leads analíticos.
/// Catalogues e endpoints home-* não exigem DASHBOARD_VER; agregado, listagem-leads e etl/reprocessar exigem.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "HorarioTrabalho")]
public class DashboardController : ControllerBase
{
    private const string PermissaoDashboardVer = "DASHBOARD_VER";
    private const int LimiteMaximoDiasCustomizado = 366;
    private const int LimiteMaximoDiasReprocessamentoPadrao = 5000;
    private const int TamanhoPaginaMinimo = 1;
    private const int TamanhoPaginaMaximo = 200;
    private const string CodigoFiltroForaEscopo = "DASHBOARD_FORBIDDEN_FILTER_SCOPE";

    private readonly IOLAPConsultaService _olapService;
    private readonly IETLProcessamentoService _etlProcessamentoService;
    private readonly IDashboardCatalogoService _catalogoService;
    private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService;
    private readonly IDimensaoOlapReadService _dimensoesService;
    private readonly ILogger<DashboardController> _logger;
    private readonly IRoleReaderService _roleReaderService;
    private readonly IAcompanhamentoDashboardReaderService _acompanhamentoDashboardReaderService;
    private readonly ETLConfig _etlConfig;

    public DashboardController(
        IOLAPConsultaService olapService,
        IETLProcessamentoService etlProcessamentoService,
        IDashboardCatalogoService catalogoService,
        IUsuarioEmpresaReaderService usuarioEmpresaReaderService,
        IDimensaoOlapReadService dimensoesService,
        ILogger<DashboardController> logger,
        IRoleReaderService roleReaderService,
        IAcompanhamentoDashboardReaderService acompanhamentoDashboardReaderService,
        IOptions<ETLConfig> etlConfig)
    {
        _olapService = olapService;
        _etlProcessamentoService = etlProcessamentoService;
        _catalogoService = catalogoService;
        _usuarioEmpresaReaderService = usuarioEmpresaReaderService;
        _dimensoesService = dimensoesService;
        _logger = logger;
        _roleReaderService = roleReaderService;
        _acompanhamentoDashboardReaderService = acompanhamentoDashboardReaderService;
        _etlConfig = etlConfig.Value;
    }

    /// <summary>
    /// Catálogos para os filtros (dropdowns) do Dashboard: hierarquia Empresa → Equipe → Vendedor,
    /// além de Origem, Campanha e Status do Lead.
    /// Body opcional mantido por compatibilidade, mas o catálogo sempre considera
    /// todas as empresas vinculadas ao usuário em UsuarioEmpresa (independente do escopo ativo no frontend).
    /// </summary>
    [HttpPost("catalogues")]
    public async Task<ActionResult<DashboardCataloguesDTO>> ObterCatalogues([FromBody] DashboardCataloguesRequestDTO? request)
    {
        int? usuarioId = null;
        if (User.Identity?.IsAuthenticated == true &&
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
        {
            usuarioId = id;
        }

        // Intencional: ignora empresaIds do body para evitar acoplamento ao escopo ativo no frontend.
        // O serviço aplica somente o escopo de UsuarioEmpresa.
        _ = request;
        var catalogues = await _catalogoService.ObterCataloguesAsync(empresaIds: null, usuarioId);
        return Ok(catalogues);
    }

    /// <summary>
    /// Catálogo de campanhas disponíveis para o filtro considerando período e escopo enviados no body.
    /// Retorna somente campanhas com leads no intervalo selecionado.
    /// </summary>
    [HttpPost("catalogues/campanhas-disponiveis")]
    public async Task<ActionResult<List<DashboardCampanhaDisponivelDTO>>> ObterCampanhasDisponiveis([FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, _, _) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: false, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var campanhas = await _olapService.ObterCampanhasDisponiveisAsync(filtros);
        return Ok(campanhas);
    }

    /// <summary>
    /// Dados agregados das 4 abas do dashboard em uma única resposta.
    /// Recebe filtros via body (multi-seleção por dimensão).
    /// O front deve chamar ao abrir o dashboard e quando qualquer filtro mudar; troca de aba não exige nova chamada.
    /// </summary>
    [HttpPost("agregado")]
    public async Task<ActionResult<DashboardAgregadoDTO>> ObterAgregado([FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, _, _) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: false, exigeVendedorId: false);
        if (erro != null)
            return erro;

        // Execução sequencial: DbContext não é thread-safe; paralelo causaria InvalidOperationException.
        var kpis = await _olapService.ObterKPIsAsync(filtros);
        var tempoResposta = await _olapService.ObterDistribuicaoTempoRespostaAsync(filtros);
        var conversoesSemana = await _olapService.ObterConversoesSemanaAsync(filtros);
        var insights = await _olapService.ObterInsightsAsync(filtros);
        var performanceVendedores = await _olapService.ObterPerformanceVendedoresAsync(filtros);
        var performanceEquipes = await _olapService.ObterPerformanceEquipesAsync(filtros);
        var atividadePorHorario = await _olapService.ObterAtividadePorHorarioAsync(filtros);
        var leadsPorStatus = await _olapService.ObterLeadsPorStatusAsync(filtros);
        var leadsPorOrigem = await _olapService.ObterLeadsPorOrigemAsync(filtros);
        var leadsPorCampanha = await _olapService.ObterLeadsPorCampanhaAsync(filtros);
        var evolucaoLeadsStatus = await _olapService.ObterEvolucaoLeadsPorStatusAsync(filtros);
        var leadsCriadosPorHorario = await _olapService.ObterLeadsCriadosPorHorarioAsync(filtros);
        var campanhasPerformance = await _olapService.ObterPerformanceCampanhasAsync(filtros);
        var eventosPorCampanha = await _olapService.ObterEventosPorCampanhaAsync(filtros);
        var leadsConvertidosPorCampanha = await _olapService.ObterLeadsConvertidosPorCampanhaAsync(filtros);
        var funilCampanha = await _olapService.ObterFunilCampanhaAsync(filtros);
        var conversaoGeral = await _olapService.ObterConversaoGeralAsync(filtros);
        var engajamentoPorCampanha = await _olapService.ObterEngajamentoPorCampanhaAsync(filtros);
        var eventosLeadPorHorarioCampanha = await _olapService.ObterEventosLeadPorHorarioCampanhaAsync(filtros);
        var funilOportunidadesPorEtapa = await _olapService.ObterFunilOportunidadesPorEtapaAsync(filtros);
        var ultimaAtualizacao = await _olapService.ObterUltimaAtualizacaoAsync();

        var resultado = new DashboardAgregadoDTO
        {
            Geral = new DashboardGeralAgregadoDTO
            {
                Kpis = kpis,
                TempoResposta = tempoResposta,
                ConversoesSemana = conversoesSemana,
                Insights = insights,
                FunilOportunidadesPorEtapa = funilOportunidadesPorEtapa
            },
            Equipes = new DashboardEquipesAgregadoDTO
            {
                PerformanceVendedores = performanceVendedores,
                PerformanceEquipes = performanceEquipes,
                AtividadePorHorario = atividadePorHorario
            },
            Leads = new DashboardLeadsAgregadoDTO
            {
                LeadsPorStatus = leadsPorStatus,
                LeadsPorOrigem = leadsPorOrigem,
                LeadsPorCampanha = leadsPorCampanha,
                EvolucaoLeadsStatus = evolucaoLeadsStatus,
                LeadsCriadosPorHorario = leadsCriadosPorHorario
            },
            Campanhas = new DashboardCampanhasAgregadoDTO
            {
                CampanhasPerformance = campanhasPerformance,
                EventosPorCampanha = eventosPorCampanha,
                LeadsConvertidosPorCampanha = leadsConvertidosPorCampanha,
                FunilCampanha = funilCampanha,
                ConversaoGeral = conversaoGeral,
                EngajamentoPorCampanha = engajamentoPorCampanha,
                EventosLeadPorHorarioCampanha = eventosLeadPorHorarioCampanha
            },
            UltimaAtualizacao = ultimaAtualizacao
        };

        return Ok(resultado);
    }

    [HttpPost("home-agregado")]
    public async Task<ActionResult<AcompanhamentoDashboardAgregadoResponseDTO>> ObterAcompanhamentoAgregado([FromBody] AcompanhamentoDashboardAgregadoRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var dashboardRequest = CriarDashboardRequestAcompanhamento(request);

        var (erro, filtros, _, _) = await ValidarEMapearRequestAsync(usuarioId, dashboardRequest, exigePaginacao: false, exigeVendedorId: false, verificarPermissaoDashboard: false);
        if (erro != null)
            return erro;

        var resultado = await _acompanhamentoDashboardReaderService.ObterAcompanhamentoAgregadoAsync(filtros, usuarioId);
        return Ok(resultado);
    }

    [HttpPost("home-leads-pendentes")]
    public async Task<ActionResult<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>>> ObterAcompanhamentoLeadsPendentes(
        [FromBody] AcompanhamentoDashboardLeadsPendentesRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var dashboardRequest = CriarDashboardRequestAcompanhamentoPaginado(
            request,
            request.Pagina,
            request.TamanhoPagina);

        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, dashboardRequest, exigePaginacao: true, exigeVendedorId: false, verificarPermissaoDashboard: false);
        if (erro != null)
            return erro;

        var resultado = await _acompanhamentoDashboardReaderService.ObterLeadsPendentesAsync(filtros, usuarioId, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Listagem paginada de leads em primeiro atendimento aguardando resposta do cliente.
    /// Critério: conversa ativa mais recente só com envios (E), sem recebidas (R); última mensagem é envio (texto ou template).
    /// </summary>
    [HttpPost("home-leads-primeiro-atendimento-aguardando-cliente")]
    public async Task<ActionResult<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>>> ObterLeadsPrimeiroAtendimentoAguardandoCliente(
        [FromBody] AcompanhamentoDashboardLeadsPendentesRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var dashboardRequest = CriarDashboardRequestAcompanhamentoPaginado(
            request,
            request.Pagina,
            request.TamanhoPagina);

        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, dashboardRequest, exigePaginacao: true, exigeVendedorId: false, verificarPermissaoDashboard: false);
        if (erro != null)
            return erro;

        var resultado = await _acompanhamentoDashboardReaderService.ObterLeadsPrimeiroAtendimentoAguardandoClienteAsync(
            filtros, usuarioId, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    [HttpPost("home-conversas-ativas")]
    public async Task<ActionResult<PagedResultDTO<AcompanhamentoDashboardConversaAtivaItemDTO>>> ObterAcompanhamentoConversasAtivas(
        [FromBody] AcompanhamentoDashboardConversasAtivasRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var dashboardRequest = CriarDashboardRequestAcompanhamentoPaginado(
            request,
            request.Pagina,
            request.TamanhoPagina);

        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, dashboardRequest, exigePaginacao: true, exigeVendedorId: false, verificarPermissaoDashboard: false);
        if (erro != null)
            return erro;

        var resultado = await _acompanhamentoDashboardReaderService.ObterConversasAtivasAsync(filtros, usuarioId, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    [HttpPost("home-conversa-contexto-categoria-ia")]
    public async Task<ActionResult<AcompanhamentoDashboardConversaClassificacaoResponseDTO>> ObterConversaContextoCategoriaIa(
        [FromBody] AcompanhamentoDashboardConversaClassificacaoRequestDTO request)
    {
        if (request == null || request.ConversaId <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Payload inválido para consulta de contexto da conversa.", "DASHBOARD_INVALID_PAYLOAD"));

        var resultado = await _acompanhamentoDashboardReaderService.ObterConversaClassificacaoSobDemandaAsync(request.ConversaId);
        if (resultado == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Conversa não encontrada.", "DASHBOARD_CONVERSA_NAO_ENCONTRADA"));

        return Ok(resultado);
    }

    /// <summary>
    /// Listagem de leads (tabela aba Leads)
    /// </summary>
    [HttpPost("listagem-leads")]
    public async Task<ActionResult<PagedResultDTO<DashboardListagemLeadsDTO>>> ObterListagemLeads(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterListagemLeadsAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Ranking paginado de vendedores com métricas consolidadas de leads e oportunidades.
    /// Mantém contrato de ordenação/paginação enviado pelo frontend.
    /// </summary>
    [HttpPost("ranking-vendedores")]
    public async Task<ActionResult<DashboardRankingVendedoresResponseDTO>> ObterRankingVendedores(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterRankingVendedoresAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    [HttpPost("ranking-leads-por-empresa")]
    public async Task<ActionResult<PagedResultDTO<DashboardRankingLeadsEmpresaDTO>>> ObterRankingLeadsPorEmpresa(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterRankingLeadsPorEmpresaAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Ranking paginado de leads por nome de campanha (agrupa várias campanhas/empresas com o mesmo nome).
    /// Mesmos filtros e paginação dos demais rankings OLAP.
    /// </summary>
    [HttpPost("ranking-leads-por-nome-campanha")]
    public async Task<ActionResult<PagedResultDTO<DashboardRankingLeadsNomeCampanhaDTO>>> ObterRankingLeadsPorNomeCampanha(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterRankingLeadsPorNomeCampanhaAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Ranking paginado de leads por nome de campanha e empresa (par nome + empresa transacional).
    /// </summary>
    [HttpPost("ranking-leads-por-nome-campanha-e-empresa")]
    public async Task<ActionResult<PagedResultDTO<DashboardRankingLeadsNomeCampanhaEmpresaDTO>>> ObterRankingLeadsPorNomeCampanhaEEmpresa(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterRankingLeadsPorNomeCampanhaEEmpresaAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    [HttpPost("ranking-oportunidades-por-empresa")]
    public async Task<ActionResult<PagedResultDTO<DashboardRankingOportunidadesEmpresaDTO>>> ObterRankingOportunidadesPorEmpresa(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterRankingOportunidadesPorEmpresaAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    [HttpPost("ranking-oportunidades-tipo-interesse-produto")]
    public async Task<ActionResult<PagedResultDTO<DashboardRankingOportunidadesTipoInteresseProdutoDTO>>> ObterRankingOportunidadesPorTipoInteresseEProduto(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: false);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterRankingOportunidadesPorTipoInteresseEProdutoAsync(filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Listagem paginada de leads aguardando primeiro atendimento
    /// (status NOVO e sem primeiro atendimento registrado).
    /// Não aplica filtro de período/datas — apenas paginação e filtros opcionais (empresa, equipe, etc.).
    /// </summary>
    [HttpPost("leads-aguardando-atendimento")]
    public async Task<ActionResult<PagedResultDTO<DashboardListagemLeadsDTO>>> ObterLeadsAguardandoAtendimento(
        [FromBody] DashboardLeadsAguardandoRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestLeadsAguardandoAsync(usuarioId, request);
        if (erro != null)
            return erro;

        var resultado = await _acompanhamentoDashboardReaderService.ObterLeadsAguardandoAtendimentoAsync(
            filtros, usuarioId, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Listagem paginada de leads aguardando resposta do vendedor
    /// (já tiveram interação humana, mas última mensagem não-bot é do cliente).
    /// Não aplica filtro de período/datas — apenas paginação e filtros opcionais (empresa, equipe, etc.).
    /// </summary>
    [HttpPost("leads-aguardando-resposta")]
    public async Task<ActionResult<PagedResultDTO<DashboardListagemLeadsDTO>>> ObterLeadsAguardandoResposta(
        [FromBody] DashboardLeadsAguardandoRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestLeadsAguardandoAsync(usuarioId, request);
        if (erro != null)
            return erro;

        var resultado = await _acompanhamentoDashboardReaderService.ObterLeadsAguardandoRespostaAsync(
            filtros, usuarioId, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Listagem paginada de leads sob responsabilidade de um vendedor específico.
    /// Usado quando o front clica em um vendedor na aba Equipes → performanceVendedores.
    /// O campo vendedorId no body corresponde ao VendedorId retornado em DashboardPerformanceVendedorDTO.
    /// </summary>
    [HttpPost("leads-por-vendedor")]
    public async Task<ActionResult<PagedResultDTO<DashboardListagemLeadsDTO>>> ObterLeadsPorVendedor(
        [FromBody] DashboardFiltrosRequestDTO request)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var (erro, filtros, pagina, tamanhoPagina) = await ValidarEMapearRequestAsync(usuarioId, request, exigePaginacao: true, exigeVendedorId: true);
        if (erro != null)
            return erro;

        var resultado = await _olapService.ObterLeadsPorVendedorAsync(
            request.VendedorId!.Value, filtros, pagina, tamanhoPagina);
        return Ok(resultado);
    }

    private async Task<(ActionResult? Erro, FiltrosDashboardDTO Filtros, int Pagina, int TamanhoPagina)> ValidarEMapearRequestAsync(
        int usuarioId,
        DashboardFiltrosRequestDTO? request,
        bool exigePaginacao,
        bool exigeVendedorId,
        bool verificarPermissaoDashboard = true)
    {
        if (request == null)
            return (BadRequest(ApiResponse<object>.ErrorResponse("Payload inválido para filtros do dashboard.", "DASHBOARD_INVALID_PAYLOAD")), new FiltrosDashboardDTO(), 0, 0);

        if (!request.TipoPeriodo.HasValue)
            return (BadRequest(ApiResponse<object>.ErrorResponse("Payload inválido para filtros do dashboard.", "DASHBOARD_INVALID_PAYLOAD")), new FiltrosDashboardDTO(), 0, 0);

        if (request.TipoPeriodo == TipoPeriodoEnum.Customizado)
        {
            if (!request.DataInicio.HasValue || !request.DataFim.HasValue)
                return (StatusCode(StatusCodes.Status422UnprocessableEntity, ApiResponse<object>.ErrorResponse("Intervalo de datas inválido.", "DASHBOARD_INVALID_DATE_RANGE")), new FiltrosDashboardDTO(), 0, 0);

            if (request.DataInicio.Value > request.DataFim.Value)
                return (StatusCode(StatusCodes.Status422UnprocessableEntity, ApiResponse<object>.ErrorResponse("Intervalo de datas inválido.", "DASHBOARD_INVALID_DATE_RANGE")), new FiltrosDashboardDTO(), 0, 0);

            var janelaDias = (request.DataFim.Value.Date - request.DataInicio.Value.Date).TotalDays + 1;
            if (janelaDias > LimiteMaximoDiasCustomizado)
                return (StatusCode(StatusCodes.Status422UnprocessableEntity, ApiResponse<object>.ErrorResponse("Intervalo de datas inválido.", "DASHBOARD_INVALID_DATE_RANGE")), new FiltrosDashboardDTO(), 0, 0);
        }
        else if (request.TipoPeriodo == TipoPeriodoEnum.MesAnterior || request.TipoPeriodo == TipoPeriodoEnum.MesesAnteriores)
        {
            if (!AnoMesValidoOuNulo(request.AnoReferencia, request.MesReferencia))
            {
                return (StatusCode(StatusCodes.Status422UnprocessableEntity,
                        ApiResponse<object>.ErrorResponse("Referência de mês inválida.", "DASHBOARD_INVALID_MONTH_REFERENCE")),
                    new FiltrosDashboardDTO(), 0, 0);
            }

            if (request.TipoPeriodo == TipoPeriodoEnum.MesesAnteriores)
            {
                if (!AnoMesValidoOuNulo(request.AnoInicioHistorico, request.MesInicioHistorico))
                {
                    return (StatusCode(StatusCodes.Status422UnprocessableEntity,
                            ApiResponse<object>.ErrorResponse("Início histórico inválido.", "DASHBOARD_INVALID_HISTORICAL_START")),
                        new FiltrosDashboardDTO(), 0, 0);
                }

                if (request.AnoInicioHistorico.HasValue && request.MesInicioHistorico.HasValue &&
                    request.AnoReferencia.HasValue && request.MesReferencia.HasValue)
                {
                    var inicioHistorico = new DateTime(request.AnoInicioHistorico.Value, request.MesInicioHistorico.Value, 1);
                    var inicioMesReferencia = new DateTime(request.AnoReferencia.Value, request.MesReferencia.Value, 1);
                    if (inicioHistorico >= inicioMesReferencia)
                    {
                        return (StatusCode(StatusCodes.Status422UnprocessableEntity,
                                ApiResponse<object>.ErrorResponse("Início histórico deve ser anterior ao mês de referência.",
                                    "DASHBOARD_INVALID_HISTORICAL_RANGE")),
                            new FiltrosDashboardDTO(), 0, 0);
                    }
                }
            }
        }
        else if (request.TipoPeriodo == TipoPeriodoEnum.MesReferenciaCompleto)
        {
            if (!AnoMesObrigatorioValido(request.AnoReferencia, request.MesReferencia))
            {
                return (StatusCode(StatusCodes.Status422UnprocessableEntity,
                        ApiResponse<object>.ErrorResponse("Referência de mês inválida.", "DASHBOARD_INVALID_MONTH_REFERENCE")),
                    new FiltrosDashboardDTO(), 0, 0);
            }
        }

        var pagina = request.Pagina ?? 0;
        var tamanhoPagina = request.TamanhoPagina ?? 0;
        if (exigePaginacao)
        {
            if (!request.Pagina.HasValue || !request.TamanhoPagina.HasValue ||
                pagina < 1 ||
                tamanhoPagina < TamanhoPaginaMinimo ||
                tamanhoPagina > TamanhoPaginaMaximo)
            {
                return (BadRequest(ApiResponse<object>.ErrorResponse("Parâmetros de paginação inválidos.", "DASHBOARD_INVALID_PAGINATION")), new FiltrosDashboardDTO(), 0, 0);
            }
        }

        if (exigeVendedorId && (!request.VendedorId.HasValue || request.VendedorId.Value <= 0))
            return (BadRequest(ApiResponse<object>.ErrorResponse("Payload inválido para filtros do dashboard.", "DASHBOARD_INVALID_PAYLOAD")), new FiltrosDashboardDTO(), 0, 0);

        var filtros = request.ToFiltrosDashboardDTO();
        if (exigeVendedorId)
        {
            filtros.VendedorIds = [request.VendedorId!.Value];
        }

        var erroEscopo = await ValidarEscopoPermissoesEFiltrosAsync(usuarioId, filtros, verificarPermissaoDashboard);
        if (erroEscopo != null)
            return (erroEscopo, new FiltrosDashboardDTO(), 0, 0);

        return (null, filtros, pagina, tamanhoPagina);
    }

    /// <summary>
    /// Valida e mapeia request dos endpoints leads-aguardando-atendimento e leads-aguardando-resposta.
    /// Não exige período/datas; exige apenas paginação e aplica as mesmas validações de escopo/permissão.
    /// </summary>
    private async Task<(ActionResult? Erro, FiltrosDashboardDTO Filtros, int Pagina, int TamanhoPagina)> ValidarEMapearRequestLeadsAguardandoAsync(
        int usuarioId,
        DashboardLeadsAguardandoRequestDTO? request)
    {
        if (request == null)
            return (BadRequest(ApiResponse<object>.ErrorResponse("Payload inválido para filtros do dashboard.", "DASHBOARD_INVALID_PAYLOAD")), new FiltrosDashboardDTO(), 0, 0);

        var pagina = request.Pagina ?? 0;
        var tamanhoPagina = request.TamanhoPagina ?? 0;
        if (!request.Pagina.HasValue || !request.TamanhoPagina.HasValue ||
            pagina < 1 ||
            tamanhoPagina < TamanhoPaginaMinimo ||
            tamanhoPagina > TamanhoPaginaMaximo)
        {
            return (BadRequest(ApiResponse<object>.ErrorResponse("Parâmetros de paginação inválidos.", "DASHBOARD_INVALID_PAGINATION")), new FiltrosDashboardDTO(), 0, 0);
        }

        var filtros = request.ToFiltrosDashboardDTO();
        var erroEscopo = await ValidarEscopoPermissoesEFiltrosAsync(usuarioId, filtros, verificarPermissaoDashboard: true);
        if (erroEscopo != null)
            return (erroEscopo, new FiltrosDashboardDTO(), 0, 0);

        return (null, filtros, pagina, tamanhoPagina);
    }

    private async Task<ActionResult?> ValidarEscopoPermissoesEFiltrosAsync(
        int usuarioId,
        FiltrosDashboardDTO filtros,
        bool verificarPermissaoDashboard)
    {
        var vinculos = await _usuarioEmpresaReaderService.GetVinculosPorUsuarioIdAsync(usuarioId);
        var empresasPermitidas = vinculos
            .Select(v => v.EmpresaId)
            .Distinct()
            .ToList();

        var empresaIds = filtros.ObterEmpresaIds();
        if (empresasPermitidas.Count > 0 && empresaIds.Any(id => !empresasPermitidas.Contains(id)))
        {
            var invalidas = empresaIds.Where(id => !empresasPermitidas.Contains(id)).Distinct().Order().ToList();
            var permitidas = empresasPermitidas.Order().ToList();
            return FiltroForaEscopo(
                $"Empresa(s) selecionada(s) não estão vinculadas ao seu usuário. empresaId(s) rejeitado(s): [{string.Join(", ", invalidas)}]; empresaId(s) permitido(s) pelo vínculo: [{string.Join(", ", permitidas)}].");
        }

        if (verificarPermissaoDashboard)
        {
            // Sem empresaIds explícito no payload, assume "todas as empresas permitidas pelo vínculo do usuário".
            // Assim evitamos exigir permissão global quando o usuário já possui permissões por empresa.
            var empresasParaValidarPermissao = empresaIds.Count > 0
                ? empresaIds
                : empresasPermitidas;

            if (empresasParaValidarPermissao.Count > 0)
            {
                var temPermissaoPorEmpresa = await UsuarioTemPermissaoTodasEmpresasAsync(usuarioId, empresasParaValidarPermissao);
                if (!temPermissaoPorEmpresa)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar o dashboard.", "PERMISSAO_NEGADA"));
            }
            else
            {
                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, null, PermissaoDashboardVer);
                if (!temPermissao)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar o dashboard.", "PERMISSAO_NEGADA"));
            }
        }

        var (equipesSelecionadas, erroEquipes) = await ValidarEquipeIdsAsync(filtros.ObterEquipeIds(), empresasPermitidas, empresaIds);
        if (erroEquipes != null)
            return FiltroForaEscopo(erroEquipes);

        var (vendedoresValidos, erroVendedores) = await ValidarVendedorIdsAsync(
            filtros.ObterVendedorIds(),
            empresasPermitidas,
            empresaIds,
            equipesSelecionadas!.Select(e => e.Id).ToHashSet());
        if (!vendedoresValidos)
            return FiltroForaEscopo(erroVendedores!);

        var (campanhasValidas, erroCampanhas) = await ValidarCampanhaNomesAsync(filtros.ObterCampanhaNomes(), empresasPermitidas, empresaIds);
        if (!campanhasValidas)
            return FiltroForaEscopo(erroCampanhas!);

        var (funisValidos, erroFunis) = await ValidarFunilIdsAsync(filtros.ObterFunilIds(), empresasPermitidas, empresaIds);
        if (!funisValidos)
            return FiltroForaEscopo(erroFunis!);

        var (etapasValidas, erroEtapas) = await ValidarEtapaIdsAsync(filtros.ObterEtapaIds(), empresasPermitidas, empresaIds);
        if (!etapasValidas)
            return FiltroForaEscopo(erroEtapas!);

        var (origensValidas, erroOrigens) = await ValidarOrigemIdsAsync(filtros.ObterOrigemIds());
        if (!origensValidas)
            return FiltroForaEscopo(erroOrigens!);

        var (statusValidos, erroStatus) = await ValidarStatusLeadIdsAsync(filtros.ObterStatusLeadIds());
        if (!statusValidos)
            return FiltroForaEscopo(erroStatus!);

        return null;
    }

    private static DashboardFiltrosRequestDTO CriarDashboardRequestAcompanhamento(
        AcompanhamentoDashboardAgregadoRequestDTO request)
    {
        return new DashboardFiltrosRequestDTO
        {
            TipoPeriodo = request.TipoPeriodo,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            AnoReferencia = request.AnoReferencia,
            MesReferencia = request.MesReferencia,
            AnoInicioHistorico = request.AnoInicioHistorico,
            MesInicioHistorico = request.MesInicioHistorico,
            EmpresaIds = request.EmpresaIds,
            EquipeIds = request.EquipeIds,
            OrigemIds = request.OrigemIds,
            CampanhaNome = request.CampanhaNome,
            CampanhaNomes = request.CampanhaNomes
        };
    }

    private static DashboardFiltrosRequestDTO CriarDashboardRequestAcompanhamentoPaginado(
        AcompanhamentoDashboardAgregadoRequestDTO request,
        int? pagina,
        int? tamanhoPagina)
    {
        var dashboardRequest = CriarDashboardRequestAcompanhamento(request);
        dashboardRequest.Pagina = pagina;
        dashboardRequest.TamanhoPagina = tamanhoPagina;
        return dashboardRequest;
    }

    private async Task<bool> UsuarioTemPermissaoTodasEmpresasAsync(int usuarioId, List<int> empresaIds)
    {
        foreach (var empresaId in empresaIds.Distinct())
        {
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, PermissaoDashboardVer);
            if (!temPermissao)
                return false;
        }

        return true;
    }

    private async Task<(List<Domain.Entities.OLAP.Dimensoes.DimensaoEquipe> equipes, string? erroDetalhe)> ValidarEquipeIdsAsync(
        List<int> equipeOrigemIds,
        List<int> empresasPermitidas,
        List<int> empresasFiltro)
    {
        if (equipeOrigemIds.Count == 0)
            return ([], null);

        var dimensoesEquipe = await _dimensoesService.ObterDimensoesEquipeNaoExcluidasAsync();
        var equipesSelecionadas = dimensoesEquipe
            .Where(e => equipeOrigemIds.Contains(e.EquipeOrigemId))
            .ToList();

        if (equipesSelecionadas.Count != equipeOrigemIds.Count)
        {
            var encontrados = equipesSelecionadas.Select(e => e.EquipeOrigemId).ToHashSet();
            var naoEncontrados = equipeOrigemIds.Where(id => !encontrados.Contains(id)).Order().ToList();
            var encontradasStr = equipesSelecionadas.Count > 0
                ? string.Join("; ", equipesSelecionadas.OrderBy(e => e.EquipeOrigemId).Select(e =>
                    $"equipeOrigemId={e.EquipeOrigemId}, dimensaoEquipeId={e.Id}"))
                : "(nenhuma)";
            return ([], $"Equipe(s) inexistente(s) ou excluída(s) no OLAP. equipeOrigemId(s) não encontrado(s): [{string.Join(", ", naoEncontrados)}]; encontrada(s) no OLAP: {encontradasStr}.");
        }

        var empresasEfetivas = empresasFiltro.Count > 0 ? empresasFiltro : empresasPermitidas;
        if (empresasEfetivas.Count > 0)
        {
            var foraEscopo = equipesSelecionadas.Where(e => !empresasEfetivas.Contains(e.EmpresaId)).ToList();
            if (foraEscopo.Count > 0)
            {
                var trechos = foraEscopo.OrderBy(e => e.EquipeOrigemId).Select(e =>
                    $"equipeOrigemId={e.EquipeOrigemId}, dimensaoEquipeId={e.Id}, empresaId={e.EmpresaId}");
                var empresasStr = string.Join(", ", empresasEfetivas.Order());
                return ([], $"Equipe(s) fora do escopo de empresa(s) selecionado(s): {string.Join("; ", trechos)}. empresaId(s) considerado(s) no filtro: [{empresasStr}].");
            }
        }

        return (equipesSelecionadas, null);
    }

    private async Task<(bool ok, string? detalhe)> ValidarVendedorIdsAsync(
        List<int> vendedorOrigemIds,
        List<int> empresasPermitidas,
        List<int> empresasFiltro,
        HashSet<int> equipeDimIdsFiltro)
    {
        if (vendedorOrigemIds.Count == 0)
            return (true, null);

        var dimensoesVendedor = await _dimensoesService.ObterDimensoesVendedorNaoExcluidasAsync();
        var vendedoresSelecionados = dimensoesVendedor
            .Where(v => vendedorOrigemIds.Contains(v.UsuarioOrigemId))
            .ToList();

        if (vendedoresSelecionados.Count != vendedorOrigemIds.Count)
        {
            var encontrados = vendedoresSelecionados.Select(v => v.UsuarioOrigemId).ToHashSet();
            var naoEncontrados = vendedorOrigemIds.Where(id => !encontrados.Contains(id)).Order().ToList();
            var encontradosStr = vendedoresSelecionados.Count > 0
                ? string.Join("; ", vendedoresSelecionados.OrderBy(v => v.UsuarioOrigemId).Select(v =>
                    $"usuarioOrigemId={v.UsuarioOrigemId}, dimensaoVendedorId={v.Id}"))
                : "(nenhum)";
            return (false, $"Vendedor(es) inexistente(s) ou excluído(s) no OLAP. usuarioOrigemId(s) não encontrado(s): [{string.Join(", ", naoEncontrados)}]; encontrado(s) no OLAP: {encontradosStr}.");
        }

        var empresasEfetivas = empresasFiltro.Count > 0 ? empresasFiltro : empresasPermitidas;
        if (empresasEfetivas.Count > 0)
        {
            var foraEmpresa = vendedoresSelecionados
                .Where(v => !v.EmpresaId.HasValue || !empresasEfetivas.Contains(v.EmpresaId.Value))
                .OrderBy(v => v.UsuarioOrigemId)
                .ToList();
            if (foraEmpresa.Count > 0)
            {
                var trechos = foraEmpresa.Select(v =>
                    $"usuarioOrigemId={v.UsuarioOrigemId}, dimensaoVendedorId={v.Id}, empresaIdOlap={(v.EmpresaId?.ToString() ?? "null")}, equipeDimensaoIdOlap={(v.EquipeId?.ToString() ?? "null")}");
                var empresasStr = string.Join(", ", empresasEfetivas.Order());
                return (false,
                    $"Vendedor(es) fora do escopo de empresa(s) permitido(s) ou selecionado(s): {string.Join("; ", trechos)}. empresaId(s) considerado(s) no filtro: [{empresasStr}].");
            }
        }

        if (equipeDimIdsFiltro.Count > 0)
        {
            var foraEquipe = vendedoresSelecionados
                .Where(v => !v.EquipeId.HasValue || !equipeDimIdsFiltro.Contains(v.EquipeId.Value))
                .OrderBy(v => v.UsuarioOrigemId)
                .ToList();
            if (foraEquipe.Count > 0)
            {
                var trechos = foraEquipe.Select(v =>
                    $"usuarioOrigemId={v.UsuarioOrigemId}, dimensaoVendedorId={v.Id}, empresaIdOlap={(v.EmpresaId?.ToString() ?? "null")}, equipeDimensaoIdOlap={(v.EquipeId?.ToString() ?? "null")}");
                var equipesFiltroStr = string.Join(", ", equipeDimIdsFiltro.Order());
                return (false,
                    $"Vendedor(es) não pertencem à(s) equipe(s) filtrada(s): {string.Join("; ", trechos)}. dimensaoEquipeId(s) exigido(s) pelo filtro: [{equipesFiltroStr}].");
            }
        }

        return (true, null);
    }

    private async Task<(bool ok, string? detalhe)> ValidarCampanhaNomesAsync(
        List<string> nomesCampanha,
        List<int> empresasPermitidas,
        List<int> empresasFiltro)
    {
        if (nomesCampanha.Count == 0)
            return (true, null);

        var dimensoesCampanha = await _dimensoesService.ObterDimensoesCampanhaNaoExcluidasAsync();
        var empresasEfetivas = empresasFiltro.Count > 0 ? empresasFiltro : empresasPermitidas;

        foreach (var nome in nomesCampanha)
        {
            var candidatos = dimensoesCampanha;
            if (empresasEfetivas.Count > 0)
                candidatos = candidatos.Where(c => empresasEfetivas.Contains(c.EmpresaId)).ToList();
            else
                candidatos = candidatos.ToList();

            var existe = candidatos.Any(c =>
                string.Equals(c.Nome.Trim(), nome.Trim(), StringComparison.OrdinalIgnoreCase));
            if (!existe)
            {
                var empresasStr = empresasEfetivas.Count > 0
                    ? string.Join(", ", empresasEfetivas.Order())
                    : "(todas as empresas permitidas)";
                return (false,
                    $"Nenhuma campanha no OLAP com nome '{nome}' no escopo de empresa. empresaId(s) considerado(s): [{empresasStr}].");
            }
        }

        return (true, null);
    }

    private async Task<(bool ok, string? detalhe)> ValidarFunilIdsAsync(
        List<int> funilOrigemIds,
        List<int> empresasPermitidas,
        List<int> empresasFiltro)
    {
        if (funilOrigemIds.Count == 0)
            return (true, null);

        var funisSelecionados = new List<DimensaoFunil>();
        foreach (var id in funilOrigemIds.Distinct().Order())
        {
            var dim = await _dimensoesService.ObterDimensaoFunilPorOrigemIdAsync(id);
            if (dim == null)
                return (false, $"Funil inexistente ou excluído no OLAP. funilOrigemId={id}.");

            funisSelecionados.Add(dim);
        }

        var empresasEfetivas = empresasFiltro.Count > 0 ? empresasFiltro : empresasPermitidas;
        if (empresasEfetivas.Count > 0)
        {
            var fora = funisSelecionados.Where(f => !empresasEfetivas.Contains(f.EmpresaOrigemId)).ToList();
            if (fora.Count > 0)
            {
                var trechos = fora.OrderBy(f => f.FunilOrigemId).Select(f =>
                    $"funilOrigemId={f.FunilOrigemId}, dimensaoFunilId={f.Id}, empresaOrigemId={f.EmpresaOrigemId}");
                var empresasStr = string.Join(", ", empresasEfetivas.Order());
                return (false, $"Funil(is) fora do escopo de empresa(s) selecionado(s): {string.Join("; ", trechos)}. empresaId(s) considerado(s) no filtro: [{empresasStr}].");
            }
        }

        return (true, null);
    }

    private async Task<(bool ok, string? detalhe)> ValidarEtapaIdsAsync(
        List<int> etapaOrigemIds,
        List<int> empresasPermitidas,
        List<int> empresasFiltro)
    {
        if (etapaOrigemIds.Count == 0)
            return (true, null);

        var empresasEfetivas = empresasFiltro.Count > 0 ? empresasFiltro : empresasPermitidas;
        foreach (var id in etapaOrigemIds.Distinct().Order())
        {
            var dim = await _dimensoesService.ObterDimensaoEtapaFunilPorOrigemIdAsync(id);
            if (dim == null)
                return (false, $"Etapa inexistente ou excluída no OLAP. etapaOrigemId={id}.");

            if (empresasEfetivas.Count == 0)
                continue;

            var empresaOrigemId = dim.Funil?.EmpresaOrigemId
                ?? (await _dimensoesService.ObterDimensaoFunilPorOrigemIdAsync(dim.FunilOrigemId))?.EmpresaOrigemId;
            if (!empresaOrigemId.HasValue || !empresasEfetivas.Contains(empresaOrigemId.Value))
            {
                var empresasStr = string.Join(", ", empresasEfetivas.Order());
                return (false,
                    $"Etapa(s) fora do escopo de empresa(s) selecionado(s): etapaOrigemId={dim.EtapaOrigemId}, funilOrigemId={dim.FunilOrigemId}, empresaOrigemIdEsperada={empresaOrigemId?.ToString() ?? "null"}. empresaId(s) considerado(s) no filtro: [{empresasStr}].");
            }
        }

        return (true, null);
    }

    private Task<(bool ok, string? detalhe)> ValidarOrigemIdsAsync(List<int> origemOrigemIds) =>
        _dimensoesService.ValidarFiltroOrigemOrigemIdsParaDashboardAsync(origemOrigemIds);

    private Task<(bool ok, string? detalhe)> ValidarStatusLeadIdsAsync(List<int> statusOrigemIds) =>
        _dimensoesService.ValidarFiltroStatusLeadOrigemIdsParaDashboardAsync(statusOrigemIds);

    /// <summary>
    /// Escopo inválido de filtros é erro de payload/filtro e não de autenticação/autorização.
    /// Retornar 400 evita que o frontend trate esse caso como sessão expirada.
    /// O campo <c>error</c> traz <see cref="CodigoFiltroForaEscopo"/>, um separador e o motivo detalhado (parseável com <c>Split(\" | \", 2)</c>).
    /// </summary>
    private BadRequestObjectResult FiltroForaEscopo(string detalhe)
        => BadRequest(ApiResponse<object>.ErrorResponse(
            "Filtros fora do escopo permitido.",
            $"{CodigoFiltroForaEscopo} | {detalhe}"));

    private int ObterLimiteDiasReprocessamento()
        => _etlConfig.ReprocessamentoMaximoDias > 0
            ? _etlConfig.ReprocessamentoMaximoDias
            : LimiteMaximoDiasReprocessamentoPadrao;

    private static bool AnoMesValidoOuNulo(int? ano, int? mes)
    {
        if (!ano.HasValue && !mes.HasValue)
            return true;

        if (!ano.HasValue || !mes.HasValue)
            return false;

        return ano is >= 1900 and <= 9999 && mes is >= 1 and <= 12;
    }

    private static bool AnoMesObrigatorioValido(int? ano, int? mes)
    {
        if (!ano.HasValue || !mes.HasValue)
            return false;

        return ano is >= 1900 and <= 9999 && mes is >= 1 and <= 12;
    }

    /// <summary>
    /// Data e status da última atualização do OLAP (para indicador visual no dashboard)
    /// </summary>
    [HttpGet("ultima-atualizacao")]
    public async Task<ActionResult<DashboardUltimaAtualizacaoDTO>> ObterUltimaAtualizacao()
    {
        var resultado = await _olapService.ObterUltimaAtualizacaoAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// Dispara reprocessamento completo do ETL para popular tabelas fato e dimensões.
    /// Use para carga inicial ou quando as tabelas OLAP estão vazias.
    /// Exemplo: POST /api/Dashboard/etl/reprocessar?dataInicio=2024-01-01&amp;dataFim=2025-02-07
    /// </summary>
    [HttpPost("etl/reprocessar")]
    public async Task<ActionResult> ReprocessarETL(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);
        var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, null, PermissaoDashboardVer);
        if (!temPermissao)
            return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar o dashboard.", "PERMISSAO_NEGADA"));

        if (dataFim <= dataInicio)
            return BadRequest("dataFim deve ser posterior a dataInicio");

        var limiteDias = ObterLimiteDiasReprocessamento();
        if ((dataFim - dataInicio).TotalDays > limiteDias)
            return BadRequest($"Período máximo: {limiteDias} dias. Use um intervalo menor.");

        try
        {
            _logger.LogInformation("Reprocessamento ETL solicitado: {DataInicio} a {DataFim}", dataInicio, dataFim);
            await _etlProcessamentoService.ReprocessarCompletoAsync(dataInicio, dataFim);
            return Ok(new { mensagem = "ETL reprocessado com sucesso.", dataInicio, dataFim });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reprocessar ETL");
            return StatusCode(500, new { erro = ex.Message });
        }
    }
}
