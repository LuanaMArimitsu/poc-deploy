using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Permissao;

namespace WebsupplyConnect.API.Controllers.Distribuicao
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    public class RedistribuicaoController(ILogger<RedistribuicaoController> logger, IRedistribuicaoService redistribuicaoService, IRoleReaderService roleReaderService, IValidator<LeadRedistribuicaoDTO> validator) : Controller
    {
        private readonly IRedistribuicaoService _redistribuicaoService = redistribuicaoService;
        private readonly IValidator<LeadRedistribuicaoDTO> _validator = validator;
        private readonly IRoleReaderService _roleReaderService = roleReaderService ?? throw new ArgumentNullException(nameof(roleReaderService));
        private readonly ILogger<RedistribuicaoController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpPost("leads/{id}/transferencia")]
        public async Task<IActionResult> TransferirLeadAsync(int id, [FromBody] LeadRedistribuicaoDTO dto)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var usuarioId) || usuarioId == 0)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Usuário não autenticado.",
                    "UserId não encontrado ou inválido no token."
                ));
            }

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaID, "LEAD_TRANSFERIR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não tem permissão para transferir leads nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }

            var validationResult = _validator.Validate(dto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Dados inválidos para transferência de lead.", errors.ToString()));
            }

            try
            {
                await _redistribuicaoService.RedistribuirLeadAsync(id, dto.NovoResponsavelId, dto.EquipeId, dto.EmpresaID);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir lead {leadId} para usuário {novoResponsavelId}", id, dto.NovoResponsavelId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPatch("transferir-equipe-padrao/{leadId}")]
        [AllowAnonymous]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> TransferirLeadParaEquipePadraoAsync(int leadId, [FromBody] int empresaId)
        {
            try
            {
                await _redistribuicaoService.TransferirLeadParaEquipePadraoAsync(leadId, empresaId);

                return Ok(ApiResponse<object>.SuccessResponse("Lead transferido para o equipe padrão."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao transferir lead.", ex.ToString()));
            }
        }
    }
}
