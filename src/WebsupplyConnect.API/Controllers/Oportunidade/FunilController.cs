using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Oportunidade;

namespace WebsupplyConnect.API.Controllers.Oportunidade
{
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    [Route("api/[controller]")]
    public class FunilController(IFunilReaderService funilReaderService, ILogger<FunilController> logger) : Controller
    {
        private readonly IFunilReaderService _funilReaderService = funilReaderService ?? throw new ArgumentNullException(nameof(funilReaderService));
        private readonly ILogger<FunilController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("{empresaID:int}")]
        public async Task<ActionResult<ApiResponse<List<GetEtapasDTO>>>> GetListEtapaPerFunil(int empresaID)
        {
            try
            {
                var ListaEtapas = await _funilReaderService.GetFunilByEmpresa(empresaID);

                return Ok(ApiResponse<object>.SuccessResponse(ListaEtapas));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar lista de etapad com ID {id}.", empresaID);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }
    }
}
