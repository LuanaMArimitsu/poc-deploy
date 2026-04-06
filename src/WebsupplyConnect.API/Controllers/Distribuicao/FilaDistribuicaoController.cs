//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading.Tasks;
//using WebsupplyConnect.API.Response;
//using WebsupplyConnect.Application.DTOs.Distribuicao;
//using WebsupplyConnect.Application.Interfaces.Distribuicao;
//using WebsupplyConnect.Application.Interfaces.Usuario;
//using WebsupplyConnect.Domain.Entities.Distribuicao;
//using System.Collections.Generic; // Added for List

//namespace WebsupplyConnect.API.Controllers.Distribuicao
//{
//    /// <summary>
//    /// Controller para gerenciamento de filas de distribuição
//    /// </summary>
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FilaDistribuicaoController : ControllerBase
//    {
//        private readonly IFilaDistribuicaoService _filaService;
//        private readonly IDistribuicaoConfiguracaoReaderService _distribuicaoConfigReaderService;
//        private readonly IUsuarioReaderService _usuarioReaderService;
//        private readonly ILogger<FilaDistribuicaoController> _logger;

//        /// <summary>
//        /// Construtor do controller
//        /// </summary>
//        public FilaDistribuicaoController(
//            IFilaDistribuicaoService filaService,
//            IDistribuicaoConfiguracaoReaderService distribuicaoConfigReaderService,
//            IUsuarioReaderService usuarioReaderService,
//            ILogger<FilaDistribuicaoController> logger)
//        {
//            _filaService = filaService ?? throw new ArgumentNullException(nameof(filaService));
//            _distribuicaoConfigReaderService = distribuicaoConfigReaderService ?? throw new ArgumentNullException(nameof(distribuicaoConfigReaderService));
//            _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        /// <summary>
//        /// Obtém o próximo vendedor na fila de distribuição (sem considerar disponibilidade)
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="apenasAtivos">Indica se deve considerar apenas vendedores ativos</param>
//        /// <returns>Informações do próximo vendedor na fila</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpGet("ObterProximoVendedor/{empresaId}")]
//        public async Task<ActionResult<ApiResponse<FilaDistribuicao>>> ObterProximoVendedor(
//            int empresaId,
//            [FromQuery] bool apenasAtivos = true)
//        {
//            try
//            {
//                _logger.LogInformation("Obtendo próximo vendedor na fila para empresa {EmpresaId}. Apenas ativos: {ApenasAtivos}", 
//                    empresaId, apenasAtivos);
                
//                var proximoVendedor = await _filaService.ObterProximoVendedorFilaAsync(empresaId, apenasAtivos);
                
//                if (proximoVendedor == null)
//                {
//                    return NotFound(ApiResponse<FilaDistribuicao>.ErrorResponse(
//                        "Nenhum vendedor na fila", 
//                        $"Não há vendedores na fila de distribuição para a empresa {empresaId}"));
//                }
                
//                return Ok(ApiResponse<FilaDistribuicao>.SuccessResponse(
//                    proximoVendedor, 
//                    $"Próximo vendedor na fila: {proximoVendedor.UsuarioId}"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao obter próximo vendedor na fila para empresa {EmpresaId}", empresaId);
//                return StatusCode(500, ApiResponse<FilaDistribuicao>.ErrorResponse(
//                    "Erro ao obter próximo vendedor na fila", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Obtém o próximo vendedor disponível na fila de distribuição (considerando horários)
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="apenasAtivos">Indica se deve considerar apenas vendedores ativos</param>
//        /// <returns>Informações do próximo vendedor disponível na fila</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpGet("ObterProximoVendedorDisponivel/{empresaId}")]
//        public async Task<ActionResult<ApiResponse<FilaDistribuicaoResponseDTO>>> ObterProximoVendedorDisponivel(
//            int empresaId,
//            [FromQuery] bool apenasAtivos = true)
//        {
//            try
//            {
//                _logger.LogInformation("Obtendo próximo vendedor disponível na fila para empresa {EmpresaId}. Apenas ativos: {ApenasAtivos}", 
//                    empresaId, apenasAtivos);
                
//                var (proximoVendedor, fallbackAplicado, detalhesFallback) = await _filaService.ObterProximoVendedorDisponivelAsync(empresaId, apenasAtivos);
                
