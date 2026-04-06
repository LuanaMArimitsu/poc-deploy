using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    [ApiController]
    public class TemplateController(ILogger<TemplateController> logger, ITemplateReaderService templateReaderService) : Controller
    {
        private readonly ILogger<TemplateController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITemplateReaderService _templateReaderService = templateReaderService ?? throw new ArgumentNullException(nameof(templateReaderService));

        [HttpGet("GetListTemplate/")]
        public async Task<ActionResult<List<ListaTemplatesReponseDTO>>> GetListConversaStatus(int usuarioID, int empresaId)
        {
            try
            {
                var lista = await _templateReaderService.GetListTemplates(usuarioID, empresaId);

                string mensagem = "Lista de templates retornada com sucesso.";

                if (lista == null || lista.Count == 0)
                {
                   mensagem = $"Nenhum template foi encontrado para esse usuário.";
                }
                return Ok(ApiResponse<List<ListaTemplatesReponseDTO>>.SuccessResponse(lista ?? [], mensagem));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar templates para o canal com usuário id {usuarioID} e empresa id {empresaId}", usuarioID, empresaId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

    }
}
