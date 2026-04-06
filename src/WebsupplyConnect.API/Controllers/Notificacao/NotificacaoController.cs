using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Notificacao;

namespace WebsupplyConnect.API.Controllers.Notificacao
{
    [Route("api/[controller]")]
    [ApiController]    
    public class NotificacaoController(INotificacaoWriterService notificacaoWriterService, INotificacaoReaderService notificacaoReaderService) : ControllerBase
    {
        private readonly INotificacaoWriterService _notificacaoWriterService = notificacaoWriterService;
        private readonly INotificacaoReaderService _notificacaoReaderService = notificacaoReaderService;

        [HttpPost("NovoLead")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> NovoLead(NotificarNovoLeadDTO dto)
        {
            try
            {
                await _notificacaoWriterService.NovoLead(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("NovoLeadVendedor")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> NovoLeadVendedor(NotificarNovoLeadVendedorDTO dto)
        {
            try
            {
                await _notificacaoWriterService.NovoLeadVendedor(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("LeadAtualizado")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> LeadAtualizado(NotificarNovoLeadDTO dto)
        {
            try
            {
                await _notificacaoWriterService.LeadAtualizado(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("NovaMensagem")] //notifica mensagem enviada pelo cliente
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> NovaMensagem(NotificarNovaMensagemDTO dto)
        {
            try
            {
                await _notificacaoWriterService.NovaMensagem(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("AtualizarMensagemStatus")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarMensagemStatus(NotificarStatusMensagemAtualizadoDTO dto)
        {
            try
            {
                await _notificacaoWriterService.MensagemAtualizarStatus(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex) 
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [Authorize(Policy = "HorarioTrabalho")]
        [HttpDelete("Excluir/{notificacaoId:int}/{usuarioId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> ExcluirNotificacao(int notificacaoId, int usuarioId)
        {
            if (notificacaoId <= 0 || usuarioId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("IDs devem ser maiores que zero."));
            }

            try
            {
                await _notificacaoWriterService.ExcluirNotificacaoAsync(notificacaoId, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Notificação excluída."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("SincronizarNotificacoes/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<List<NotificacaoListaDTO>>>> SincronizarNotificacoes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do usuário deve ser maior que zero."));
            }

            try
            {
                var notificacoes = await _notificacaoReaderService.NotificacoesSyncAsync(usuarioId);
                return Ok(ApiResponse<List<NotificacaoListaDTO>>.SuccessResponse(notificacoes));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("ExcluirTodasEMarcarComoLidas/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<NotificacaoLimpezaResultadoDTO>>> ExcluirTodasEMarcarComoLidas(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do usuário deve ser maior que zero."));
            }

            try
            {
                var resultado = await _notificacaoWriterService.ExcluirTodasEMarcarComoLidasAsync(usuarioId);
                var mensagem = resultado.Excluidas == 0
                    ? "Nenhuma notificação ativa para processar."
                    : "Notificações marcadas como lidas e excluídas.";
                return Ok(ApiResponse<NotificacaoLimpezaResultadoDTO>.SuccessResponse(resultado, mensagem));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("MarcarTodasComoLidas/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<object>>> MarcarTodasComoLidas(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do usuário deve ser maior que zero."));
            }

            try
            {
                await _notificacaoWriterService.MarcarTodasComoLidasAsync(usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Notificações marcadas como lidas."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("Pendentes/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<List<NotificacaoProcessadaDTO>>>> ProcessarPendentes(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do usuário deve ser maior que zero."));
            }

            try
            {
                var notificacoes = await _notificacaoWriterService.ProcessarNotificacoesCriadasAsync(usuarioId);

                if (notificacoes.Any())
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        new { notificacoesEnviadas = notificacoes.Count, detalhes = notificacoes },
                        "Notificações enviadas com sucesso"
                    ));
                }
                else
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        new { notificacoesEnviadas = 0 },
                        "Nenhuma notificação pendente"
                    ));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("EscalonamentoAutomaticoLider")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> EscalonamentoAutomaticoLider(NotificacaoEscalonamentoDTO dto)
        {
            try
            {
                await _notificacaoWriterService.EscalonamentoAutomaticoLider(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("EscalonamentoAutomaticoVendedor")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> EscalonamentoAutomaticoVendedor(NotificacaoEscalonamentoDTO dto)
        {
            try
            {
                await _notificacaoWriterService.EscalonamentoAutomaticoVendedor(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("LeadEvento")]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> NovoLeadEvento(NotificarNovoLeadDTO dto)
        {
            try
            {
                await _notificacaoWriterService.NovoLeadEvento(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }
    }
}
