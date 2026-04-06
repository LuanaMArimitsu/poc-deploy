
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Interfaces.Comunicacao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [ApiController]
    [Route("api/meta")]
    public class WebhookController(IWebhookReaderService webhookReaderValidator, IWebhookWriterService webhookWriterService, ILogger<WebhookController> logger, IConfiguration configuration) : ControllerBase
    {
        private readonly IWebhookReaderService _webhookReaderValidator = webhookReaderValidator;
        private readonly IWebhookWriterService _webhookWriterService = webhookWriterService;
        private readonly ILogger<WebhookController> _logger = logger;
        private readonly string _verifyToken = configuration["WhatsApp:VerifyToken"]!;

        [AllowAnonymous]
        [HttpPost]

        /// <summary>
        /// Endpoint de recebimento de eventos do Webhook (POST)
        /// </summary>
        public async Task<IActionResult> ProcessWebhook()
        {
            try
            {
                // 1. Ler o payload do webhook
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Webhook recebido sem assinatura.");
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Assinatura ausente."));
                }

                var result = await _webhookReaderValidator.IsValid(payload, signature);   
                if (!result.IsValid)
                {
                    _logger.LogWarning("Webhook recebido com assinatura inválida.");
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Assinatura inválida."));
                }

                await _webhookWriterService.ProcessWebhookAsync(payload, result.Assinatura!);

                // 5. Responder imediatamente com 200 OK
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook da Meta");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao processar o webhook.", ex.StackTrace));
            }
        }

        /// <summary>
        /// Endpoint de verificação do Webhook (GET) utilizado pela Meta.
        /// </summary>        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            try
            {
                if (mode == "subscribe" && token == _verifyToken)
                {
                    _logger.LogInformation("Verificação do webhook realizada com sucesso.");
                    return Ok(challenge);
                }

                _logger.LogWarning("Falha na verificação do webhook.");
                return Unauthorized(ApiResponse<string>.ErrorResponse("Verificação falhou."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na verificação do webhook.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno na verificação do webhook."));
            }
        }
    }
}
