using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Permissao.Role;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Entities.Permissao;

namespace WebsupplyConnect.API.Controllers.Permissao
{
    [Authorize(Policy = "HorarioTrabalho")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(IRoleReaderService roleReaderService, IRoleWriterService roleWriterService) : ControllerBase
    {
        private readonly IRoleReaderService _roleReaderService = roleReaderService ?? throw new ArgumentNullException(nameof(roleReaderService));
        private readonly IRoleWriterService _roleWriterService = roleWriterService ?? throw new ArgumentNullException(nameof(roleWriterService));

        [HttpPost("listar")]
        public async Task<ActionResult<ApiResponse<RolePaginadoDTO>>> GetRoles(RoleFiltroDTO filtro) 
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro?.EmpresaId, "ROLE_VISUALIZAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar roles nesta empresa.", "PERMISSAO_NEGADA"));

            var roles = await _roleReaderService.GetRoles(filtro);
            return Ok(ApiResponse<RolePaginadoDTO>.SuccessResponse(roles));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleDTO?>>> GetRoleById([FromRoute] int id, [FromQuery] int empresaId)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(id);
            if (role == null)
                return NotFound(ApiResponse<Role?>.ErrorResponse("Role não encontrada"));

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ROLE_VISUALIZAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar esta role nesta empresa.", "PERMISSAO_NEGADA"));

            return Ok(ApiResponse<RoleDTO?>.SuccessResponse(role));
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDTO>>>> GetRolesByUsuario([FromRoute] int usuarioId, [FromQuery] int empresaId)
        {
            var usuarioAtualId = _roleReaderService.ObterUsuarioId(User);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioAtualId, empresaId, "ROLE_VISUALIZAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar roles.", "PERMISSAO_NEGADA"));

            var roles = await _roleReaderService.GetRolesByUsuario(usuarioId);
            return Ok(ApiResponse<IReadOnlyList<RoleDTO>>.SuccessResponse(roles));
        }

        [HttpGet("{roleId}/permissoes")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<Domain.Entities.Permissao.Permissao>>>> GetPermissoesByRole([FromRoute] int roleId, [FromQuery] int empresaId)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ROLE_VISUALIZAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar permissões desta role.", "PERMISSAO_NEGADA"));

            var permissoes = await _roleReaderService.GetPermissoesByRole(roleId);
            return Ok(ApiResponse<IReadOnlyList<Domain.Entities.Permissao.Permissao>>.SuccessResponse(permissoes));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CriarRole([FromBody] CreateRoleDTO dto)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "ROLE_CRIAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para criar roles nesta empresa.", "PERMISSAO_NEGADA"));

            await _roleWriterService.CriarRoleAsync(dto);
            return CreatedAtAction(nameof(GetRoles), null, ApiResponse<object>.SuccessResponse("Role criada com sucesso"));
        }

        [HttpPut("{roleId}")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarRole(int roleId, [FromBody] UpdateRoleDTO dto)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "ROLE_EDITAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para editar esta role nesta empresa.", "PERMISSAO_NEGADA"));

            await _roleWriterService.AtualizarRoleAsync(roleId, usuarioId, dto);
            return Ok(ApiResponse<object>.SuccessResponse("Role atualizada com sucesso"));
        }

        [HttpDelete("{roleId}")]
        public async Task<ActionResult<ApiResponse<object>>> ExcluirRole([FromRoute] int roleId, [FromQuery] int empresaId)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ROLE_EXCLUIR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para excluir esta role nesta empresa.", "PERMISSAO_NEGADA"));

            await _roleWriterService.ExcluirRoleAsync(roleId);
            return Ok(ApiResponse<object>.SuccessResponse("Role excluída com sucesso"));
        }

        [HttpPost("{roleId}/permissoes/{permissaoId}")]
        public async Task<ActionResult<ApiResponse<object>>> AtribuirPermissaoARole(int roleId, int permissaoId, int concessorId, string? observacoes = null)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, role?.EmpresaId, "ROLE_EDITAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para editar permissões desta role.", "PERMISSAO_NEGADA"));

            await _roleWriterService.AtribuirPermissaoARoleAsync(roleId, permissaoId, concessorId, observacoes);
            return Ok(ApiResponse<object>.SuccessResponse("Permissão atribuída à role com sucesso"));
        }

        [HttpDelete("{roleId}/permissoes/{permissaoId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoverPermissaoDaRole([FromRoute] int roleId, [FromRoute] int permissaoId, [FromQuery] int empresaId)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ROLE_EDITAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para editar permissões desta role.", "PERMISSAO_NEGADA"));

            await _roleWriterService.RemoverPermissaoDaRoleAsync(roleId, permissaoId);
            return Ok(ApiResponse<object>.SuccessResponse("Permissão removida da role com sucesso"));
        }

        [HttpPost("{roleId}/usuarios/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<object>>> AssociarUsuarioARoleAsync([FromRoute] int roleId, [FromRoute] int usuarioId, [FromBody] AssociarUsuarioRoleDTO request)
        {
            var usuarioAtualId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioAtualId, request.EmpresaId, "ROLE_VINCULAR_USUARIO");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para vincular usuários a esta role.", "PERMISSAO_NEGADA"));

            await _roleWriterService.AssociarUsuarioARoleAsync(usuarioId, roleId, request.AtribuidorId, request.Observacoes);
            return Ok(ApiResponse<object>.SuccessResponse("Usuário associado à role com sucesso"));
        }

        [HttpDelete("{roleId}/usuarios/{usuarioId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoverUsuarioDaRoleAsync([FromRoute] int roleId, [FromRoute] int usuarioId, [FromQuery] int empresaId)
        {
            var usuarioAtualId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioAtualId, empresaId, "ROLE_VINCULAR_USUARIO");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para desvincular usuários desta role.", "PERMISSAO_NEGADA"));

            await _roleWriterService.RemoverUsuarioDaRoleAsync(usuarioId, roleId);
            return Ok(ApiResponse<object>.SuccessResponse("Usuário removido da role com sucesso"));
        }

        [HttpGet("{roleId}/usuarios")]
        public async Task<ActionResult<ApiResponse<List<UsuarioRoleDTO>>>> GetUsuariosByRole([FromRoute] int roleId, [FromQuery] int empresaId)
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var role = await _roleReaderService.GetRoleByIdWithDetails(roleId);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "ROLE_VISUALIZAR");
            if (!temPermissao)
                return BadRequest(ApiResponse<object>.ErrorResponse("Você não possui permissão para visualizar usuários desta role.", "PERMISSAO_NEGADA"));

            var usuarios = await _roleReaderService.ListarUsuarioByRoleAsync(roleId);
            return Ok(ApiResponse<List<UsuarioRoleDTO>>.SuccessResponse(usuarios));
        }
    }
}