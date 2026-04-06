using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Permissao.Permissao;
using WebsupplyConnect.Application.Interfaces.Perfil;
using WebsupplyConnect.Application.Interfaces.Permissao;

namespace WebsupplyConnect.API.Controllers.Perfil
{
    [Authorize(Policy = "HorarioTrabalho")]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissaoController(IPermissaoReaderService permissaoaReaderService, IPermissaoWriterService permissaoWriterService) : ControllerBase
    {
        private readonly IPermissaoReaderService _permissaoReaderService = permissaoaReaderService ?? throw new ArgumentNullException(nameof(permissaoaReaderService));
        private readonly IPermissaoWriterService _permissaoWriterService = permissaoWriterService ?? throw new ArgumentNullException(nameof(permissaoWriterService));

        [HttpPost("Listar")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissaoDTO>>>> Get()
        {
            try
            {
                var permissoes = await _permissaoReaderService.GetPermissoes();
                return Ok(ApiResponse<object>.SuccessResponse(permissoes));
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
        [HttpPost("ListarPaginado")]
        public async Task<ActionResult<ApiResponse<PermissaoPaginadaDTO>>> GetPaginado(PermissaoFiltroDTO filtro)
        {
            try
            {
                var permissoes = await _permissaoReaderService.GetPermissoes(filtro);
                return Ok(ApiResponse<object>.SuccessResponse(permissoes));
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetById(int id)
        {
            try
            {
                var permissao = await _permissaoReaderService.GetPermissaoPorIdAsync(id);
                if (permissao == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Permissão não encontrada"));
                return Ok(ApiResponse<object>.SuccessResponse(permissao));
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> Criar([FromBody] CreatePermissaoDTO dto)
        {
            try
            {
                await _permissaoWriterService.CriarPermissaoAsync(dto);
                return Ok(ApiResponse<object>.SuccessResponse("Permissão criada com sucesso"));
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Atualizar(int id, [FromBody] UpdatePermissaoDTO dto)
        {
            try
            {
                await _permissaoWriterService.AtualizarPermissaoAsync(id, dto.Nome, dto.Descricao, dto.IsCritica);
                return Ok(ApiResponse<object>.SuccessResponse("Permissão atualizada com sucesso"));
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

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
        {
            try
            {
                await _permissaoWriterService.ExcluirPermissaoAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse("Permissão excluída com sucesso"));
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

        [HttpPost("{id:int}/desativar")]
        public async Task<ActionResult<ApiResponse<object>>> Desativar(int id)
        {
            try
            {
                await _permissaoWriterService.DesativarPermissaoAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse("Permissão desativada com sucesso"));
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

        [HttpPost("{id:int}/reativar")]
        public async Task<ActionResult<ApiResponse<object>>> Reativar(int id)
        {
            try
            {
                await _permissaoWriterService.ReativarPermissaoAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse("Permissão reativada com sucesso"));
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
    }
}
