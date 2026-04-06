using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.API.Controllers.Equipe
{
    [ApiController]
    [Route("api/membros")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class MembroEquipeController(IMembroEquipeReaderService membroReader, IMembroEquipeWriterService membroWriter, IStatusMembroEquipeReadService statusRead, ILogger<MembroEquipeController> logger, IRoleReaderService roleReaderService) : ControllerBase
    {
        private readonly IMembroEquipeReaderService _membroReader = membroReader;
        private readonly IMembroEquipeWriterService _membroWriter = membroWriter;
        private readonly IStatusMembroEquipeReadService _statusRead = statusRead;
        private readonly ILogger<MembroEquipeController> _logger = logger;
        private readonly IRoleReaderService _roleReaderService = roleReaderService;

        /// <summary>Lista os tipos de status de MembroEquipe</summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<List<StatusMembroEquipeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<StatusMembroEquipeDto>>>> ListarStatus()
        {
            try
            {
                var dados = await _statusRead.ListarStatusFixoAsync();
                return Ok(ApiResponse<List<StatusMembroEquipeDto>>.SuccessResponse(dados));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar StatusMembroEquipe");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar status.", ex.ToString()));
            }
        }

        /// <summary>Adiciona membro em uma equipe.</summary>
        [HttpPost("equipe/{equipeId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> CreateMembro(int equipeId, int empresaId, [FromBody] AdicionarMembroDto dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "EQUIPE_MEMBRO_ADICIONAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para adicionar membros nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }
                var id = await _membroWriter.AddMembroAsync(equipeId, dto);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(new { id }, "Membro adicionado à equipe."));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar membro à equipe {EquipeId}", equipeId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao adicionar membro.", ex.ToString()));
            }
        }

        /// <summary>Lista membros de uma equipe com filtros (status, busca por nome) e paginação.</summary>
        [HttpPost("listar")]
        [ProducesResponseType(typeof(ApiResponse<MembrosEquipePaginadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MembrosEquipePaginadoDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<MembrosEquipePaginadoDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MembrosEquipePaginadoDto>>> Listar([FromBody] MembrosEquipeFiltroRequestDto filtro)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro.EmpresaId, "EQUIPE_VISUALIZAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para visualizar membros nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                var resultado = await _membroReader.ListarMembrosAsync(filtro);
                return Ok(ApiResponse<MembrosEquipePaginadoDto>.SuccessResponse(resultado));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<MembrosEquipePaginadoDto>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<MembrosEquipePaginadoDto>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar membros da equipe {EquipeId}", filtro?.EquipeId);
                return StatusCode(500, ApiResponse<MembrosEquipePaginadoDto>.ErrorResponse("Erro interno ao listar membros.", ex.ToString()));
            }
        }

        /// <summary>Atualiza o status do vínculo MembroEquipe.</summary>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarStatus([FromBody] AtualizarMembroEquipeDto dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "EQUIPE_MEMBRO_EDITAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para editar membros nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                var novo = await _membroWriter.AtualizarStatusAsync(dto);
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { membroId = dto.MembroId, statusMembroEquipeId = novo },
                    "Status do membro atualizado com sucesso."));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do membro {MembroId}", dto?.MembroId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao atualizar status do membro.", ex.ToString()));
            }
        }

        /// <summary>Alterar responsável de equipe</summary>
        [HttpPatch("lider/transferir")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> TransferirLideranca([FromBody] TransferirLiderancaRequestDto dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "EQUIPE_MEMBRO_EDITAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para transferir liderança nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                var (liderAnteriorId, novoLiderId) = await _membroWriter.TransferirLiderancaAsync(dto);
                var msg = liderAnteriorId is null ? "Liderança atribuída." : "Liderança transferida.";
                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    liderAnteriorMembroId = liderAnteriorId,
                    novoLiderMembroId = novoLiderId
                }, msg));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir liderança: {@Dto}", dto);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao transferir liderança.", ex.ToString()));
            }
        }

        /// <summary>Exclusão lógica de TODOS os membros ativos de uma equipe.</summary>
        [HttpDelete("equipe/{equipeId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTodosDaEquipe(int equipeId, int empresaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "EQUIPE_MEMBRO_EXCLUIR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para excluir membros nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }
                var removidos = await _membroWriter.DeleteTodosDaEquipeAsync(equipeId);
                var msg = removidos.quantidadeRemovidos > 0
                    ? $"{removidos} membro(s) removido(s) da equipe."
                    : "Nenhum membro ativo para remover.";
                return Ok(ApiResponse<object>.SuccessResponse(new { removidos }, msg));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover membros da equipe {EquipeId}", equipeId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao remover membros.", ex.ToString()));
            }
        }

        ///<summary>Exclusão lógica de UM membro específico da equipe.</summary>
        [HttpDelete("excluir/{membroId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMembro(int membroId, int empresaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "EQUIPE_MEMBRO_EXCLUIR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para excluir este membro nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _membroWriter.DeleteMembroAsync(membroId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Membro removido com sucesso."));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover membro {MembroId}", membroId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao remover membro.", ex.ToString()));
            }
        }
    }
}
