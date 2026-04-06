using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;

namespace WebsupplyConnect.API.Controllers.Comunicacao
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class SugestaoController(ILogger<SugestaoController> logger, IIaWriterService iaWriterService) : Controller
    {
        private readonly ILogger<SugestaoController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IIaWriterService _iaWriterService = iaWriterService ?? throw new ArgumentNullException(nameof(iaWriterService));
        /// <summary>
        /// Gera sugestões de resposta baseadas em uma conversa
        /// </summary>
        /// <param name="request">Dados da requisição contendo ID da conversa e rascunho opcional</param>
        /// <returns>Lista de sugestões de resposta</returns>
        [HttpPost("gerar")]
        public async Task<ActionResult<ApiResponse<List<MensagemSugestaoDTO>>>> GerarSugestoes([FromBody] SuggestionRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _iaWriterService.GerarSugestoesPorEmpresa(request);

                return Ok(ApiResponse<List<MensagemSugestaoDTO>>.SuccessResponse(response, "Lista de sugestões retornada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de sugestões");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        /// <summary>
        /// Gera Resumo da conversa
        /// </summary>
        /// <param name="request">Dados da requisição contendo ID da conversa</param>
        /// <returns>Resumo de toda conversa</returns>
        /// 
        [HttpPost("resumir")]
        public async Task<ActionResult<ApiResponse<ResumoIaResponseDTO>>> GerarResumo([FromBody] ResumoIaRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _iaWriterService.GerarResumoPorEmpresa(request);
                return Ok(ApiResponse<ResumoIaResponseDTO>.SuccessResponse(response!, "Resumo gerado com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de resumo");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [AllowAnonymous]
        [HttpPost("transcricao-audio")]
        public async Task<ActionResult<ApiResponse<TranscricaoResponseDTO>>> GerarTranscricao([FromBody] TranscricaoAudioRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _iaWriterService.GerarTranscricaoAudio(request);
                return Ok(ApiResponse<TranscricaoResponseDTO>.SuccessResponse(response!, "Transcrição gerada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de transcrição");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }
    }
}
