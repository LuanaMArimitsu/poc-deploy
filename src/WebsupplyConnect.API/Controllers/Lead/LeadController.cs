using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Attributes;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Permissao;

namespace WebsupplyConnect.API.Controllers.Lead
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class LeadController(ILeadWriterService leadWriterService, ILeadReaderService leadReaderService, IRoleReaderService roleReaderService, ILeadExportService leadExportService, ILeadResponsavelWriterService leadResponsavelWriterService) : ControllerBase
    {
        private readonly ILeadWriterService _leadWriterService = leadWriterService ?? throw new ArgumentNullException(nameof(leadWriterService));
        private readonly ILeadReaderService _leadReaderService = leadReaderService ?? throw new ArgumentNullException(nameof(leadReaderService));
        private readonly IRoleReaderService _roleReaderService = roleReaderService ?? throw new ArgumentNullException(nameof(roleReaderService));
        private readonly ILeadExportService _leadExportService = leadExportService ?? throw new ArgumentNullException(nameof(leadExportService));
        private readonly ILeadResponsavelWriterService _leadResponsavelWriterService = leadResponsavelWriterService ?? throw new ArgumentNullException(nameof(leadResponsavelWriterService));

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> Post(LeadCompletoDTO request)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, request.EmpresaId, "LEAD_CRIAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não tem permissão para criar leads nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _leadWriterService.CreateAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var lead = await _leadReaderService.GetLeadByIdAsync(id);

                if (lead.Responsavel.UsuarioId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EXCLUIR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para excluir leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EXCLUIR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para excluir seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                await _leadWriterService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> AtualizarStatusAsync(int id, AtualizarStatusDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var lead = await _leadReaderService.GetLeadByIdAsync(id);
                if (dto?.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para atualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para atualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                await _leadWriterService.UpdateStatusAsync(id, dto.StatusId, dto.Observacao ?? string.Empty);
                return NoContent();
            }
            catch (AppException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao atualizar status do lead." });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, LeadUpdateDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var lead = await _leadReaderService.GetLeadByIdAsync(id);
                if (dto?.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para atualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para atualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                await _leadWriterService.UpdateAsync(id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LeadRetornoDTO>>> Get(int id)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                LeadRetornoDTO lead = await _leadReaderService.GetDetalhesAsync(id, usuarioId);

                if (lead.UsuarioId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_VISUALIZAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_VISUALIZAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                return Ok(ApiResponse<LeadRetornoDTO>.SuccessResponse(lead));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPatch("atualizar-endereco")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarEnderecoLead([FromBody] EnderecoLeadDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var lead = await _leadReaderService.GetLeadByIdAsync(dto.LeadId);
                if (dto?.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                await _leadWriterService.AtualizarEnderecoLeadAsync(dto.LeadId, dto.Endereco, dto.IsComercial);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPatch("remover-endereco")]
        public async Task<ActionResult<ApiResponse<object>>> RemoverEnderecoLead(RemoverEnderecoLeadDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var lead = await _leadReaderService.GetLeadByIdAsync(dto.LeadId);
                if (dto?.ResponsavelId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para atualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, lead.EmpresaId, "LEAD_EDITAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para atualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                await _leadWriterService.RemoverEnderecoLeadAsync(dto.LeadId, dto.EnderecoId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<List<StatusLeadDTO>>>> ListarStatusDoLeadAsync()
        {
            try
            {
                var status = await _leadReaderService.ListarStatusDoLeadAsync();
                return Ok(ApiResponse<List<StatusLeadDTO>>.SuccessResponse(status, "Status do lead retornados com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar status do lead.", ex.ToString()));
            }
        }

        [HttpPost("crawler")]
        [AllowAnonymous]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> PostFromCrawler(LeadCrawlerDTO request)
        {
            try
            {
                await _leadResponsavelWriterService.CriarLeadViaCrawler(request);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Lead criado via crawler."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (NotFoundAppException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        /// <summary>Exporta leads filtrados e envia o relatório por e-mail</summary>
        [HttpPost("exportar-email")]
        public async Task<IActionResult> ExportarLeadsEnviarPorEmail([FromBody] LeadExportEmailDTO dto)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var emailSolicitante = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var nomeSolicitante = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Usuário";

                if (string.IsNullOrWhiteSpace(emailSolicitante))
                    return BadRequest(ApiResponse<object>.ErrorResponse("E-mail do solicitante não encontrado no token."));

                if (dto.UsuarioId.HasValue && dto.UsuarioId.Value == usuarioId)
                {
                    // Exportando seus próprios leads
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(
                        usuarioId,
                        dto.EmpresaId,
                        "LEAD_VISUALIZAR"
                    );

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Para exportar seus leads nesta empresa é necessário a permissão de visualizar seus leads.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    // Exportando leads de outros usuários ou todos (sem passar usuarioId no payload)
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(
                        usuarioId,
                        dto.EmpresaId,
                        "LEAD_VISUALIZAR_TODOS"
                    );

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Para exportar leads de outros usuários é necessário a permissão de visualizar leads.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                await _leadExportService.ExportarLeadsEEnviarPorEmailAsync(
                    dto.EmpresaId,
                    emailSolicitante,
                    nomeSolicitante,
                    dto.EquipeId,
                    dto.UsuarioId,
                    dto.StatusId,
                    dto.De,
                    dto.Ate
                );

                return Ok(ApiResponse<object>.SuccessResponse(
                    $"Relatório enviado com sucesso para {emailSolicitante}."
                ));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao exportar e enviar Excel por e-mail",
                    ex.ToString()
                ));
            }
        }

        [HttpPost("listar")]
        public async Task<ActionResult<ApiResponse<LeadPaginadoDTO>>> ListarLeads([FromBody] LeadFiltroRequestDTO filtro)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                if (filtro?.UsuarioId != usuarioId)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro?.EmpresaId, "LEAD_VISUALIZAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro.EmpresaId, "LEAD_VISUALIZAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                var resultado = await _leadReaderService.ListarLeadsAsync(filtro, usuarioId);
                return Ok(ApiResponse<LeadPaginadoDTO>.SuccessResponse(resultado));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPost("listarNovo")]
        public async Task<ActionResult<ApiResponse<LeadPaginadoDTO>>> Listar([FromBody] LeadFiltrosDto filtro)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                if (filtro.MeusLeads == false || !filtro.MeusLeads.HasValue)
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro?.EmpresaId, "LEAD_VISUALIZAR_TODOS");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar leads de outros usuários nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }
                else
                {
                    var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro.EmpresaId, "LEAD_VISUALIZAR");

                    if (!temPermissao)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse(
                            "Você não tem permissão para visualizar seus leads nesta empresa.",
                            "PERMISSAO_NEGADA"
                        ));
                    }
                }

                var resultado = await _leadReaderService.ListarLeadsNovoAsync(filtro, usuarioId);   
                return Ok(ApiResponse<LeadPaginadoDTO>.SuccessResponse(resultado));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPost("listar-por-permissao")]
        public async Task<ActionResult<ApiResponse<LeadPaginadoDTO>>> ListarPorPermissao([FromBody] LeadFiltrosDto? filtro)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);
                var resultado = await _leadReaderService.ListarLeadsPorPermissaoAsync(filtro, usuarioId);
                return Ok(ApiResponse<LeadPaginadoDTO>.SuccessResponse(resultado));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPatch("lead-nome/{id}")]
        [AllowAnonymous]
        [ApiKeyAuth]
        public async Task<ActionResult<ApiResponse<object>>> AlterarNomeLeadIdAsync(int id, [FromBody] string novoNome)
        {
            try
            {
                await _leadWriterService.AlterarNomeLeadIdAsync(id, novoNome);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }

        [HttpPost("lead-rapido")]
        public async Task<ActionResult<ApiResponse<object>>> CreateLeadRapidoAsync(LeadRapidoDTO request)
        {
            try
            {
                var usuarioId = _roleReaderService.ObterUsuarioId(User);

                var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, request.EmpresaId, "LEAD_CRIAR");

                if (!temPermissao)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Você não tem permissão para criar leads nesta empresa.",
                        "PERMISSAO_NEGADA"
                    ));
                }

                await _leadWriterService.CreateLeadRapidoAsync(request, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse(new { }));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno", ex.ToString()));
            }
        }
    }
}
