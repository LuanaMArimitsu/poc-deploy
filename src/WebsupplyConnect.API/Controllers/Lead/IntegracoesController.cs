using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.OLX;
using WebsupplyConnect.Application.Interfaces.Lead;

namespace WebsupplyConnect.API.Controllers.Lead
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class IntegracoesController : ControllerBase
    {
        private readonly IOlxIntegracaoService _olxIntegracaoService;
        private readonly ILogger<IntegracoesController> _logger;

        public IntegracoesController(
            IOlxIntegracaoService olxIntegracaoService,
            ILogger<IntegracoesController> logger)
        {
            _olxIntegracaoService = olxIntegracaoService;
            _logger = logger;
        }

        [HttpPost("olx/{cnpjEmpresa}")]
        public async Task<ActionResult<OlxWebhookResponseDTO>> ReceberLeadViaOlx([FromRoute] string cnpjEmpresa)
        {
            try
            {
                string rawJson;

                //Ler diretamente de Request.Body e não da assinatura
                using (var reader = new StreamReader(Request.Body))
                {
                    rawJson = await reader.ReadToEndAsync();
                }

                var resultado = await _olxIntegracaoService.ReceberLeadOlxAsync(cnpjEmpresa, rawJson);

                return Ok(new OlxWebhookResponseDTO
                {
                    Success = true,
                    Message = "Lead recebido com sucesso",
                    ResponseId = resultado
                });
            }
            catch (AppException ex)
            {
                if (ex.Message.Contains("Token"))
                {
                    return Unauthorized(new OlxWebhookResponseDTO
                    {
                        Success = false,
                        Message = ex.Message
                    });
                }

                return BadRequest(new OlxWebhookResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new OlxWebhookResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