//                if (proximoVendedor == null)
//                {
//                    return NotFound(ApiResponse<FilaDistribuicaoResponseDTO>.ErrorResponse(
//                        "Nenhum vendedor disponível na fila", 
//                        $"Não há vendedores disponíveis na fila de distribuição para a empresa {empresaId}"));
//                }
                
//                //var responseDto = proximoVendedor.ToResponseDTO(fallbackAplicado, detalhesFallback);
                
//                return Ok(ApiResponse<FilaDistribuicaoResponseDTO>.SuccessResponse(
//                    //responseDto, 
//                    $"Próximo vendedor disponível na fila: {proximoVendedor.MembroEquipeId}"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao obter próximo vendedor disponível na fila para empresa {EmpresaId}", empresaId);
//                return StatusCode(500, ApiResponse<FilaDistribuicaoResponseDTO>.ErrorResponse(
//                    "Erro ao obter próximo vendedor disponível na fila", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Atualiza a posição de um vendedor na fila após receber um lead
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="vendedorId">ID do vendedor</param>
//        /// <param name="leadId">ID do lead (opcional)</param>
//        /// <returns>Resultado da operação</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPost("AtualizarPosicaoFila")]
//        public async Task<ActionResult<ApiResponse<bool>>> AtualizarPosicaoFila(
//            [FromQuery] int empresaId,
//            [FromQuery] int vendedorId,
//            [FromQuery] int? leadId = null)
//        {
//            try
//            {
//                _logger.LogInformation("Atualizando posição na fila do vendedor {VendedorId} da empresa {EmpresaId}", 
//                    vendedorId, empresaId);
                
//                var resultado = await _filaService.AtualizarPosicaoFilaAposAtribuicaoAsync(empresaId, vendedorId, leadId);
                
//                if (!resultado)
//                {
//                    return BadRequest(ApiResponse<bool>.ErrorResponse(
//                        "Falha ao atualizar posição na fila", 
//                        "Não foi possível atualizar a posição do vendedor na fila de distribuição"));
//                }
                
//                return Ok(ApiResponse<bool>.SuccessResponse(
//                    true, 
//                    $"Posição na fila do vendedor {vendedorId} atualizada com sucesso"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao atualizar posição na fila do vendedor {VendedorId}", vendedorId);
//                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
//                    "Erro ao atualizar posição na fila", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Reorganiza a fila de distribuição para uma empresa
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="vendedorId">ID do vendedor que acabou de receber um lead</param>
//        /// <returns>Resultado da operação</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPost("ReorganizarFila")]
//        public async Task<ActionResult<ApiResponse<bool>>> ReorganizarFila(
//            [FromQuery] int empresaId,
//            [FromQuery] int vendedorId)
//        {
//            try
//            {
//                _logger.LogInformation("Reorganizando fila de distribuição para empresa {EmpresaId} após atribuição ao vendedor {VendedorId}", 
//                    empresaId, vendedorId);
                
//                var resultado = await _filaService.ReorganizarFilaAposDistribuicaoAsync(empresaId, vendedorId);
                
//                if (!resultado)
//                {
//                    return BadRequest(ApiResponse<bool>.ErrorResponse(
//                        "Falha ao reorganizar fila", 
//                        "Não foi possível reorganizar a fila de distribuição"));
//                }
                
//                return Ok(ApiResponse<bool>.SuccessResponse(
//                    true, 
//                    "Fila de distribuição reorganizada com sucesso"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao reorganizar fila para empresa {EmpresaId}", empresaId);
//                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
//                    "Erro ao reorganizar fila", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Inicializa a posição de um vendedor na fila de distribuição
//        /// </summary>
//        /// <param name="request">Dados para inicializar a posição na fila</param>
//        /// <returns>Dados da posição na fila criada</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPost("InicializarPosicaoFila")]
//        public async Task<ActionResult<ApiResponse<FilaDistribuicao>>> InicializarPosicaoFila(
//            [FromBody] InicializarPosicaoFilaRequestDTO request)
//        {
//            try
//            {
//                _logger.LogInformation("Inicializando posição na fila para vendedor {VendedorId} da empresa {EmpresaId}", 
//                    request.VendedorId, request.EmpresaId);
                
//                var posicaoFila = await _filaService.InicializarPosicaoFilaVendedorAsync(request.VendedorId, request.EmpresaId);
                
