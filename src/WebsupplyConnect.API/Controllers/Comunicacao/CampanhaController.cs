using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.Campanha;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Permissao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    public class CampanhaController(
        ICampanhaWriterService campanhaWriterService,
        ICampanhaReaderService campanhaReaderService,
        IRoleReaderService _roleReaderService,
        ILogger<CampanhaController> logger) : ControllerBase
    {
        private readonly ICampanhaWriterService _campanhaWriterService = campanhaWriterService;
        private readonly ICampanhaReaderService _campanhaReaderService = campanhaReaderService;
        private readonly IRoleReaderService _roleReaderService = _roleReaderService;
        private readonly ILogger<CampanhaController> _logger = logger;

        /// <summary> Cria uma nova campanha para a empresa informada.</summary>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<ApiResponse<object>>> CriarCampanha([FromBody] CriarCampanhaDTO request)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, request.EmpresaId, "CAMPANHA_CRIAR");
                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não tem permissão para criar campanhas nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                var dto = new CriarCampanhaApiDTO
                {
                    Nome = request.Nome,
                    Codigo = request.Codigo,
                    EmpresaId = request.EmpresaId,
                    DataInicio = request.DataInicio,
                    DataFim = request.DataFim,
                    EquipeId = request.EquipeId,
                    Temporaria = false,
                };

                await _campanhaWriterService.CriarCampanhaAsync(dto, commit: true);

                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar campanha. Objeto de envio: {request}", request);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        /// <summary> Lista campanhas filtradas de acordo com os parâmetros enviados.</summary>
        [HttpPost]
        [Route("listar")]
        public async Task<ActionResult<ApiResponse<CampanhaPaginadaDTO>>> ListarCampanhas(FiltroCampanhaDTO filtroCampanhaDTO)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtroCampanhaDTO.EmpresaId, "CAMPANHA_VISUALIZAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não tem permissão para visualizar campanhas nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                var campanhas = await _campanhaReaderService.ListarCampanhasAsync(filtroCampanhaDTO);
                return Ok(ApiResponse<CampanhaPaginadaDTO>.SuccessResponse(campanhas));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<CampanhaPaginadaDTO>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar campanhas.");
                return StatusCode(500, ApiResponse<List<CampanhaDTO>>.ErrorResponse("Erro ao listar campanhas.", ex.ToString()));
            }
        }

        /// <summary> Edita uma campanha existente.</summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> EditarCampanha(int id, [FromBody] EditarCampanhaDTO request)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, request.EmpresaId, "CAMPANHA_EDITAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para editar campanhas nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _campanhaWriterService.EditarCampanhaAsync(
                    id,
                    request
                );

                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar campanha. Objeto de envio: {request}", request);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        /// <summary> Exclui logicamente uma campanha.</summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> ExcluirCampanha(int id, int empresaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "CAMPANHA_EXCLUIR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para excluir campanhas nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _campanhaWriterService.DeleteCampanhaAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir campanha. Id da campanha: {id}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        /// <summary>Listagem simples de campanhas.</summary>
        [HttpGet]
        [Route("listaSimples/{empresaId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ListCampanhaResponseDTO>>>> ListagemSimples(int empresaId)
        {
            try
            {
                var campanhas = await _campanhaReaderService.ListagemSimplesAsync(empresaId);
                return Ok(ApiResponse<IEnumerable<ListCampanhaResponseDTO>>.SuccessResponse(campanhas));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<IEnumerable<ListCampanhaResponseDTO>>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar campanhas não transferidas para a empresa ID {empresaId}.", empresaId);
                return StatusCode(500, ApiResponse<IEnumerable<ListCampanhaResponseDTO>>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        /// <summary> Transfere todos os leads de uma campanha temporária para uma campanha definitiva. </summary>
        [HttpPost]
        [Route("transferirCampanha/{campanhaOrigemId:int}/{campanhaDestinoId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> TransferirCampanha(
            int campanhaOrigemId,
            int campanhaDestinoId)
        {
            try
            {
                await _campanhaWriterService.TransferirCampanhasAsync(campanhaOrigemId, campanhaDestinoId);

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    Mensagem = $"Campanha {campanhaOrigemId} transferida com sucesso para a campanha {campanhaDestinoId}."
                }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir campanha de {origem} para {destino}",
                    campanhaOrigemId, campanhaDestinoId);

                return StatusCode(500,
                    ApiResponse<object>.ErrorResponse("Erro ao transferir campanha.", ex.ToString()));
            }
        }

        /// <summary> Listagem de campanha por ID. </summary>
        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CampanhaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CampanhaDTO>>> ListarCampanhaById(int id)
        {
            try
            {
                var campanha = await _campanhaReaderService.ListarCampanhaByIdAsync(id);
                return Ok(ApiResponse<CampanhaDTO>.SuccessResponse(campanha));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter campanha por ID {id}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao obter campanha.", ex.ToString()));
            }
        }
    }
}
