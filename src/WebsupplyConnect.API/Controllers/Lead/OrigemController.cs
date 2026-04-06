using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Permissao;

namespace WebsupplyConnect.API.Controllers.Lead
{
    [Authorize(Policy = "HorarioTrabalho")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrigemController(IOrigemReaderService _origemService, IOrigemWriterService _origemWriterService, IRoleReaderService _roleReaderService) : ControllerBase
    {
        private readonly IOrigemReaderService _origemService = _origemService ?? throw new ArgumentNullException(nameof(_origemService));
        private readonly IOrigemWriterService _origemWriterService = _origemWriterService ?? throw new ArgumentNullException(nameof(_origemWriterService));
        private readonly IRoleReaderService _roleReaderService = _roleReaderService ?? throw new ArgumentNullException(nameof(_roleReaderService));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] OrigemRequest request)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, request.EmpresaId, "ORIGEM_CRIAR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para criar origens nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }
            try
            {
                await _origemWriterService.CreateAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<OrigemDTO>>>> ListarOrigensAsync([FromQuery] int empresaId)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ORIGEM_VISUALIZAR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para visualizar origens nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }

            try
            {
                var origens = await _origemService.ListarOrigensAsync();
                return Ok(ApiResponse<List<OrigemDTO>>.SuccessResponse(origens, "Origens retornadas com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar origens.", ex.ToString()));
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrigemDTO>>> GetOrigemByIdAsync(int id, [FromQuery] int empresaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var dto = await _origemService.GetOrigemByIdAsync(id);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ORIGEM_VISUALIZAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para visualizar origens especificas nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                return Ok(ApiResponse<OrigemDTO>.SuccessResponse(dto, "Origem retornada com sucesso."));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<OrigemDTO>.ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<OrigemDTO>.ErrorResponse("Erro interno ao buscar origem."));
            }
        }

        [HttpGet("tipos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<TipoOrigemDTO>>>> GetAllOrigemTiposAsync()
        {
            try
            {
                var origemTipos = await _origemService.GetAllOrigemTiposAsync();
                return Ok(ApiResponse<List<TipoOrigemDTO>>.SuccessResponse(origemTipos, "Tipos de origem retornados com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar tipos de origem.", ex.ToString()));
            }
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrigemDTO>>> UpdateOrigemAsync(int id, [FromBody] UpdateOrigemDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "ORIGEM_EDITAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para editar origens nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _origemWriterService.UpdateOrigemAsync(id, dto);
                return Ok(ApiResponse<OrigemDTO>.SuccessResponse(null, "Origem atualizada com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<OrigemDTO>.ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<OrigemDTO>.ErrorResponse("Erro interno ao atualizar origem."));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteOrigemAsync(int id, [FromQuery] int empresaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ORIGEM_EXCLUIR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para excluir origens nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _origemWriterService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Origem deletada com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao deletar origem.", ex.ToString()));
            }
        }

        [HttpGet("listagem-simples")]
        public async Task<ActionResult<ApiResponse<List<OrigemSimplesDTO>>>> ListagemSimples()
        {
            try
            {
                var origens = await _origemService.ListarOrigensSimplesAsync();
                return Ok(ApiResponse<List<OrigemSimplesDTO>>.SuccessResponse(origens, "Origens retornadas com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar origens.", ex.ToString()));
            }
        }
    }
}