//                return Ok(ApiResponse<FilaDistribuicao>.SuccessResponse(
//                    posicaoFila, 
//                    $"Posição na fila inicializada com sucesso para o vendedor {request.VendedorId}"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao inicializar posição na fila para vendedor {VendedorId}", request.VendedorId);
//                return StatusCode(500, ApiResponse<FilaDistribuicao>.ErrorResponse(
//                    "Erro ao inicializar posição na fila", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Atribui um lead pelo método de fila simples (round-robin)
//        /// </summary>
//        /// <param name="leadId">ID do lead</param>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="configuracaoId">ID da configuração de distribuição</param>
//        /// <returns>Dados da atribuição realizada</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPost("AtribuirPorFilaSimples")]
//        public Task<ActionResult<ApiResponse<AtribuicaoLead>>> AtribuirPorFilaSimples(
//            [FromQuery] int leadId,
//            [FromQuery] int empresaId,
//            [FromQuery] int configuracaoId)
//        {
//            try
//            {
//                _logger.LogInformation("Atribuindo lead {LeadId} por fila simples para empresa {EmpresaId}", 
//                    leadId, empresaId);
                
//                // Nota: Este endpoint requer uma implementação específica para obter a lista de vendedores disponíveis
//                // que pode não estar diretamente acessível no controller. Uma abordagem seria adicionar esse método
//                // ao serviço ou adicionar um serviço auxiliar que busca os vendedores.
                
//                // Para fins desta implementação, usaremos um placeholder para indicar que é necessário implementar
//                // a lógica específica.
                
//                return Task.FromResult<ActionResult<ApiResponse<AtribuicaoLead>>>(StatusCode(501, ApiResponse<AtribuicaoLead>.ErrorResponse(
//                    "Método não implementado", 
//                    "Este endpoint requer uma implementação específica para obter a lista de vendedores disponíveis")));
                
//                // A implementação completa seria algo como:
//                // var vendedoresDisponiveis = await _vendedoresService.ObterVendedoresDisponiveisAsync(empresaId);
//                // var atribuicao = await _filaService.AtribuirPorFilaSimplesAsync(leadId, vendedoresDisponiveis, configuracaoId);
//                // return Ok(ApiResponse<AtribuicaoLead>.SuccessResponse(atribuicao, "Lead atribuído com sucesso"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao atribuir lead {LeadId} por fila simples", leadId);
//                return Task.FromResult<ActionResult<ApiResponse<AtribuicaoLead>>>(StatusCode(500, ApiResponse<AtribuicaoLead>.ErrorResponse(
//                    "Erro ao atribuir lead por fila simples", ex.Message)));
//            }
//        }

//        /// <summary>
//        /// Atualiza a posição de um vendedor na fila após atribuição
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="vendedorId">ID do vendedor que recebeu o lead</param>
//        /// <returns>Resultado da atualização</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPut("AtualizarPosicaoFila/{empresaId}")]
//        public async Task<ActionResult<ApiResponse<object>>> AtualizarPosicaoFila(
//            int empresaId,
//            [FromQuery] int vendedorId)
//        {
//            try
//            {
//                _logger.LogInformation("Reorganizando fila para empresa {EmpresaId} após atribuição ao vendedor {VendedorId}", 
//                    empresaId, vendedorId);
                
//                var sucesso = await _filaService.AtualizarPosicaoFilaAposAtribuicaoAsync(empresaId, vendedorId, null);
                
//                if (!sucesso)
//                {
//                    return BadRequest(ApiResponse<object>.ErrorResponse(
//                        "Falha na reorganização", 
//                        "Não foi possível reorganizar a fila"));
//                }
                
//                return Ok(ApiResponse<object>.SuccessResponse(
//                    new { empresaId, vendedorId }, 
//                    "Fila reorganizada com sucesso"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao reorganizar fila para empresa {EmpresaId}", empresaId);
//                return StatusCode(500, ApiResponse<object>.ErrorResponse(
//                    "Erro ao reorganizar fila", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Atualiza o status de um vendedor na fila
//        /// </summary>
//        /// <param name="request">Dados para atualizar o status</param>
//        /// <returns>Resultado da operação</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPut("AtualizarStatusVendedor")]
//        public async Task<ActionResult<ApiResponse<bool>>> AtualizarStatusVendedor(
//            [FromBody] AtualizarStatusVendedorRequestDTO request)
//        {
//            try
//            {
//                _logger.LogInformation("Atualizando status do vendedor {VendedorId} para {Status} na empresa {EmpresaId}", 
//                    request.VendedorId, request.Status, request.EmpresaId);
                
