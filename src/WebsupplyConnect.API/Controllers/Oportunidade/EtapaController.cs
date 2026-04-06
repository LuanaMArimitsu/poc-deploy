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
    public class EtapaController(ILogger<EtapaController> logger, IEtapaReaderService etapaReaderService) : Controller
    {
        private readonly ILogger<EtapaController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEtapaReaderService _etapaReaderService = etapaReaderService ?? throw new ArgumentNullException(nameof(etapaReaderService));

        [HttpGet("{oportunidadeId:int}")]
        public async Task<ActionResult<ApiResponse<List<EtapaHistoricoListDTO>>>> GetListEtapaHistorico(int oportunidadeId)
        {
            try
            {
                var listaHistorico = await _etapaReaderService.GetListEtapaHistorico(oportunidadeId);
                string mensagem = $"Histórico de etapas da oportunidade {oportunidadeId} recuperado com sucesso.";
                if (listaHistorico.Count == 0)
                {
                    mensagem = "Oportunidade não possui histórico de etapas.";
                }
                return Ok(ApiResponse<List<EtapaHistoricoListDTO>>.SuccessResponse(listaHistorico, mensagem));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar histórico de etapas da oportunidade com ID {id}.", oportunidadeId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }
    }
}