using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de distribuição de leads
    /// </summary>
    public class DistribuicaoWriterService : IDistribuicaoWriterService
    {
        private readonly IDistribuicaoRepository _distribuicaoRepository;
        private readonly IAtribuicaoLeadService _atribuicaoLeadService;
        //private readonly IRedistribuicaoService _redistribuicaoService;
        private readonly ILeadReaderService _leadReaderService;
        private readonly ILeadWriterService _leadWriterService;
        private readonly IEquipeReaderService _equipeReaderService;
        private readonly IMembroEquipeReaderService _membroEquipeReaderService;
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly IDistribuicaoConfiguracaoReaderService _configurationService;
        private readonly IMetricaVendedorService _metricaService;
        private readonly IFilaDistribuicaoService _filaService;
        private readonly IScoreCalculationService _scoreCalculationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificacaoClient _notificacaoClient;
        private readonly ITransferenciaLeadCommand _transferenciaCommand;
        private readonly IEmpresaReaderService _empresaReaderService;
        private readonly ILogger<DistribuicaoWriterService> _logger;
        private readonly IConversaReaderService _conversaReaderService;
        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public DistribuicaoWriterService(
            IDistribuicaoRepository distribuicaoRepository,
            IAtribuicaoLeadService atribuicaoLeadService,
            IUsuarioReaderService usuarioReaderService,
            //IRedistribuicaoService redistribuicaoService,
            ILeadReaderService leadReader,
            ILeadWriterService leadWriterService,
            IEquipeReaderService equipeReaderService,
            IMembroEquipeReaderService membroEquipeReaderService,
            IDistribuicaoConfiguracaoReaderService configurationService,
            IMetricaVendedorService metricaService,
            IFilaDistribuicaoService filaService,
            IScoreCalculationService scoreCalculationService,
            IUnitOfWork unitOfWork,
            INotificacaoClient notificacaoClient,
            IConversaReaderService conversaReaderService,
            ITransferenciaLeadCommand transferenciaCommand,
            IEmpresaReaderService empresaReaderService,
            ILogger<DistribuicaoWriterService> logger)
        {
            _distribuicaoRepository = distribuicaoRepository ?? throw new ArgumentNullException(nameof(distribuicaoRepository));
            _atribuicaoLeadService = atribuicaoLeadService ?? throw new ArgumentNullException(nameof(atribuicaoLeadService));
            _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
            //_redistribuicaoService = redistribuicaoService ?? throw new ArgumentNullException(nameof(redistribuicaoService));
            _leadReaderService = leadReader ?? throw new ArgumentNullException(nameof(leadReader));
            _leadWriterService = leadWriterService ?? throw new ArgumentNullException(nameof(leadWriterService));
            _equipeReaderService = equipeReaderService ?? throw new ArgumentNullException(nameof(equipeReaderService));
            _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _metricaService = metricaService ?? throw new ArgumentNullException(nameof(metricaService));
            _filaService = filaService ?? throw new ArgumentNullException(nameof(filaService));
            _scoreCalculationService = scoreCalculationService ?? throw new ArgumentNullException(nameof(scoreCalculationService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _atribuicaoLeadService = atribuicaoLeadService ?? throw new ArgumentNullException(nameof(atribuicaoLeadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
            _transferenciaCommand = transferenciaCommand ?? throw new ArgumentNullException(nameof(transferenciaCommand));
            _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
            _notificacaoClient = notificacaoClient ?? throw new ArgumentNullException(nameof(notificacaoClient));
        }

        /// <summary>
        /// Executa a distribuição automática de leads pendentes para uma empresa
        /// </summary>
        public async Task<HistoricoDistribuicao> ExecutarDistribuicaoAutomaticaAsync(
            int empresaId,
            int maxLeads = 100,
            int? usuarioExecutorId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var dataExecucao = TimeHelper.GetBrasiliaTime();

            try
            {
                // Obter contexto de configuração e regras
                var configContext = await _configurationService.GetConfiguracaoComRegrasAsync(empresaId);
                if (!configContext.IsValid)
                {
                    _logger.LogError("Nenhuma configuração de distribuição ativa encontrada para a empresa {EmpresaId}", empresaId);
                    throw new ApplicationException($"Nenhuma configuração de distribuição ativa encontrada para a empresa {empresaId}");
                }

                var configuracao = configContext.Configuracao!;

                // Obter vendedores disponíveis
                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _usuarioReaderService.ObterVendedoresDisponiveisAsync(empresaId, configuracao);
                if (!vendedoresDisponiveis.Any())
                {

                    // Criar histórico sem vendedores disponíveis
                    var historicoSemVendedores = new HistoricoDistribuicao(
                        configuracaoDistribuicaoId: configuracao.Id,
                        dataExecucao: dataExecucao,
                        totalLeadsDistribuidos: 0,
                        totalVendedoresAtivos: 0,
                        tempoExecucaoSegundos: (int)stopwatch.Elapsed.TotalSeconds,
                        usuarioExecutouId: usuarioExecutorId,
                        resultadoDistribuicao: "Nenhum vendedor disponível",
                        errosOcorridos: null
                    );

                    await _distribuicaoRepository.SalvarHistoricoDistribuicaoAsync(historicoSemVendedores);

                    return historicoSemVendedores;
                }

                // Processar cada lead pendente
                int leadsDistribuidos = 0;
                var resultadoDistribuicao = new List<Dictionary<string, object>>();
                var erros = new List<string>();

                // Criar histórico da distribuição
                var historico = new HistoricoDistribuicao(
                    configuracaoDistribuicaoId: configuracao.Id,
                    dataExecucao: dataExecucao,
                    totalLeadsDistribuidos: leadsDistribuidos,
                    totalVendedoresAtivos: vendedoresDisponiveis.Count,
                    tempoExecucaoSegundos: (int)stopwatch.Elapsed.TotalSeconds,
                    usuarioExecutouId: usuarioExecutorId,
                    resultadoDistribuicao: JsonSerializer.Serialize(resultadoDistribuicao),
                    errosOcorridos: erros.Any() ? string.Join("; ", erros) : null
                );

                await _distribuicaoRepository.SalvarHistoricoDistribuicaoAsync(historico);

                return historico;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro durante distribuição automática para empresa {EmpresaId}", empresaId);

                // Criar histórico com erro
                var historicoErro = new HistoricoDistribuicao(
                    configuracaoDistribuicaoId: 1, // Valor padrão, deveria ser obtido corretamente
                    usuarioExecutouId: usuarioExecutorId
                );

                // Atualizar o resultado com erro
                historicoErro.AtualizarResultado(
                    totalLeadsDistribuidos: 0,
                    totalVendedoresAtivos: 0,
                    resultado: "Distribuição falhou",
                    erros: $"Erro: {ex.Message}"
                );

                try
                {
                    await _distribuicaoRepository.SalvarHistoricoDistribuicaoAsync(historicoErro);
                }
                catch (Exception inner)
                {
                    _logger.LogError(inner, "Erro ao salvar histórico de erro");
                }

                throw new ApplicationException($"Erro durante distribuição automática: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Distribui um lead específico para o melhor vendedor disponível
        /// </summary>
        public async Task<AtribuicaoLead?> DistribuirLeadAsync(int leadId, int empresaId)
        {
            try
            {
                // Obter lead
                var lead = await _leadReaderService.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    throw new ApplicationException($"Lead não encontrado. ID: {leadId}");
                }

                // Obter contexto de configuração e regras
                var configContext = await _configurationService.GetConfiguracaoComRegrasAsync(empresaId);
                if (!configContext.IsValid)
                {
                    throw new ApplicationException($"Nenhuma configuração de distribuição ativa encontrada para a empresa {empresaId}");
                }

                var configuracao = configContext.Configuracao!;
                var regras = configContext.Regras;

                // Obter vendedores disponíveis
                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _usuarioReaderService.ObterVendedoresDisponiveisAsync(empresaId, configuracao);
                if (!vendedoresDisponiveis.Any())
                {
                    return null;
                }

                // Se não houver regras, usar distribuição simples por fila
                //if (!regras.Any())
                //{
                //    return await _filaService.AtribuirPorFilaSimplesAsync(leadId, vendedoresDisponiveis, configuracao.Id, fallbackAplicado, detalhesFallback);
                //}

                // Calcular scores para todos os vendedores elegíveis
                var scoresVendedores = new Dictionary<int, (decimal Score, Dictionary<string, object> Detalhes)>();

                foreach (var vendedor in vendedoresDisponiveis)
                {
                    // Calcular score usando serviço especializado
                    var scoreData = await _scoreCalculationService.CalcularScoreVendedorAsync(leadId, vendedor, empresaId, configContext.Configuracao);
                    decimal scoreVendedor = scoreData.ScoreTotal;
                    Dictionary<string, object> detalhesVendedor = new()
                    {
                        { "vendedorId", vendedor.Id },
                        { "nomeVendedor", vendedor.Nome },
                        { "scoreTotal", scoreData.ScoreTotal },
                        { "posicao", scoreData.Posicao },
                        { "elegivel", scoreData.Elegivel },
                        { "leadsAtivos", scoreData.LeadsAtivos },
                        { "taxaConversao", scoreData.TaxaConversao },
                        { "velocidadeMedia", scoreData.VelocidadeMediaAtendimento },
                        { "posicaoFila", scoreData.PosicaoFila },
                        { "scoresPorRegra", scoreData.ScoresPorRegra }
                    };

                    if (scoreVendedor > 0) // Só considera vendedores com score positivo
                    {
                        scoresVendedores.Add(vendedor.Id, (scoreVendedor, detalhesVendedor));
                    }
                }

                // Se nenhum vendedor tiver score positivo, tenta usar round-robin
                //if (!scoresVendedores.Any())
                //{
                //    return await _filaService.AtribuirPorFilaSimplesAsync(leadId, vendedoresDisponiveis, configuracao.Id, fallbackAplicado, detalhesFallback);
                //}

                // Selecionar vendedor com maior score
                var melhorVendedor = scoresVendedores
                    .OrderByDescending(kv => kv.Value.Score)
                    .First();

                // Obter o ID do tipo de atribuição "AUTOMATICA"
                int tipoAtribuicaoId = 1; // Valor padrão

                // Determinar qual regra foi decisiva (a de maior peso no score final)
                int? regraDecisivaId = null;
                string motivoAtribuicao = $"Melhor score: {melhorVendedor.Value.Score:F2}";

                var regrasAplicadas = melhorVendedor.Value.Detalhes.ContainsKey("regras")
                    ? melhorVendedor.Value.Detalhes["regras"] as List<Dictionary<string, object>>
                    : null;

                if (regrasAplicadas != null && regrasAplicadas.Any())
                {
                    var regraPrincipal = regrasAplicadas
                        .OrderByDescending(r => Convert.ToDecimal(r["scoreComPeso"]))
                        .First();

                    regraDecisivaId = Convert.ToInt32(regraPrincipal["id"]);
                    motivoAtribuicao = $"Melhor score: {melhorVendedor.Value.Score:F2}, regra principal: {regraPrincipal["nome"]}";
                }

                // Serializar informações para os campos JSON
                string scoresCalculados = JsonSerializer.Serialize(melhorVendedor.Value.Detalhes);
                string vendedoresElegiveis = JsonSerializer.Serialize(scoresVendedores.Keys);

                // Criar parâmetros aplicados com informações da distribuição
                var parametrosAplicados = new Dictionary<string, object>
                {
                    { "configuracaoId", configuracao.Id },
                    { "empresaId", configuracao.EmpresaId },
                    { "regraDecisivaId", regraDecisivaId },
                    { "regraDecisivaNome", regraDecisivaId.HasValue ? "Regra aplicada" : "Sem regra específica" },
                    { "totalVendedoresElegiveis", scoresVendedores.Count },
                    { "scoreVencedor", melhorVendedor.Value.Score },
                    { "fallbackHorarioAplicado", fallbackAplicado },
                    { "detalhesFallback", detalhesFallback ?? "" },
                    { "dataDistribuicao", DateTime.UtcNow },
                    { "metodoDistribuicao", "AUTOMATICA" }
                };

                string parametrosAplicadosJson = JsonSerializer.Serialize(parametrosAplicados);

                // Criar registro de atribuição
                var atribuicao = new AtribuicaoLead(
                    leadId: leadId,
                    membroAtribuidoId: melhorVendedor.Key,
                    tipoAtribuicaoId: tipoAtribuicaoId,
                    motivoAtribuicao: motivoAtribuicao,
                    atribuicaoAutomatica: true,
                    configuracaoDistribuicaoId: configuracao.Id,
                    regraDistribuicaoId: regraDecisivaId,
                    scoreVendedor: melhorVendedor.Value.Score,
                    membroAtribuiuId: null, // Atribuição automática não tem usuário que atribuiu
                    parametrosAplicados: parametrosAplicadosJson,
                    vendedoresElegiveis: vendedoresElegiveis,
                    scoresCalculados: scoresCalculados
                );

                // Registrar fallback de horário se aplicado
                if (fallbackAplicado && !string.IsNullOrEmpty(detalhesFallback))
                {
                    atribuicao.RegistrarFallbackHorario(detalhesFallback);
                }

                // Adicionar atribuição
                await _atribuicaoLeadService.CriarAtribuicaoAsync(atribuicao);

                await _leadWriterService.AtualizarResponsavel(lead.Id, melhorVendedor.Key, lead.EquipeId!.Value, configuracao.EmpresaId);

                // Atualizar métricas e posição na fila do vendedor
                await _metricaService.AtualizarMetricasVendedorAposAtribuicaoAsync(melhorVendedor.Key, configuracao.EmpresaId);
                await _filaService.AtualizarPosicaoFilaAposAtribuicaoAsync(
                    configuracao.EmpresaId, melhorVendedor.Key, leadId);

                return atribuicao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao distribuir lead {LeadId}", leadId);
                throw new ApplicationException($"Erro ao distribuir lead {leadId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Realiza a atribuição manual de um lead a um vendedor específico
        /// </summary>
        public async Task<AtribuicaoLead?> AtribuirLeadManualmenteAsync(
            int leadId,
            int vendedorId,
            int usuarioAtribuiuId,
            string motivo)
        {
            try
            {
                // Validações básicas
                if (leadId <= 0)
                    throw new ApplicationException("ID do lead deve ser maior que zero");

                if (vendedorId <= 0)
                    throw new ApplicationException("ID do vendedor deve ser maior que zero");

                if (usuarioAtribuiuId <= 0)
                    throw new ApplicationException("ID do usuário que está atribuindo deve ser maior que zero");

                if (string.IsNullOrWhiteSpace(motivo))
                    throw new ApplicationException("Motivo da atribuição manual é obrigatório");

                // Verificar se o lead existe
                var lead = await _leadReaderService.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    _logger.LogError("Lead não encontrado. ID: {LeadId}", leadId);
                    throw new ApplicationException($"Lead não encontrado. ID: {leadId}");
                }

                // Verificar se o lead já tem um responsável e criar histórico se necessário
                int? responsavelAnteriorId = lead.ResponsavelId;

                // Obter o ID do tipo de atribuição "MANUAL"
                int tipoAtribuicaoId = 1; // Valor padrão para o tipo manual

                // Registrar informações do responsável anterior (se houver) para parametrosAplicados
                string? parametrosAplicados = null;
                if (responsavelAnteriorId.HasValue && responsavelAnteriorId.Value != vendedorId)
                {
                    // Criar um objeto para serializar para JSON
                    var parametros = new Dictionary<string, object>
                    {
                        { "responsavelAnteriorId", responsavelAnteriorId.Value },
                        { "atribuidoPor", usuarioAtribuiuId }
                    };

                    // Serializar para JSON
                    parametrosAplicados = JsonSerializer.Serialize(parametros);
                }

                // Criar registro de atribuição
                var atribuicao = new AtribuicaoLead(
                    leadId: leadId,
                    membroAtribuidoId: vendedorId,
                    tipoAtribuicaoId: tipoAtribuicaoId,
                    motivoAtribuicao: motivo,
                    atribuicaoAutomatica: false,
                    configuracaoDistribuicaoId: null, // Atribuição manual não usa configuração
                    regraDistribuicaoId: null, // Atribuição manual não usa regra
                    scoreVendedor: null, // Atribuição manual não tem score
                    membroAtribuiuId: usuarioAtribuiuId,
                    parametrosAplicados: parametrosAplicados,
                    vendedoresElegiveis: null, // Não há vendedores elegíveis além do escolhido
                    scoresCalculados: null // Não há scores calculados
                );

                // Adicionar atribuição
                await _atribuicaoLeadService.CriarAtribuicaoAsync(atribuicao);

                // Atualizar responsável do lead
                await _leadWriterService.AtualizarResponsavel(lead.Id, vendedorId, lead.EquipeId!.Value, lead.EmpresaId);

                // Registrar histórico de status se houver mudança de responsável
                if (responsavelAnteriorId != vendedorId)
                {
                    await _atribuicaoLeadService.UpdateAsync(atribuicao);
                }

                // Atualizar métricas do vendedor
                await _metricaService.AtualizarMetricasVendedorAposAtribuicaoAsync(vendedorId, lead.EmpresaId);

                return atribuicao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir lead manualmente. Lead: {LeadId}, Vendedor: {VendedorId}",
                    leadId, vendedorId);
                throw new ApplicationException($"Erro ao atribuir lead manualmente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém ou atribui um vendedor responsável para um lead específico
        /// </summary>
        public async Task<(int? VendedorId, AtribuicaoLead? Atribuicao)> ObterOuAtribuirVendedorParaLeadAsync(
            int leadId,
            bool forcarDistribuicao = false)
        {
            try
            {
                // Validar o leadId
                if (leadId <= 0)
                    throw new ApplicationException("ID do lead deve ser maior que zero");

                // Buscar o lead
                var lead = await _leadReaderService.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    _logger.LogError("Lead não encontrado. ID: {LeadId}", leadId);
                    throw new ApplicationException($"Lead não encontrado. ID: {leadId}");
                }

                // Se o lead já tem um responsável (> 0) e não estamos forçando redistribuição, retornar o responsável atual
                if (lead.ResponsavelId > 0 && !forcarDistribuicao)
                {
                    // Buscar a última atribuição para detalhes
                    var ultimaAtribuicao = await _atribuicaoLeadService.ObterUltimaAtribuicaoLeadAsync(leadId);

                    return (lead.ResponsavelId, ultimaAtribuicao);
                }

                // Se chegamos aqui, precisamos atribuir um novo vendedor
                var atribuicao = await DistribuirLeadAsync(leadId, lead.EmpresaId);

                if (atribuicao == null)
                {
                    _logger.LogError("Não foi possível atribuir o lead {LeadId} a nenhum vendedor", leadId);
                    return (null, null);
                }

                return (atribuicao.MembroAtribuidoId, atribuicao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter ou atribuir vendedor para lead {LeadId}", leadId);
                throw new ApplicationException($"Erro ao obter ou atribuir vendedor para lead: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Atribui ou atualiza responsável a uma lead baseado na regra ativa da empresa
        /// </summary>
        /// <param name="leadId">ID da lead</param>
        /// <returns>Registro de atribuição criado ou null em caso de falha</returns>
        public async Task<AtribuicaoLead?> AtribuirResponsavelParaLeadAsync(int leadId)
        {
            try
            {
                // Validar o leadId
                if (leadId <= 0)
                    throw new ApplicationException("ID do lead deve ser maior que zero");

                // Obter a lead para extrair o empresaId
                var lead = await _leadReaderService.GetLeadByIdAsync(leadId);
                if (lead == null)
                {
                    _logger.LogError("Lead não encontrado. ID: {LeadId}", leadId);
                    throw new ApplicationException($"Lead não encontrado. ID: {leadId}");
                }

                var empresaId = lead.EmpresaId;

                // Executar a distribuição usando o método existente
                var atribuicao = await DistribuirLeadAsync(leadId, empresaId);

                return atribuicao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir responsável para lead {LeadId}", leadId);
                throw new ApplicationException($"Erro ao atribuir responsável para lead {leadId}: {ex.Message}", ex);
            }
        }

        public async Task<AtribuicaoPorEquipeDTO> AtribuirResponsavelPorEquipe(int leadId, int empresaId)
        {
            try
            {
                if (empresaId <= 0)
                {
                    _logger.LogError("EmpresaId inválido: {EmpresaId}", empresaId);
                    throw new ApplicationException("ID da empresa deve ser maior que zero");
                }

                var equipe = await _equipeReaderService.GetEquipePadraoAsync(empresaId);
                if (equipe == null)
                {
                    _logger.LogError("Equipe não encontrada. EmpresaId: {EmpresaId}", empresaId);
                    throw new ApplicationException($"Equipe não encontrada. EmpresaId {empresaId}");
                }

                var configContext = await _configurationService.GetConfiguracaoComRegrasAsync(empresaId);
                if (!configContext.IsValid)
                {
                    throw new ApplicationException($"Nenhuma configuração de distribuição ativa encontrada para a empresa {equipe.EmpresaId}");
                }

                var configuracao = configContext.Configuracao!;

                // Obter vendedores disponíveis
                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _membroEquipeReaderService.ObterVendedoresDisponiveisPorEquipeAsync(equipe.Id, configuracao);
                if (vendedoresDisponiveis.Count == 0)
                {
                    _logger.LogError("Nenhum vendedor está disponível para realizar a distribuição");
                    throw new ApplicationException("Nenhum vendedor está disponível para realizar a distribuição");
                }

                var atribuicao = await _filaService.AtribuirPorFilaSimplesAsync(leadId, vendedoresDisponiveis, configuracao.Id, fallbackAplicado, detalhesFallback);

                if (atribuicao == null)
                {
                    _logger.LogError("Não foi possível atribuir o lead {LeadId} a nenhum vendedor da equipe {EquipeId}", leadId, equipe.Id);
                    throw new ApplicationException($"Não foi possível atribuir o lead {leadId} a nenhum vendedor da equipe {equipe.Id}");
                }

                var atribuicaoDto = new AtribuicaoPorEquipeDTO
                {
                    AtribuicaoLead = atribuicao,
                    EquipeId = equipe.Id
                };

                return atribuicaoDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir responsável por equipe pela empresaId {EmpresaId}", empresaId);
                throw new ApplicationException($"Erro ao atribuir responsável por equipe para empresa.", ex);
            }
        }

        public async Task<(bool sucess, string message, DistribuicaoAutomaticaEquipeResponseDTO? response)> ExecutarDistribuicaoAutomaticaPorEquipe(int leadId, int empresaId, int equipeId)
        {
            try
            {
                // 1. Buscar equipe ou equipe padrão
                var equipe = await _equipeReaderService.GetEquipeByIdAsync(equipeId);

                if (equipe == null)
                {
                    equipe = await _equipeReaderService.GetEquipePadraoAsync(empresaId);
                    if (equipe == null)
                    {
                        _logger.LogError("Equipe padrão não encontrada. EmpresaId: {EmpresaId}", empresaId);
                        return (false, "Equipe padrão não encontrada.", null);
                    }
                }

                // 2. Verificar última conversa do lead
                var ultimaConversaLead = await _conversaReaderService.GetUltimaConversaLead(leadId, equipe.Id);

                if (ultimaConversaLead != null && ultimaConversaLead.Lead?.ResponsavelId != null)
                {
                    var membros = await _membroEquipeReaderService.ObterMembrosPorUsuarioAsync(ultimaConversaLead.UsuarioId, equipeId);
                    var vendedorAtivo = membros.FirstOrDefault();

                    if (vendedorAtivo != null && !vendedorAtivo.Excluido && vendedorAtivo.StatusMembroEquipe!.Codigo == "ATIVO")
                    {
                        await _transferenciaCommand.ExecutarSemOportunidadeAsync(leadId, vendedorAtivo.Id, vendedorAtivo.EquipeId, empresaId);

                        return await FinalizarDistribuicaoParaMembroAsync(
                            leadId,
                            vendedorAtivo,
                            empresaId,
                            notificarLider: true);
                    }
                }

                var configContext = await _configurationService.GetConfiguracaoComRegrasAsync(empresaId);

                if (!configContext.IsValid)
                {
                    _logger.LogError(
                        "Nenhuma configuração de distribuição ativa encontrada para a empresa {EmpresaId}",
                        empresaId
                    );
                    return (false, "Nenhuma configuração de distribuição ativa encontrada para a empresa.", null);
                }

                var configuracao = configContext.Configuracao!;

                // 4. Buscar vendedores disponíveis
                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _membroEquipeReaderService.ObterVendedoresDisponiveisPorEquipeAsync(equipe.Id, configuracao);

                if (vendedoresDisponiveis.Count == 0)
                    return await FinalizarDistribuicaoParaLiderAsync(leadId, equipe.Id, empresaId);

                // 5. Atribuir por fila
                var atribuicao = await _filaService.AtribuirPorFilaSimplesAsync(
                   leadId,
                   vendedoresDisponiveis,
                   configuracao.Id,
                   fallbackAplicado,
                   detalhesFallback,
                   empresaId
                );

                if (atribuicao == null)
                    return await FinalizarDistribuicaoParaLiderAsync(leadId, equipe.Id, empresaId);

                var membro = await _membroEquipeReaderService.GetByIdAsync(atribuicao.MembroAtribuidoId);

                return await FinalizarDistribuicaoParaMembroAsync(
                    leadId,
                    membro,
                    empresaId,
                    notificarLider: true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(
                    ex,
                    "Erro ao atribuir responsável por equipe pela empresaId {EmpresaId}",
                    empresaId
                );
                throw;
            }
        }

        public async Task<(bool sucess, string message, int? responsavelId)> ObterVendedorParaDistribuicaoPorEquipe(int empresaId, int equipeId)
        {
            try
            {
                var equipe = await _equipeReaderService.GetEquipeByIdAsync(equipeId);
                if (equipe == null)
                {
                    var equipePadrao = await _equipeReaderService.GetEquipePadraoAsync(empresaId);
                    if (equipePadrao == null)
                    {
                        _logger.LogError("Equipe padrão não encontrada. EmpresaId: {EmpresaId}", empresaId);
                        return (false, "Equipe padrão não encontrada.", null);
                    }

                    equipe = equipePadrao;
                }

                var configContext = await _configurationService.GetConfiguracaoComRegrasAsync(empresaId);
                if (!configContext.IsValid)
                {
                    _logger.LogError("Nenhuma configuração de distribuição ativa encontrada para a empresa {equipe.EmpresaId}", empresaId);
                    return (false, "Nenhuma configuração de distribuição ativa encontrada para a empresa.", null);
                }

                var configuracao = configContext.Configuracao!;

                // Obter vendedores disponíveis
                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _membroEquipeReaderService.ObterVendedoresDisponiveisPorEquipeAsync(equipe.Id, configuracao);
                if (vendedoresDisponiveis.Count == 0)
                {
                    _logger.LogError("Nenhum vendedor está disponível para associar a um lead.");
                    return (false, "Nenhum vendedor está disponível para associar a um lead.", null);
                }

                var atribuicao = await _filaService.ObterVendedorPorFilaSimples(vendedoresDisponiveis, configuracao.Id, empresaId);

                return (true, "Vendendor encontrado sucesso.", atribuicao);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao buscar um responsável por equipe pela empresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        private async Task<(bool sucess, string message, DistribuicaoAutomaticaEquipeResponseDTO? response)> FinalizarDistribuicaoParaLiderAsync(
        int leadId,
        int equipeId,
        int empresaId)
        {
            var responsavel = await _membroEquipeReaderService.ObterLiderDaEquipeAsync(equipeId);

            await _transferenciaCommand.ExecutarAsync(leadId, responsavel.Id, equipeId, empresaId);
            await _unitOfWork.CommitAsync();

            await _notificacaoClient.NovoLead(new NotificarNovoLeadDTO
            {
                LeadId = leadId,
                UsuarioId = responsavel.UsuarioId
            });

            var response = new DistribuicaoAutomaticaEquipeResponseDTO(
                responsavel.Id,
                responsavel.Usuario!.Nome);

            return (true, "Distribuição realizada com sucesso.", response);
        }


        private async Task<(bool sucess, string message, DistribuicaoAutomaticaEquipeResponseDTO? response)> FinalizarDistribuicaoParaMembroAsync(
        int leadId,
        MembroEquipe membro,
        int empresaId,
        bool notificarLider)
        {
            await _transferenciaCommand.ExecutarAsync(leadId, membro.Id, membro.EquipeId, empresaId);
            await _unitOfWork.CommitAsync();
            await _notificacaoClient.NovoLead(new NotificarNovoLeadDTO
            {
                LeadId = leadId,
                UsuarioId = membro.UsuarioId
            });

            if (notificarLider)
            {
                var lider = await _membroEquipeReaderService.ObterLiderDaEquipeAsync(membro.EquipeId);

                await _notificacaoClient.NovoLeadVendedor(new NotificarNovoLeadVendedorDTO
                {
                    LeadId = leadId,
                    UsuarioId = lider.UsuarioId,
                    NomeVendedor = membro.Usuario.Nome
                });
            }

            var response = new DistribuicaoAutomaticaEquipeResponseDTO(
                membro.Id,
                membro.Usuario!.Nome);

            return (true, "Distribuição realizada com sucesso.", response);
        }

    }
}
