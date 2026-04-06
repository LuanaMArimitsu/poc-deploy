using System.Security.Claims;
using WebsupplyConnect.Application.DTOs.Permissao;
using WebsupplyConnect.Application.DTOs.Permissao.Permissao;
using WebsupplyConnect.Application.DTOs.Permissao.Role;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Permissao;

namespace WebsupplyConnect.Application.Services.Permissao
{
    public class RoleReaderService(
        IRoleRepository roleRepository, IUnitOfWork unitOfWork) : IRoleReaderService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
       
        public async Task<RolePaginadoDTO> GetRoles(RoleFiltroDTO filtro)
        {
            try
            {
                var (roles, totalItens) = await _roleRepository.GetRolesAsync(filtro.Nome, filtro.EmpresaId, filtro.Contexto, filtro.Pagina, filtro.TamanhoPagina);

                var itens = roles.Select(x => new RoleDTO
                {
                    Id = x.Id,
                    Descricao = x.Descricao,
                    Nome = x.Nome,
                    Ativa = x.Ativa,
                    Contexto = x.Contexto,
                    EmpresaId = x.EmpresaId,
                    Empresa = x.Empresa != null ? x.Empresa.Nome : null,
                    IsSistema = x.IsSistema,
                    Permissoes = x.RolePermissoes.Select(x => new PermissaoDTO
                    {
                        Id = x.Permissao.Id,
                        Categoria = x.Permissao.Categoria,
                        Descricao = x.Permissao.Descricao,
                        Modulo = x.Permissao.Modulo,
                        Nome = x.Permissao.Nome,
                        IsCritica = x.Permissao.IsCritica,
                        Ativa = x.Permissao.Ativa
                    }).ToList(),
                    QntdUsuarios = x.ObterTotalUsuariosAtivos()
                }).ToList();

                var totalPaginas = (int)Math.Ceiling(totalItens / (double)filtro.TamanhoPagina);

                return new RolePaginadoDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtro.Pagina,
                    TotalPaginas = totalPaginas,
                    Itens = itens
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IReadOnlyList<RoleDTO>> GetRolesByUsuario(int usuarioId)
        {
            var roles = await _roleRepository.GetRolesByUsuarioAsync(usuarioId);

            var roleDtos = roles.Select(x => new RoleDTO
            {
                Id = x.Id,
                Descricao = x.Descricao,
                Nome = x.Nome,
                Ativa = x.Ativa,
                Contexto = x.Contexto,
                EmpresaId = x.EmpresaId,
                IsSistema = x.IsSistema,
                Permissoes = x.RolePermissoes.Select(x => new PermissaoDTO
                {
                    Id = x.Permissao.Id,
                    Categoria = x.Permissao.Categoria,
                    Descricao = x.Permissao.Descricao,
                    Modulo = x.Permissao.Modulo,
                    Nome = x.Permissao.Nome,
                    IsCritica = x.Permissao.IsCritica,
                    Ativa = x.Permissao.Ativa
                }).ToList(),
                QntdUsuarios = x.ObterTotalUsuariosAtivos()
            }).ToList();

            return roleDtos;
        }

        public async Task<IReadOnlyList<Domain.Entities.Permissao.Permissao>> GetPermissoesByRole(int roleId)
        {
            return await _roleRepository.GetPermissoesByRoleAsync(roleId);
        }

        public async Task<RoleDTO?> GetRoleByIdWithDetails(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync<Domain.Entities.Permissao.Role>(roleId, false);

            if (role == null)
                return null;

            var roleDto = new RoleDTO
            {
                Id = role.Id,
                Descricao = role.Descricao,
                Nome = role.Nome,
                Ativa = role.Ativa,
                Contexto = role.Contexto,
                EmpresaId = role.EmpresaId,
                IsSistema = role.IsSistema,
                Permissoes = role.RolePermissoes.Select(x => new PermissaoDTO
                {
                    Id = x.Permissao.Id,
                    Categoria = x.Permissao.Categoria,
                    Descricao = x.Permissao.Descricao,
                    Modulo = x.Permissao.Modulo,
                    Nome = x.Permissao.Nome,
                    IsCritica = x.Permissao.IsCritica,
                    Ativa = x.Permissao.Ativa
                }).ToList(),
                QntdUsuarios = role.ObterTotalUsuariosAtivos()
            };

            return roleDto;
        }

        public async Task<bool> UsuarioTemPermissaoAsync(int usuarioId, int? empresaId, string codigoPermissao)
        {
            if (string.IsNullOrWhiteSpace(codigoPermissao))
                throw new ArgumentException("Código de permissão não pode ser vazio", nameof(codigoPermissao));

            try
            {
                var possuiPermissao = await _roleRepository.PossuiRolePermissao(usuarioId, empresaId, codigoPermissao);

                return possuiPermissao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PermissaoEmpresasResult> EmpresasPermissaoAsync(int usuarioId, List<string> codigoPermissao)
        {
            if (codigoPermissao == null || codigoPermissao.Count == 0)
                throw new ArgumentException("Código de permissão não pode ser vazio", nameof(codigoPermissao));

            try
            {
                var permissao = await _roleRepository.ObterAlcancePermissaoUsuarioAsync(usuarioId, codigoPermissao);

                return new PermissaoEmpresasResult
                {
                    EmpresasIds = permissao.EmpresasIds,
                    AcessoGlobal = permissao.AcessoGlobal
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int ObterUsuarioId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var usuarioId) || usuarioId == 0)
                throw new UnauthorizedAccessException("Usuário não autenticado. UserId não encontrado ou inválido no token.");

            return usuarioId;
        }

        public async Task<List<UsuarioRoleDTO>> ListarUsuarioByRoleAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetRoleWithIncludes(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                var usuariosRole = await _roleRepository.ListarUsuarioByRoleAsync(roleId);
                var usuarios = usuariosRole.Select(ur => new UsuarioRoleDTO
                {
                    RoleId = ur.RoleId,
                    UsuarioId = ur.UsuarioId,
                    Nome = ur.Usuario.Nome,
                    DataAtribuicao = ur.DataAtribuicao,
                    DataExpiracao = ur.DataExpiracao,
                    Ativo = ur.Ativo,
                    AtribuidorId = ur.AtribuidorId,
                    AtribuidorNome = ur.Atribuidor.Nome,
                }).ToList();

                return usuarios;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
