using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversaController(ILogger<ConversaController> logger, IConversaReaderService conversaReaderService, IConversaWriterService conversaWriterService, IRoleReaderService roleReaderService) : Controller
    {
        private readonly ILogger<ConversaController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService ?? throw new ArgumentNullException(nameof(conversaWriterService));
        private readonly IRoleReaderService _roleReaderService = roleReaderService;

        [HttpGet("status")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<List<ConversaStatus>>>> GetListConversaStatus()
        {
            try
            {
                var lista = await _conversaReaderService.GetListConversaStatus();

                if (lista == null)
                {
                    _logger.LogWarning("Erro ao buscar lista de status da conversa.");
                    return NotFound(ApiResponse<object>.ErrorResponse("Erro ao buscar lista de status da conversa."));
                }

                return Ok(ApiResponse<List<ConversaStatus>>.SuccessResponse(lista, "Lista de status da conversa retornada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao buscar lista de status da conversa.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }


        [HttpGet("SincronizarConversas/{usuarioId}")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<List<ConversaListaDTO>>>> SincronizarConversas(int usuarioId)
        {
            try
            {
                var usuariologadoId = _roleReaderService.ObterUsuarioId(User);
                var conversas = await _conversaWriterService.ConversasSyncAsync(usuarioId, usuariologadoId);
                return Ok(ApiResponse<List<ConversaListaDTO>>.SuccessResponse(conversas));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao sincroniza conversas. Detalhes: {detalhes}", ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao sincronizar conversas. Tente novamente mais tarde.", ex.ToString()));
            }
        }

        [HttpGet("SincronizarConversasPag/{usuarioId}")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<ConversaPag>>> SincronizarConversasPag(int usuarioId, [FromQuery] ConversaPagParam param)
        {
            try
            {
                var usuariologadoId = _roleReaderService.ObterUsuarioId(User);
                var conversas = await _conversaWriterService.GetConversasListaPaginadaAsync(usuarioId, usuariologadoId, param);
                return Ok(ApiResponse<ConversaPag>.SuccessResponse(conversas));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao sincroniza conversas. Detalhes: {detalhes}", ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao sincronizar conversas. Tente novamente mais tarde.", ex.ToString()));
            }
        }

        [HttpPatch("{conversaId}/status")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateConversaStatus(int conversaId, [FromBody] UpdateConversaDTO dto)
        {
            try
            {
                await _conversaWriterService.UpdateConversaStatus(new ConversaStatusDTO
                {
                    ConversaID = conversaId,
                    StatusId = dto.StatusId
                }, "commit");

                return Ok(ApiResponse<object>.SuccessResponse(new { }, "O status da conversa foi atualizado com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar status da conversa. Detalhes: {detalhes}", ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPatch("{conversaId}/encerrar")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<object>>> EncerrarConversa(int conversaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                await _conversaWriterService.EncerrarConversaAsync(conversaId, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "A conversa foi encerrada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao encerrar conversa. Detalhes: {detalhes}", ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPatch("{conversaId}/encerrarPeloBot")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> EncerrarConversaPeloBot(int conversaId)
        {
            try
            {
                await _conversaWriterService.EncerrarConversaAsync(conversaId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "A conversa foi encerrada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao encerrar conversa. Detalhes: {detalhes}", ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpGet("encerradas")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<ListConversasEncerradaDTO>>> ListarConversasEncerradas(int usuarioId, [FromQuery] ConversaPagParam param)
        {
            try
            {
                var conversas = await _conversaReaderService.ListConversasEncerradaAsync(usuarioId, param);

                return Ok(ApiResponse<ConversasEncerradasResultDTO>
                    .SuccessResponse(conversas, "Conversas encerradas retornadas com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar conversas encerradas. Usuário: {usuarioId}", usuarioId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar conversas encerradas.", ex.ToString()));
            }
        }


        [HttpGet("lead/{leadId}")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<TemConversaDTO>>> ConversaByLead(int leadId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                TemConversaDTO conversa = await _conversaWriterService.VerificarSeLeadTemConversaAtivaAsync(leadId, usuarioId);
                return Ok(ApiResponse<TemConversaDTO>.SuccessResponse(conversa));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao sincroniza conversas. Detalhes: {detalhes}", ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao sincronizar conversas. Tente novamente mais tarde.", ex.ToString()));
            }
        }

        [HttpGet("lead/{leadId}/encerradas")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<ConversasEncerradasLeadResultDTO>>> ListarConversasEncerradasPorLead(int leadId,
            [FromQuery] LeadConversaEncerradaParamsDTO param)
        {
            try
            {
                var conversas = await _conversaWriterService.ListConversasEncerradasByLeadAsync(leadId, param);

                return Ok(ApiResponse<ConversasEncerradasLeadResultDTO>
                    .SuccessResponse(conversas, "Conversas encerradas do lead retornadas com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar conversas encerradas do lead {leadId}", leadId);

                return StatusCode(500,
                    ApiResponse<object>.ErrorResponse(
                        "Erro interno ao listar conversas encerradas do lead.",
                        ex.ToString()));
            }
        }

        [HttpPatch("alternar-fixacao/{conversaId}")]
        [Authorize(Policy = "HorarioTrabalho")]
        public async Task<ActionResult<ApiResponse<object>>> AlternarFixacaoConversa(int conversaId)
        {
            try
            {
                var mensagem = await _conversaWriterService.AlterarFixacaoConversaAsync(conversaId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, mensagem));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar fixação da conversa {conversaId}. Detalhes: {detalhes}", conversaId, ex.Message);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }
    }
}
