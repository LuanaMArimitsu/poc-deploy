using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.VersaoApp;
using WebsupplyConnect.Application.Interfaces.VersaoApp;

namespace WebsupplyConnect.API.Controllers.VersaoApp
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersaoAppController(IVersaoAppReaderService versaoAppReaderService) : ControllerBase
    {
        private readonly IVersaoAppReaderService _versaoAppReaderService = versaoAppReaderService;

       [HttpGet("ultima-versao")]
       public async Task<ActionResult<ApiResponse<VersaoAppRetornoDTO>>> GetUltimaVersaoApp([FromQuery] string? plataformaApp)
       {
           try
           {
                var versaoApp = await _versaoAppReaderService.GetUltimaVersaoAppAsync(plataformaApp);

                return Ok(ApiResponse<VersaoAppRetornoDTO>.SuccessResponse(versaoApp));
            }
           catch (AppException ex)
           {
               return BadRequest(ApiResponse<VersaoAppRetornoDTO>.ErrorResponse(ex.Message, ex.ToString()));
           }
           catch (Exception ex)
           {
               return StatusCode(500, ApiResponse<VersaoAppRetornoDTO>.ErrorResponse("Erro interno", ex.ToString()));
           }
       }
    }
}
