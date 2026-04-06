using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Usuario;

namespace WebsupplyConnect.API.Controllers.Usuario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
        {
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [AllowAnonymous]
        [HttpPost("generate-jwt")]
        public async Task<IActionResult> GenerateJwt([FromBody] GenerateJwtRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Requisição inválida para geração de JWT.");
                return BadRequest(ApiResponse<string>.ErrorResponse("Dados inválidos."));
            }

            try
            {
                var result = await _jwtTokenService.GerarJwtAsync(request);
                return Ok(ApiResponse<GenerateJwtResponseDTO>.SuccessResponse(result, "Token gerado com sucesso."));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Tentativa não autorizada para gerar JWT.");
                return Unauthorized(ApiResponse<string>.ErrorResponse("Não autorizado.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao gerar JWT.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao gerar o token. Tente novamente mais tarde."));
            }
        }

        [HttpPost("renew-jwt")]
        public async Task<IActionResult> RenovarJwt([FromBody] RenewJwtRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Requisição inválida para renovação de JWT.");
                return BadRequest(ApiResponse<string>.ErrorResponse("Dados inválidos."));
            }

            try
            {
                var clientType = string.IsNullOrWhiteSpace(request.ClientType) ? "web" : request.ClientType;
                var result = await _jwtTokenService.RenovarJwtAsync(request.RefreshToken, User, clientType);
                return Ok(ApiResponse<GenerateJwtResponseDTO>.SuccessResponse(result, "Token renovado com sucesso."));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Tentativa não autorizada para renovar JWT.");
                return Unauthorized(ApiResponse<string>.ErrorResponse("Não autorizado.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao renovar JWT.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao renovar o token. Tente novamente mais tarde."));
            }
        }
    }
}