//                var resultado = await _filaService.AtualizarStatusVendedorAsync(request.EmpresaId, request.VendedorId, request.Status);
                
//                if (!resultado)
//                {
//                    return BadRequest(ApiResponse<bool>.ErrorResponse(
//                        "Falha ao atualizar status", 
//                        "Não foi possível atualizar o status do vendedor na fila"));
//                }
                
//                return Ok(ApiResponse<bool>.SuccessResponse(
//                    true, 
//                    $"Status do vendedor {request.VendedorId} atualizado com sucesso para {request.Status}"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao atualizar status do vendedor {VendedorId}", request.VendedorId);
//                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
//                    "Erro ao atualizar status do vendedor", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Debug: Verifica detalhes de um vendedor específico na fila
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <param name="vendedorId">ID do vendedor</param>
//        /// <returns>Detalhes do vendedor na fila</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpGet("DebugVendedor/{empresaId}/{vendedorId}")]
//        public async Task<ActionResult<ApiResponse<object>>> DebugVendedor(int empresaId, int vendedorId)
//        {
//            try
//            {
//                _logger.LogInformation("Debug: Verificando detalhes do vendedor {VendedorId} na empresa {EmpresaId}", vendedorId, empresaId);
                
//                // Obter posição na fila
//                var posicaoFila = await _filaService.ObterPosicaoVendedorAsync(empresaId, vendedorId);
                
//                // Obter próximo vendedor para comparação
//                var proximoVendedor = await _filaService.ObterProximoVendedorFilaAsync(empresaId, true);
                
//                // Obter configuração da empresa
//                var configContext = await _distribuicaoConfigReaderService.GetConfiguracaoComRegrasAsync(empresaId);
                
//                // Obter vendedores disponíveis
//                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _usuarioReaderService.ObterVendedoresDisponiveisAsync(empresaId, configContext.Configuracao!);
                
//                var debugInfo = new
//                {
//                    VendedorSolicitado = new
//                    {
//                        Id = vendedorId,
//                        PosicaoFila = posicaoFila,
//                        EstaDisponivel = vendedoresDisponiveis.Any(v => v.Id == vendedorId),
//                        IndexNaListaDisponiveis = vendedoresDisponiveis.FindIndex(v => v.Id == vendedorId)
//                    },
//                    ProximoVendedor = new
//                    {
//                        Id = proximoVendedor?.UsuarioId,
//                        PosicaoFila = proximoVendedor?.PosicaoFila,
//                        EstaDisponivel = proximoVendedor != null ? vendedoresDisponiveis.Any(v => v.Id == proximoVendedor.UsuarioId) : false
//                    },
//                    Configuracao = new
//                    {
//                        ConsiderarHorarioTrabalho = configContext.Configuracao?.ConsiderarHorarioTrabalho,
//                        MaxLeadsAtivosVendedor = configContext.Configuracao?.MaxLeadsAtivosVendedor
//                    },
//                    VendedoresDisponiveis = new
//                    {
//                        Total = vendedoresDisponiveis.Count,
//                        Ids = vendedoresDisponiveis.Select(v => v.Id).ToList(),
//                        FallbackAplicado = fallbackAplicado,
//                        DetalhesFallback = detalhesFallback
//                    }
//                };
                
//                return Ok(ApiResponse<object>.SuccessResponse(debugInfo, "Debug info obtida com sucesso"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao obter debug do vendedor {VendedorId}", vendedorId);
//                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao obter debug", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Obtém estatísticas da fila de distribuição
//        /// </summary>
//        /// <param name="empresaId">ID da empresa</param>
//        /// <returns>Estatísticas da fila</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpGet("EstatisticasFila/{empresaId}")]
//        public async Task<ActionResult<ApiResponse<object>>> ObterEstatisticasFila(int empresaId)
//        {
//            try
//            {
//                _logger.LogInformation("Obtendo estatísticas da fila para empresa {EmpresaId}", empresaId);
                
//                // Obter dados básicos da fila
//                var proximoVendedor = await _filaService.ObterProximoVendedorFilaAsync(empresaId, true);
//                var proximoVendedorGeral = await _filaService.ObterProximoVendedorFilaAsync(empresaId, false);
                
