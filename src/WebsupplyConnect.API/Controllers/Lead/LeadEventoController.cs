using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.Evento;
using WebsupplyConnect.Application.DTOs.Lead.Historico;
using WebsupplyConnect.Application.Interfaces.Lead;

namespace WebsupplyConnect.API.Controllers.Lead
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    public class LeadEventoController(ILeadEventoWriterService leadEventoWriterService, ILeadEventoReaderService leadEventoReaderService) : ControllerBase
    {
        private readonly ILeadEventoWriterService _leadEventoWriterService = leadEventoWriterService ?? throw new ArgumentNullException(nameof(leadEventoWriterService));
        private readonly ILeadEventoReaderService _leadEventoReaderService = leadEventoReaderService ?? throw new ArgumentNullException(nameof(leadEventoReaderService));

        [HttpPost("eventoManual")]
        public async Task<ActionResult<ApiResponse<object>>> RegistrarEventoManual([FromBody] LeadEventoDTO dto)
        {
            try
            {
                await _leadEventoWriterService.RegistrarEventoManualAsync(dto);

                return Ok(ApiResponse<object>.SuccessResponse("Evento manual criado com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao criar evento manual.", ex.ToString()));
            }
        }

        [HttpGet("eventos")]
        public async Task<ActionResult<ApiResponse<LeadEventoResponseDTO>>> GetAllEventos()
        {
            try
            {
                var historicos = await _leadEventoReaderService.GetAllAsync();

                return Ok(ApiResponse<List<LeadEventoResponseDTO>>.SuccessResponse(
                    historicos,
                    "Eventos recuperados com sucesso."
                ));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao recuperar eventos.", ex.ToString()));
            }
        }

        [HttpGet("{leadId}/eventos")]
        public async Task<ActionResult<ApiResponse<LeadEventoResponseDTO>>> GetEventosByLeadId(int leadId)
        {
            try
            {
                var response = await _leadEventoReaderService.GetByLeadIdAsync(leadId);
                return Ok(ApiResponse<List<LeadEventoResponseDTO>>.SuccessResponse(response, "Eventos do lead recuperados com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao listar eventos do lead.", ex.Message));
            }
        }

        /// <summary>Edita um evento de lead.</summary>
        [HttpPatch("evento/{eventoId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateEvento(int eventoId, [FromBody] LeadEventoUpdateDTO dto)
        {
            try
            {
                await _leadEventoWriterService.UpdateEventoAsync(eventoId, dto);

                return Ok(ApiResponse<object>.SuccessResponse("Evento atualizado com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao atualizar evento.", ex.ToString()));
            }
        }

        [HttpPost("eventos/campanha")]
        [ProducesResponseType(typeof(ApiResponse<EventosPaginadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EventosPaginadoDto>>>ListarEventosPorCampanha([FromBody] ListEventoRequestDTO request)
        {
            try
            {
                var eventos = await _leadEventoReaderService.ListarEventosPorCampanhaAsync(request);

                return Ok(ApiResponse<EventosPaginadoDto>.SuccessResponse(
                    eventos,
                    "Eventos retornados com sucesso."
                ));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar eventos.", ex.ToString()));
            }
        }
    }
}
