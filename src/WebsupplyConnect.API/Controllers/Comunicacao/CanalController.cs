using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    public class CanalController(ICanalWriterService canalWriterService, ICanalReaderService canalReadService) : ControllerBase
    {
        private readonly ICanalWriterService _canalWriterService = canalWriterService;
        private readonly ICanalReaderService _canalReadService = canalReadService;

        [HttpGet("List")]
        public async Task<ActionResult<ApiResponse<List<Canal>>>> List()
        {
            try
            {
                var canais = await _canalReadService.List();
                return Ok(ApiResponse<List<Canal>>.SuccessResponse(canais, "Canais listados com sucesso."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<Canal>>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("AddCanal")]
        public async Task<ActionResult<ApiResponse<object>>> AddCanal(CreateCanalDTO dto)
        {
            try
            {
                await _canalWriterService.Create(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Canal adicionado com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

    }
}
