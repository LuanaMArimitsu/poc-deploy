using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Lead;

namespace WebsupplyConnect.API.Controllers.Lead
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    public class EnderecoController : ControllerBase
    {
        private readonly IEnderecoWriterService _enderecoWriterService;

        public EnderecoController(IEnderecoWriterService enderecoWriterService)
        {
            _enderecoWriterService = enderecoWriterService;
        }

        [HttpPut("editar")]
        public async Task<ActionResult<ApiResponse<object>>> EditarEndereco([FromBody] EditarEnderecoDTO dto)
        {
            try
            {
                await _enderecoWriterService.EditarEnderecoAsync(dto);
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
    }
}