//                var estatisticas = new
//                {
//                    EmpresaId = empresaId,
//                    ProximoVendedorAtivo = proximoVendedor != null ? new
//                    {
//                        VendedorId = proximoVendedor.UsuarioId,
//                        Posicao = proximoVendedor.PosicaoFila,
//                        Status = proximoVendedor.StatusFilaDistribuicaoId
//                    } : null,
//                    ProximoVendedorGeral = proximoVendedorGeral != null ? new
//                    {
//                        VendedorId = proximoVendedorGeral.UsuarioId,
//                        Posicao = proximoVendedorGeral.PosicaoFila,
//                        Status = proximoVendedorGeral.StatusFilaDistribuicaoId
//                    } : null,
//                    TemProximoAtivo = proximoVendedor != null,
//                    DataConsulta = DateTime.UtcNow
//                };
                
//                return Ok(ApiResponse<object>.SuccessResponse(
//                    estatisticas, 
//                    "Estatísticas da fila obtidas com sucesso"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao obter estatísticas da fila para empresa {EmpresaId}", empresaId);
//                return StatusCode(500, ApiResponse<object>.ErrorResponse(
//                    "Erro ao obter estatísticas", ex.Message));
//            }
//        }

//        /// <summary>
//        /// Teste: Adiciona vendedores à fila com logs detalhados
//        /// </summary>
//        /// <param name="request">Dados para adicionar vendedores à fila</param>
//        /// <returns>Resultado da operação</returns>
//        [Authorize(Policy = "HorarioTrabalho")]
//        [HttpPost("TesteAdicionarVendedores")]
//        public async Task<ActionResult<ApiResponse<object>>> TesteAdicionarVendedores(
//            [FromBody] List<InicializarPosicaoFilaRequestDTO> request)
//        {
//            try
//            {
//                _logger.LogInformation("Iniciando teste de adição de {Count} vendedores à fila", request.Count);
                
//                var resultados = new List<object>();
                
//                foreach (var item in request)
//                {
//                    _logger.LogInformation("Processando vendedor {VendedorId} para empresa {EmpresaId}", 
//                        item.VendedorId, item.EmpresaId);
                    
//                    try
//                    {
//                        // Verificar se já existe
//                        var posicaoExistente = await _filaService.ObterPosicaoVendedorAsync(item.EmpresaId, item.VendedorId);
                        
//                        if (posicaoExistente != null)
//                        {
//                            _logger.LogInformation("Vendedor {VendedorId} já está na fila na posição {Posicao}", 
//                                item.VendedorId, posicaoExistente.PosicaoFila);
                            
//                            resultados.Add(new
//                            {
//                                VendedorId = item.VendedorId,
//                                EmpresaId = item.EmpresaId,
//                                Status = "Já existe na fila",
//                                PosicaoFila = posicaoExistente.PosicaoFila,
//                                StatusFila = posicaoExistente.StatusFilaDistribuicaoId
//                            });
//                            continue;
//                        }
                        
//                        // Tentar adicionar
//                        var novaPosicao = await _filaService.InicializarPosicaoFilaVendedorAsync(item.VendedorId, item.EmpresaId);
                        
//                        _logger.LogInformation("Vendedor {VendedorId} adicionado com sucesso na posição {Posicao}", 
//                            item.VendedorId, novaPosicao.PosicaoFila);
                        
//                        resultados.Add(new
//                        {
//                            VendedorId = item.VendedorId,
//                            EmpresaId = item.EmpresaId,
//                            Status = "Adicionado com sucesso",
//                            PosicaoFila = novaPosicao.PosicaoFila,
//                            StatusFila = novaPosicao.StatusFilaDistribuicaoId,
//                            Id = novaPosicao.Id
//                        });
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, "Erro ao processar vendedor {VendedorId}", item.VendedorId);
                        
//                        resultados.Add(new
//                        {
//                            VendedorId = item.VendedorId,
//                            EmpresaId = item.EmpresaId,
//                            Status = "Erro",
//                            Erro = ex.Message
//                        });
//                    }
//                }
                
//                return Ok(ApiResponse<object>.SuccessResponse(
//                    new { Resultados = resultados }, 
//                    $"Processamento concluído. {resultados.Count} vendedores processados"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro no teste de adição de vendedores");
//                return StatusCode(500, ApiResponse<object>.ErrorResponse(
//                    "Erro no teste de adição de vendedores", ex.Message));
//            }
//        }
//    }
//}