using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensagemController(ILogger<MensagemController> logger, IMensagemReaderService mensagemReaderService, IMensagemWriterService mensagemWriterService) : ControllerBase
    {
        private readonly IMensagemReaderService _mensagemReaderService = mensagemReaderService ?? throw new ArgumentNullException(nameof(mensagemReaderService));
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService ?? throw new ArgumentNullException(nameof(mensagemWriterService));
        private readonly ILogger<MensagemController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("SincronizarMensagens")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<List<MensagemDTO>>>> GetMensagensRecentes(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] int conversaId,
            [FromQuery] int? quantidadeInicio,
            [FromQuery] int? quantidadeFim)
        {
            try
            {
                var mensagens = await _mensagemReaderService.GetMensagensRecentesAsync(conversaId, quantidadeInicio, quantidadeFim, dataInicio);
                return Ok(ApiResponse<List<MensagemDTO>>.SuccessResponse(mensagens, "Lista de sincronização de mensagem retornado com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao sincronizar mensagens.", ex.Message));
            }
        }

        [HttpGet("ObterMensagensAntigas")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<List<MensagemDTO>>>> GetMensagensAntigas(
            [FromQuery] DateTime dataLimite,
            [FromQuery] int conversaId,
            [FromQuery] int? pageSize = 30)
        {
            try
            {
                var mensagens = await _mensagemReaderService.GetMensagensAntigasAsync(dataLimite, conversaId, pageSize);
                return Ok(ApiResponse<List<MensagemDTO>>.SuccessResponse(mensagens, "Lista de mensagens antigas retornado com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao carregar mensagens antigas.", ex.Message));
            }
        }

        [HttpPost("SendMessagesToQueue")]
        [Authorize(Policy = "HorarioTrabalho")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> SendMessagesToQueue([FromForm] MensagemRequestDTO dto)
        {
            try
            {
                await _mensagemWriterService.ProcessarMensagemAsync(dto);

                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Mensagem criada e infilerada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar e enviar mensagem. Objeto de envio: {objeto}", dto);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }


        [HttpPost("SendBotMessagesToQueue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> SendBotMessagesToQueue([FromForm] MensagemRequestDTO dto)
        {
            try
            {
                await _mensagemWriterService.ProcessarMensagemAsync(dto);

                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Mensagem criada e infilerada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar e enviar mensagem. Objeto de envio: {objeto}", dto);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }


        [HttpGet("GetStatusMensagens")]
        [Authorize(Policy = "HorarioTrabalho")]
        [ProducesResponseType(typeof(List<MensagemStatus>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<MensagemStatus>>>> GetStatusMensagens()
        {
            try
            {
                List<MensagemStatus> statusMensagens = await _mensagemReaderService.GetListMensagemStatusAsync();

                return Ok(ApiResponse<List<MensagemStatus>>.SuccessResponse(statusMensagens, "Lista de status da mensagem retornado com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao retornar lista de status da mensagem.", ex.Message));
            }
        }

        [HttpGet("GetTipoMensagens")]
        [Authorize(Policy = "HorarioTrabalho")]
        [ProducesResponseType(typeof(List<MensagemTipo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<MensagemTipo>>>> GetTipoMensagens()
        {
            try
            {
                List<MensagemTipo> tipoMensagens = await _mensagemReaderService.GetListMensagemTiposAsync();

                return Ok(ApiResponse<List<MensagemTipo>>.SuccessResponse(tipoMensagens, "Lista de tipos de mensagem retornado com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao retornar lista de tipos de mensagem.", ex.Message));
            }
        }

        [HttpPost("MarcarMensagemComoLida")]
        [Authorize(Policy = "HorarioTrabalho")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> MarcarMensagemComoLida(int conversaId)
        {
            try
            {
                var mensagemSucesso = await _mensagemWriterService.MarcarMensagemComoLidaAsync(conversaId);

                return Ok(ApiResponse<object>.SuccessResponse(new { }, $"{mensagemSucesso}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao Marcar mensagens como lidas.", ex.Message));
            }
        }

    }
}