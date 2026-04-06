using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.API.Controllers.Distribuicao
{
    /// <summary>
    /// Controller para gerenciamento da distribuição de leads
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class DistribuicaoController : ControllerBase
    {
        private readonly IDistribuicaoWriterService _distribuicaoWriterService;
        private readonly IDistribuicaoConfiguracaoReaderService _distribuicaoConfigReaderService;
        private readonly IHistoricoDistribuicaoReaderService _historicoDistribuicaoReaderService;
        private readonly ILogger<DistribuicaoController> _logger;

        /// <summary>
        /// Construtor do controller
        /// </summary>
        public DistribuicaoController(
            IDistribuicaoWriterService distribuicaoWriterService,
            IDistribuicaoConfiguracaoReaderService distribuicaoConfigReaderService,
            ILogger<DistribuicaoController> logger,
            IHistoricoDistribuicaoReaderService historicoDistribuicaoReaderService)
        {
            _distribuicaoWriterService = distribuicaoWriterService ?? throw new ArgumentNullException(nameof(distribuicaoWriterService));
            _distribuicaoConfigReaderService = distribuicaoConfigReaderService ?? throw new ArgumentNullException(nameof(distribuicaoConfigReaderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _historicoDistribuicaoReaderService = historicoDistribuicaoReaderService ?? throw new ArgumentNullException(nameof(historicoDistribuicaoReaderService));
        }

        [HttpPost("ExecutarDistribuicaoAutomaticaPorEquipe")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<DistribuicaoAutomaticaEquipeResponseDTO>>> ExecutarDistribuicaoAutomaticaPorEquipe(
            [FromBody] DistribuicaoAutomaticaEquipeRequestDTO request)
        {
            try
            {
                 (bool sucess, string message, DistribuicaoAutomaticaEquipeResponseDTO? response) = await _distribuicaoWriterService.ExecutarDistribuicaoAutomaticaPorEquipe(request.LeadId,
                    request.EmpresaId, request.EquipeId);

                if (!sucess)
                {
                    return NotFound(ApiResponse<DistribuicaoAutomaticaEquipeResponseDTO>.ErrorResponse(message));
                }

                return Ok(ApiResponse<DistribuicaoAutomaticaEquipeResponseDTO>.SuccessResponse(response!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar distribuição automática por equipe para empresa {EmpresaId} e equipe {EquipeId}",
                    request.EmpresaId, request.EquipeId);

                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao executar distribuição automática por equipe", ex.Message));
            }
        }

        /// <summary>
        /// Executa a distribuição automática de leads pendentes para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="maxLeads">Número máximo de leads a serem distribuídos (padrão: 100)</param>
        /// <returns>Resultado da distribuição</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("ExecutarDistribuicaoAutomatica/{empresaId}")]
        public async Task<ActionResult<ApiResponse<HistoricoDistribuicao>>> ExecutarDistribuicaoAutomatica(
            int empresaId,
            [FromQuery] int maxLeads = 100)
        {
            try
            {
                // Obter ID do usuário logado para registrar quem executou a distribuição
                int? usuarioExecutorId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (User?.Identity?.IsAuthenticated == true && int.TryParse(userIdClaim, out int userId))
                {
                    usuarioExecutorId = userId;
                }

                var resultado = await _distribuicaoWriterService.ExecutarDistribuicaoAutomaticaAsync(
                    empresaId, maxLeads, usuarioExecutorId);

                return Ok(ApiResponse<HistoricoDistribuicao>.SuccessResponse(
                    resultado,
                    $"Distribuição automática concluída com sucesso. Leads distribuídos: {resultado.TotalLeadsDistribuidos}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar distribuição automática para empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<HistoricoDistribuicao>.ErrorResponse(
                    "Erro ao executar distribuição automática", ex.Message));
            }
        }

        /// <summary>
        /// Distribui um lead específico para o melhor vendedor disponível
        /// </summary>
        /// <param name="request">Dados da distribuição</param>
        /// <returns>Dados da atribuição realizada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("DistribuirLead")]
        public async Task<ActionResult<ApiResponse<AtribuicaoLeadResponseDTO>>> DistribuirLead(
            [FromBody] DistribuirLeadRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Distribuindo lead {LeadId} para empresa {EmpresaId} com configuração {ConfiguracaoId}",
                    request.LeadId, request.EmpresaId, request.ConfiguracaoId);

                var atribuicao = await _distribuicaoWriterService.DistribuirLeadAsync(request.LeadId, request.EmpresaId);

                if (atribuicao == null)
                {
                    return NotFound(ApiResponse<AtribuicaoLeadResponseDTO>.ErrorResponse(
                        "Não foi possível atribuir o lead",
                        "Não há vendedores disponíveis ou houve um problema na distribuição"));
                }

                var responseDTO = atribuicao.ToResponseDTO();

                return Ok(ApiResponse<AtribuicaoLeadResponseDTO>.SuccessResponse(
                    responseDTO,
                    $"Lead {request.LeadId} atribuído com sucesso ao vendedor {atribuicao.MembroAtribuidoId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao distribuir lead {LeadId}", request.LeadId);
                return StatusCode(500, ApiResponse<AtribuicaoLeadResponseDTO>.ErrorResponse(
                    "Erro ao distribuir lead", ex.Message));
            }
        }



        /// <summary>
        /// Atribui um lead manualmente a um vendedor específico
        /// </summary>
        /// <param name="request">Dados da atribuição manual</param>
        /// <returns>Dados da atribuição realizada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AtribuirLeadManualmente")]
        public async Task<ActionResult<ApiResponse<AtribuicaoLeadResponseDTO>>> AtribuirLeadManualmente(
            [FromBody] AtribuicaoLeadManualDTO request)
        {
            try
            {
                // Obter ID do usuário logado que está fazendo a atribuição
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int usuarioAtribuiuId))
                {
                    return BadRequest(ApiResponse<AtribuicaoLeadResponseDTO>.ErrorResponse(
                        "Erro de autenticação",
                        "Não foi possível identificar o usuário que está realizando a atribuição"));
                }

                _logger.LogInformation(
                    "Atribuindo lead {LeadId} manualmente ao vendedor {VendedorId}. Atribuído por: {UsuarioId}",
                    request.LeadId, request.VendedorId, usuarioAtribuiuId);

                var atribuicao = await _distribuicaoWriterService.AtribuirLeadManualmenteAsync(
                    request.LeadId, request.VendedorId, usuarioAtribuiuId, request.Motivo);

                if (atribuicao == null)
                {
                    return NotFound(ApiResponse<AtribuicaoLeadResponseDTO>.ErrorResponse(
                        "Não foi possível realizar a atribuição manual",
                        "Lead, vendedor ou usuário não encontrados"));
                }

                var responseDTO = atribuicao.ToResponseDTO();

                return Ok(ApiResponse<AtribuicaoLeadResponseDTO>.SuccessResponse(
                    responseDTO,
                    $"Lead {request.LeadId} atribuído manualmente com sucesso ao vendedor {request.VendedorId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir lead {LeadId} manualmente ao vendedor {VendedorId}",
                    request.LeadId, request.VendedorId);

                return StatusCode(500, ApiResponse<AtribuicaoLeadResponseDTO>.ErrorResponse(
                    "Erro ao atribuir lead manualmente", ex.Message));
            }
        }

        /// <summary>
        /// Obtém ou atribui um vendedor responsável para um lead específico
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="forcarDistribuicao">Indica se deve forçar uma nova distribuição mesmo se já existir um responsável</param>
        /// <returns>ID do vendedor atribuído e dados da atribuição</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ObterOuAtribuirVendedor/{leadId}")]
        public async Task<ActionResult<ApiResponse<object>>> ObterOuAtribuirVendedor(
            int leadId,
            [FromQuery] bool forcarDistribuicao = false)
        {
            try
            {
                _logger.LogInformation("Obtendo ou atribuindo vendedor para lead {LeadId}. Forçar: {ForcarDistribuicao}",
                    leadId, forcarDistribuicao);

                var resultado = await _distribuicaoWriterService.ObterOuAtribuirVendedorParaLeadAsync(leadId, forcarDistribuicao);

                var responseData = new
                {
                    VendedorId = resultado.VendedorId,
                    Atribuicao = resultado.Atribuicao?.ToResponseDTO()
                };

                if (resultado.VendedorId == null)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        responseData,
                        "Não foi possível atribuir um vendedor para o lead no momento"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    responseData,
                    $"Lead {leadId} atribuído ao vendedor {resultado.VendedorId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter ou atribuir vendedor para lead {LeadId}", leadId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao obter ou atribuir vendedor", ex.Message));
            }
        }

        /// <summary>
        /// Atribui ou atualiza responsável a uma lead baseado na regra ativa da empresa
        /// </summary>
        /// <param name="leadId">ID da lead</param>
        /// <returns>Dados da atribuição realizada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AtribuirResponsavelParaLead/{leadId}")]
        public async Task<ActionResult<ApiResponse<AtribuicaoLeadResponseDTO>>> AtribuirResponsavelParaLead(int leadId)
        {
            try
            {
                _logger.LogInformation("Atribuindo responsável para lead {LeadId}", leadId);

                var atribuicao = await _distribuicaoWriterService.AtribuirResponsavelParaLeadAsync(leadId);

                if (atribuicao == null)
                {
                    return Ok(ApiResponse<AtribuicaoLeadResponseDTO>.SuccessResponse(
                        null,
                        "Lead já possui responsável ou não foi possível atribuir um responsável no momento"));
                }

                return Ok(ApiResponse<AtribuicaoLeadResponseDTO>.SuccessResponse(
                    atribuicao.ToResponseDTO(),
                    $"Responsável atribuído com sucesso para lead {leadId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir responsável para lead {LeadId}", leadId);
                return StatusCode(500, ApiResponse<AtribuicaoLeadResponseDTO>.ErrorResponse(
                    "Erro ao atribuir responsável para lead", ex.Message));
            }
        }


        /// <summary>
        /// Obtém a configuração ativa de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Configuração ativa com regras</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ConfiguracaoAtiva/{empresaId}")]
        public async Task<ActionResult<ApiResponse<object>>> ObterConfiguracaoAtiva(int empresaId)
        {
            try
            {
                _logger.LogInformation("Obtendo configuração ativa para empresa {EmpresaId}", empresaId);

                var context = await _distribuicaoConfigReaderService.GetConfiguracaoComRegrasAsync(empresaId);

                if (!context.IsValid)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Configuração não encontrada",
                        $"Não existe configuração ativa para a empresa {empresaId}"));
                }

                var responseData = new
                {
                    ConfiguracaoId = context.Configuracao!.Id,
                    Nome = context.Configuracao.Nome,
                    Descricao = context.Configuracao.Descricao,
                    Ativo = context.Configuracao.Ativo,
                    MaxLeadsAtivosVendedor = context.Configuracao.MaxLeadsAtivosVendedor,
                    TotalRegras = context.Regras.Count,
                    RegrasAtivas = context.Regras.Count(r => r.Ativo),
                    Regras = context.Regras.Select(r => new
                    {
                        r.Id,
                        r.Nome,
                        r.TipoRegraId,
                        r.Peso,
                        r.Ordem,
                        r.Ativo,
                        r.Obrigatoria
                    }).ToList()
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    responseData,
                    "Configuração obtida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração ativa para empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao obter configuração", ex.Message));
            }
        }


        /// <summary>
        /// Distribui múltiplos leads em lote
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="request">Dados para distribuição em lote incluindo leads e critérios</param>
        /// <returns>Resultado da distribuição em lote com detalhes de cada lead</returns>
        /// <response code="200">Distribuição em lote executada com sucesso</response>
        /// <response code="400">Dados inválidos ou erro na distribuição</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="500">Erro interno do servidor</response>
        /// <remarks>
        /// Este endpoint permite distribuir múltiplos leads simultaneamente, otimizando
        /// o processo para grandes volumes. Cada lead é processado individualmente
        /// seguindo as regras de distribuição configuradas.
        /// 
        /// Características:
        /// - Processamento em lote otimizado
        /// - Validação individual de cada lead
        /// - Relatório detalhado de resultados
        /// - Tratamento de erros por lead
        /// 
        /// Útil para:
        /// - Importação de leads em massa
        /// - Sincronização de dados externos
        /// - Otimização de performance
        /// </remarks>
        /// <example>
        /// <code>
        /// POST /api/Distribuicao/DistribuirLeadsEmLote/1
        /// {
        ///   "leadsIds": [1001, 1002, 1003, 1004, 1005],
        ///   "criterios": {
        ///     "forcarDistribuicao": false,
        ///     "priorizarVendedoresDisponiveis": true,
        ///     "considerarHorarioTrabalho": true
        ///   },
        ///   "configuracaoDistribuicaoId": 1
        /// }
        /// </code>
        /// </example>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("DistribuirLeadsEmLote/{empresaId}")]
        [ProducesResponseType(typeof(ApiResponse<DistribuicaoLoteResultadoDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<DistribuicaoLoteResultadoDTO>), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ApiResponse<DistribuicaoLoteResultadoDTO>), 500)]
        public async Task<ActionResult<ApiResponse<DistribuicaoLoteResultadoDTO>>> DistribuirLeadsEmLote(
            int empresaId,
            [FromBody] DistribuicaoLoteDTO request)
        {
            try
            {
                _logger.LogInformation("Iniciando distribuição de leads em lote para empresa {EmpresaId}. Total leads: {TotalLeads}",
                    request.EmpresaId, request.LeadIds.Count);

                var resultado = new DistribuicaoLoteResultadoDTO
                {
                    EmpresaId = request.EmpresaId,
                    TotalLeadsProcessados = request.LeadIds.Count
                };

                var distribuicoes = new List<DistribuicaoDetalheDTO>();
                var erros = new List<ErroDistribuicaoDTO>();

                foreach (var leadId in request.LeadIds)
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    try
                    {
                        var atribuicao = await _distribuicaoWriterService.DistribuirLeadAsync(leadId, request.EmpresaId);

                        stopwatch.Stop();

                        if (atribuicao != null)
                        {
                            distribuicoes.Add(new DistribuicaoDetalheDTO
                            {
                                LeadId = leadId,
                                VendedorId = atribuicao.MembroAtribuidoId,
                                NomeVendedor = "Vendedor", // TODO: Obter nome do vendedor
                                ScoreVendedor = 0, // TODO: Obter score do vendedor
                                Status = "Distribuído",
                                TempoProcessamentoMs = stopwatch.ElapsedMilliseconds
                            });

                            resultado.TotalLeadsDistribuidos++;
                        }
                        else
                        {
                            distribuicoes.Add(new DistribuicaoDetalheDTO
                            {
                                LeadId = leadId,
                                Status = "Falhou",
                                MensagemErro = "Não foi possível atribuir o lead",
                                TempoProcessamentoMs = stopwatch.ElapsedMilliseconds
                            });

                            resultado.TotalLeadsFalharam++;
                        }
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();

                        erros.Add(new ErroDistribuicaoDTO
                        {
                            LeadId = leadId,
                            TipoErro = "ErroInterno",
                            Mensagem = ex.Message,
                            CodigoErro = ex.GetType().Name
                        });

                        resultado.TotalLeadsFalharam++;
                    }
                }

                resultado.Distribuicoes = distribuicoes;
                resultado.Erros = erros;
                resultado.TempoExecucaoSegundos = (decimal)distribuicoes.Sum(d => d.TempoProcessamentoMs) / 1000;

                return Ok(ApiResponse<DistribuicaoLoteResultadoDTO>.SuccessResponse(
                    resultado,
                    $"Distribuição em lote concluída. Sucessos: {resultado.TotalLeadsDistribuidos}, Falhas: {resultado.TotalLeadsFalharam}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao distribuir leads em lote");
                return StatusCode(500, ApiResponse<DistribuicaoLoteResultadoDTO>.ErrorResponse(
                    "Erro ao distribuir leads em lote", ex.Message));
            }
        }

        /// <summary>
        /// Verifica o status de distribuição em tempo real
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Status atual da distribuição</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("StatusDistribuicao/{empresaId}")]
        public async Task<ActionResult<ApiResponse<object>>> ObterStatusDistribuicao(int empresaId)
        {
            try
            {
                _logger.LogInformation("Obtendo status de distribuição para empresa {EmpresaId}", empresaId);

                var ultimaDistribuicao = await _historicoDistribuicaoReaderService.GetUltimaDistribuicaoAsync(empresaId);
                var configuracaoAtiva = await _distribuicaoConfigReaderService.GetConfiguracaoComRegrasAsync(empresaId);

                var status = new
                {
                    EmpresaId = empresaId,
                    Status = configuracaoAtiva.IsValid ? "Ativo" : "Inativo",
                    UltimaExecucao = ultimaDistribuicao?.DataExecucao,
                    ProximaExecucao = ultimaDistribuicao?.DataExecucao.AddMinutes(5),
                    ConfiguracaoAtiva = configuracaoAtiva.IsValid,
                    TotalRegrasAtivas = configuracaoAtiva.Regras?.Count(r => r.Ativo) ?? 0,
                    UltimaDistribuicao = ultimaDistribuicao != null ? new
                    {
                        ultimaDistribuicao.TotalLeadsDistribuidos,
                        ultimaDistribuicao.TotalVendedoresAtivos,
                        ultimaDistribuicao.TempoExecucaoSegundos
                    } : null
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    status,
                    "Status de distribuição obtido com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status de distribuição para empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao obter status de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Pausa ou retoma a distribuição automática
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="pausar">Indica se deve pausar (true) ou retomar (false)</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPatch("PausarDistribuicao/{empresaId}")]
        public async Task<ActionResult<ApiResponse<bool>>> PausarDistribuicao(int empresaId, bool pausar)
        {
            try
            {
                _logger.LogInformation("Alterando status de distribuição para empresa {EmpresaId}. Pausar: {Pausar}",
                    empresaId, pausar);

                // TODO: Implementar lógica de pausar/retomar distribuição
                // Por enquanto, apenas simula a operação
                var resultado = true;

                return Ok(ApiResponse<bool>.SuccessResponse(
                    resultado,
                    $"Distribuição {(pausar ? "pausada" : "retomada")} com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status de distribuição para empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Erro ao alterar status de distribuição", ex.Message));
            }
        }
    }
}