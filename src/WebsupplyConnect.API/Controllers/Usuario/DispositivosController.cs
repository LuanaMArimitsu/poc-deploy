using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Usuario;

namespace WebsupplyConnect.API.Controllers.Usuario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class DispositivosController : ControllerBase
    {
        private readonly IDispositivosWriterService _dispositivoWriterService;
        private readonly IDispositivosReaderService _dispositivoReaderService;
        private readonly ILogger<DispositivosController> _logger;

        public DispositivosController(IDispositivosWriterService dispositivoWriterService, IDispositivosReaderService dispositivosReaderService ,ILogger<DispositivosController> logger)
        {
            _dispositivoWriterService = dispositivoWriterService ?? throw new ArgumentNullException(nameof(dispositivoWriterService));
            _dispositivoReaderService = dispositivosReaderService ?? throw new ArgumentNullException(nameof(dispositivosReaderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponseDTO<DispositivoListagemDTO>>>> ListarDispositivos([FromQuery] DispositivoFiltroRequestDTO filtro)
        {
            try
            {
                var resultado = await _dispositivoReaderService.ListarDispositivosPaginadoAsync(filtro);

                return Ok(ApiResponse<PagedResponseDTO<DispositivoListagemDTO>>.SuccessResponse(
                    resultado, "Dispositivos listados com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar dispositivos com os filtros fornecidos.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar dispositivos. Tente novamente mais tarde."));
            }
        }

        [HttpGet("{deviceId}")]
        public async Task<ActionResult<ApiResponse<DispositivoDetalheDTO>>> ObterPorIdAsync(string deviceId)
        {
            try
            {
                var dispositivo = await _dispositivoReaderService.ObterDispositivoDetalhadoAsync(deviceId);

                if (dispositivo == null)
                    return NotFound(ApiResponse<DispositivoDetalheDTO>.ErrorResponse("Dispositivo não encontrado."));

                return Ok(ApiResponse<DispositivoDetalheDTO>.SuccessResponse(dispositivo, "Detalhes do dispositivo obtidos com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do dispositivo com ID {DispositivoId}.", deviceId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar detalhes do dispositivo. Tente novamente mais tarde."));
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> AlterarStatusDispositivo(int id, [FromBody] AlterarStatusDispositivoRequestDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Usuário não autenticado."));

                int usuarioLogadoId = int.Parse(userIdClaim.Value);

                await _dispositivoWriterService.AlterarStatusDispositivoAsync(id, request);

                var mensagem = request.Ativo
                    ? "Dispositivo desbloqueado com sucesso."
                    : "Dispositivo bloqueado com sucesso.";

                return Ok(ApiResponse<string>.SuccessResponse(mensagem));
            }
            catch (AppException ex)
            {
                if (ex.Message == "Dispositivo não encontrado.")
                    return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo não encontrado."));

                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status do dispositivo com ID {Id}", id);
                return BadRequest(ApiResponse<string>.ErrorResponse("Erro interno ao alterar status do dispositivo."));
            }
        }

        [HttpPut("{deviceId}/signalr-connection")]
        public async Task<ActionResult<ApiResponse<string>>> AtualizarConexaoSignalR(string deviceId, [FromBody] AtualizarConexaoSignalRRequestDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Usuário não autenticado."));

                var userId = int.Parse(userIdClaim.Value);

                var possuiDispositivo = await _dispositivoReaderService.UsuarioPossuiDispositivoAsync(userId, deviceId);
                if (!possuiDispositivo)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<string>.ErrorResponse("Usuário não tem permissão para atualizar este dispositivo."));

                var resultado = await _dispositivoWriterService.AtualizarConexaoSignalRAsync(deviceId, userId, request.ConnectionId);
                if (!resultado)
                    return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo não encontrado."));

                return Ok(ApiResponse<string>.SuccessResponse("Conexão SignalR atualizada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar conexão SignalR do dispositivo {DispositivoId}", deviceId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao atualizar conexão SignalR.", ex.Message));
            }
        }

        [HttpDelete("{deviceId}/signalr-desconnect")]
        public async Task<ActionResult<ApiResponse<string>>> DesconectarSignalR(string deviceId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Usuário não autenticado."));

                var userId = int.Parse(userIdClaim.Value);

                var possuiDispositivo = await _dispositivoReaderService.UsuarioPossuiDispositivoAsync(userId, deviceId);
                if (!possuiDispositivo)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<string>.ErrorResponse("Usuário não tem permissão para desconectar este dispositivo."));

                var sucesso = await _dispositivoWriterService.LimparConexaosignalRAsync(deviceId, userId);
                if (!sucesso)
                    return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo não encontrado."));

                return Ok(ApiResponse<string>.SuccessResponse("Conexão SignalR removida com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desconectar SignalR do dispositivo com ID {DispositivoId}", deviceId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao desconectar SignalR.", ex.Message));
            }
        }


        [HttpPost("{deviceId}/heartbeat")]
        public async Task<ActionResult<ApiResponse<string>>> RegistrarHeartbeat(string deviceId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Usuário não autenticado."));

                int usuarioLogadoId = int.Parse(userIdClaim.Value);

                var sucesso = await _dispositivoWriterService.RegistrarHeartbeatAsync(deviceId, usuarioLogadoId);
                if (!sucesso)
                    return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo não encontrado."));

                return NoContent();
            }
            catch (AppException ex)
            {
                if (ex.Message.Contains("permissão"))
                    return StatusCode(403, ApiResponse<string>.ErrorResponse("Você não tem permissão para registrar heartbeat neste dispositivo."));

                if (ex.Message.Contains("inativo"))
                    return BadRequest(ApiResponse<string>.ErrorResponse("Dispositivo inativo. Não é possível registrar heartbeat."));

                return BadRequest(ApiResponse<string>.ErrorResponse("Erro ao registrar heartbeat.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar heartbeat para o dispositivo {DispositivoId}", deviceId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Erro interno ao registrar heartbeat.", ex.Message));
            }
        }

        [HttpPost("{deviceId}/sincronizacao")]
        public async Task<ActionResult<ApiResponse<SincronizacaoDispositivoDTO>>> RegistrarSincronizacao(string deviceId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(ApiResponse<SincronizacaoDispositivoDTO>.ErrorResponse("Usuário não autenticado."));

                int usuarioLogadoId = int.Parse(userIdClaim.Value);

                var resultado = await _dispositivoWriterService.RegistrarSincronizacaoAsync(deviceId, usuarioLogadoId);
                if (resultado == null)
                    return NotFound(ApiResponse<SincronizacaoDispositivoDTO>.ErrorResponse("Dispositivo não encontrado."));

                return Ok(ApiResponse<SincronizacaoDispositivoDTO>.SuccessResponse(resultado, "Sincronização registrada com sucesso."));
            }
            catch (AppException ex)
            {
                if (ex.Message.Contains("permissão"))
                    return StatusCode(403, ApiResponse<SincronizacaoDispositivoDTO>.ErrorResponse("Você não tem permissão para registrar sincronização neste dispositivo."));

                if (ex.Message.Contains("inativo"))
                    return BadRequest(ApiResponse<SincronizacaoDispositivoDTO>.ErrorResponse("Dispositivo inativo. Não é possível registrar sincronização."));

                return BadRequest(ApiResponse<SincronizacaoDispositivoDTO>.ErrorResponse("Erro ao registrar sincronização.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar sincronização para o dispositivo {DispositivoId}", deviceId);
                return StatusCode(500, ApiResponse<SincronizacaoDispositivoDTO>.ErrorResponse("Erro interno ao registrar sincronização.", ex.Message));
            }
        }


        [HttpGet("verificar/{dispositivoId}")]
        public async Task<ActionResult<ApiResponse<DispositivoAcessoDTO>>> VerificarDispositivoStatus(int dispositivoId)
        {
            try
            {
                var resultado = await _dispositivoReaderService.VerificarDispositivoStatusAsync(dispositivoId);
                if (resultado == null)
                    return NotFound(ApiResponse<DispositivoAcessoDTO>.ErrorResponse("Dispositivo não encontrado."));
                return Ok(ApiResponse<DispositivoAcessoDTO>.SuccessResponse(resultado, "Status do dispositivo verificado com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status do dispositivo com ID {DispositivoId}", dispositivoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao verificar status do dispositivo.", ex.Message));
            }
        }
    }
}
