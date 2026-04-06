using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Permissao;

namespace WebsupplyConnect.API.Controllers.Oportunidade
{
    [ApiController]
    [Authorize(Policy = "HorarioTrabalho")]
    [Route("api/[controller]")]
    public class OportunidadeController(IOportunidadeReaderService oportunidadeReaderService, IOportunidadeWriterService oportunidadeWriterService, IRoleReaderService roleReaderService, IValidator<CreateOportunidadeDTO> validator, ILogger<OportunidadeController> logger) : ControllerBase
    {
        private readonly ILogger<OportunidadeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOportunidadeReaderService _oportunidadeReaderService = oportunidadeReaderService ?? throw new ArgumentNullException(nameof(oportunidadeReaderService));
        private readonly IOportunidadeWriterService _oportunidadeWriterService = oportunidadeWriterService ?? throw new ArgumentNullException(nameof(oportunidadeWriterService));
        private readonly IRoleReaderService _roleReaderService = roleReaderService ?? throw new ArgumentNullException(nameof(roleReaderService));
        private readonly IValidator<CreateOportunidadeDTO> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        [HttpPost("criar")]
        public async Task<ActionResult<ApiResponse<object>>> CreateOportunidade([FromBody] CreateOportunidadeDTO dto)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "OPORTUNIDADE_CRIAR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para criar oportunidades nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }

            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
                return BadRequest(ApiResponse<object>.ErrorResponse(errors));
            }
            try
            {
                var oportunidade = await _oportunidadeWriterService.CreateOportunidadeAsync(dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Oportunidade criada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar e enviar mensagem. Objeto de envio: {objeto}", dto);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPost("listar")]
        public async Task<ActionResult<ApiResponse<OportunidadePaginadoDTO>>> GetOportunidades([FromBody] FilterOportunidadeDTO filtro)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                if (filtro?.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro?.EmpresaId, "OPORTUNIDADE_VISUALIZAR_TODAS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não possui permissão para visualizar todas as oportunidades nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro.EmpresaId, "OPORTUNIDADE_VISUALIZAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não possui permissão para visualizar suas oportunidades nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                var oportunidades = await _oportunidadeReaderService.GetOportunidadesAsync(filtro);
                return Ok(ApiResponse<object>.SuccessResponse(oportunidades));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar oportunidades.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }

        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<GetOportunidadeDTO>>> GetOportunidadeById(int id)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var oportunidade = await _oportunidadeReaderService.GetOportunidadeByIdDetalhadoAsync(id);

                if (oportunidade.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, oportunidade.EmpresaId, "OPORTUNIDADE_VISUALIZAR_TODAS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não possui permissão para visualizar todas as oportunidades nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, oportunidade.EmpresaId, "OPORTUNIDADE_VISUALIZAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não possui permissão para visualizar esta oportunidade nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                return Ok(ApiResponse<object>.SuccessResponse(oportunidade));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar oportunidade com ID {id}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPut("atualizar")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateOportunidade([FromBody] UpdateOportunidadeDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var oportunidade = await _oportunidadeReaderService.GetOportunidadeByIdAsync(dto.Id);

                if (dto?.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, oportunidade.EmpresaId, "OPORTUNIDADE_EDITAR_TODAS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não possui permissão para editar todas as oportunidades nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, oportunidade.EmpresaId, "OPORTUNIDADE_EDITAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não possui permissão para editar esta oportunidade nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                _oportunidadeWriterService.UpdateOportunidadeAsync(dto).Wait();
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Oportunidade atualizada com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar oportunidade. Objeto de envio: {objeto}", dto);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteOportunidade(int id, int empresaId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "OPORTUNIDADE_EXCLUIR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para excluir oportunidades nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _oportunidadeWriterService.DeleteOportunidadeAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Oportunidade excluída com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir oportunidade com ID {id}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPatch("{oportunidadeId:int}/mover-etapa")]
        public async Task<ActionResult<ApiResponse<object>>> MoverOportunidadeParaProximaEtapa(int oportunidadeId,[FromBody] ChangeEtapaDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var oportunidade = await _oportunidadeReaderService.GetOportunidadeByIdAsync(oportunidadeId);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, oportunidade.EmpresaId, "OPORTUNIDADE_MOVER_ETAPAS");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para mover etapas de oportunidades nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _oportunidadeWriterService.UpdateEtapaOpotunidade(oportunidadeId, dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Movimentação de etapa concluída com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover oportunidade com ID {id} para a próxima etapa.", oportunidadeId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpPatch("{oportunidadeId:int}/converter-gold")]
        public async Task<ActionResult<ApiResponse<object>>> ConverterOportunidadeGold(int oportunidadeId)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var oportunidade = await _oportunidadeReaderService.GetOportunidadeByIdAsync(oportunidadeId);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, oportunidade.EmpresaId, "OPORTUNIDADE_ENVIAR_GOLD");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não possui permissão para criar eventos de oportunidades nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _oportunidadeWriterService.EnviarParaIntegrador(oportunidadeId, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Evento criado para a oportunidade com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar evento para oportunidade com ID {id}.", oportunidadeId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpGet("tipos-interesse")]
        public async Task<ActionResult<ApiResponse<List<TipoInteresseDTO>>>> ListarTiposInteresse()
        {
            try
            {
                var tipos = await _oportunidadeReaderService.ListarTiposInteresseAsync();
                return Ok(ApiResponse<object>.SuccessResponse(tipos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar tipos de interesse.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao listar tipos de interesse.", ex.ToString()));
            }
        }

        [ApiKeyAuth]
        [AllowAnonymous]
        [HttpPost("conversao")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarConversao([FromBody] ConversaoOportunidadeDTO request)
        {
            try
            {
                await _oportunidadeWriterService.AtualizarConversaoAsync(request);

                return Ok(ApiResponse<object>
                    .SuccessResponse(null, "Conversão atualizada com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>
                    .ErrorResponse("Erro interno ao atualizar conversão.", ex.ToString()));
            }
        }
    }
}
