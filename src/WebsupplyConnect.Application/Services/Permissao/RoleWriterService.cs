using WebsupplyConnect.Application.DTOs.Permissao.Role;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Permissao;
using Role = WebsupplyConnect.Domain.Entities.Permissao.Role;

namespace WebsupplyConnect.Application.Services.Permissao
{
    public class RoleWriterService(IRoleRepository roleRepository, IUnitOfWork unitOfWork) : IRoleWriterService
    {
        private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        /// <summary>
        /// Cria uma nova role no sistema.
        /// </summary>
        /// <param name="nome">Nome da role</param>
        /// <param name="descricao">Descrição detalhada</param>
        /// <param name="empresaId">ID da empresa (null para roles globais)</param>
        /// <param name="nivel">Nível hierárquico</param>
        /// <param name="contexto">Contexto (GLOBAL ou EMPRESA)</param>
        /// <param name="isSistema">Se é role de sistema</param>
        /// <returns>Role criada</returns>
        public async Task CriarRoleAsync(CreateRoleDTO dto)
        {
            try
            {
                var rolesExistentes = await _roleRepository.GetByPredicateAsync<Role>(r => r.Nome == dto.Nome && r.EmpresaId == dto.EmpresaId && !r.Excluido);
                if (rolesExistentes != null)
                    throw new InvalidOperationException("Já existe uma role com este nome para esta empresa/contexto.");

                var role = new Role(
                    dto.Nome,
                    dto.Descricao,
                    dto.EmpresaId,
                    dto.Contexto,
                    false
                );

                var roleCriada = await _roleRepository.CreateAsync(role);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao criar role: " + ex.Message);
            }
        }

        /// <summary>
        /// Exclui logicamente uma role pelo ID.
        /// </summary>
        /// <param name="roleId">ID da role</param>
        public async Task ExcluirRoleAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync<Role>(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                role.ExcluirLogicamente();
                _roleRepository.Update(role);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao excluir role: " + ex.Message);
            }
        }

        /// <summary>
        /// Atualiza os dados de uma role existente.
        /// </summary>
        /// <param name="roleId">ID da role</param>
        /// <param name="nome">Novo nome</param>
        /// <param name="descricao">Nova descrição</param>
        /// <param name="nivel">Novo nível hierárquico</param>
        public async Task AtualizarRoleAsync(int roleId, int usuarioId, UpdateRoleDTO dto)
        {
            try
            {
                var role = await _roleRepository.GetRoleWithIncludes(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                if (role.IsSistema && dto.Ativa == false)
                    throw new InvalidOperationException("Roles de sistema não podem ser desativadas.");

                role.AtualizarInformacoes(dto.Nome, dto.Descricao, dto.Ativa);
                _roleRepository.Update(role);

                var permissoesAtuais = role.RolePermissoes.Select(rp => rp.PermissaoId).ToList();
                var permissoesDto = dto.Permissoes?.ToList() ?? new List<int>();

                // Permissões a adicionar
                var permissoesParaAdicionar = permissoesDto.Except(permissoesAtuais).ToList();
                foreach (var permissaoId in permissoesParaAdicionar)
                {
                    await AtribuirPermissaoARoleAsync(roleId, permissaoId, usuarioId);
                }

                // Permissões a remover
                var permissoesParaRemover = permissoesAtuais.Except(permissoesDto).ToList();
                foreach (var permissaoId in permissoesParaRemover)
                {
                    await RemoverPermissaoDaRoleAsync(roleId, permissaoId);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao atualizar role: " + ex.Message);
            }
        }

        /// <summary>
        /// Atribui uma permissão a uma role.
        /// </summary>
        /// <param name="roleId">ID da role</param>
        /// <param name="permissaoId">ID da permissão</param>
        /// <param name="concessorId">ID do usuário que está concedendo</param>
        /// <param name="observacoes">Observações (opcional)</param>
        public async Task AtribuirPermissaoARoleAsync(int roleId, int permissaoId, int concessorId, string? observacoes = null)
        {
            try
            {
                var role = await _roleRepository.GetRoleWithIncludes(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                if (role.JaPossuiPermissao(permissaoId))
                    throw new InvalidOperationException("A role já possui esta permissão.");

                var permissao = await _roleRepository.GetByIdAsync<Domain.Entities.Permissao.Permissao>(permissaoId);
                if (permissao == null)
                    throw new InvalidOperationException("Permissão não encontrada.");

                var rolePermissao = await CriarRolePermissao(roleId, permissaoId, concessorId, observacoes);

                await _unitOfWork.CommitAsync();
            }
            catch 
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Remove uma permissão de uma role.
        /// </summary>
        /// <param name="roleId">ID da role</param>
        /// <param name="permissaoId">ID da permissão</param>
        public async Task RemoverPermissaoDaRoleAsync(int roleId, int permissaoId)
        {
            try
            {
                var role = await _roleRepository.GetRoleWithIncludes(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                if (!role.JaPossuiPermissao(permissaoId))
                    throw new InvalidOperationException("A role não possui esta permissão.");

                role.RemoverPermissao(permissaoId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao remover permissão da role: " + ex.Message);
            }
        }

        private async Task<RolePermissao> CriarRolePermissao(int roleId, int permissaoId, int concessorId, string? observacoes = null)
        {
            var rolePermissao = new RolePermissao(roleId, permissaoId, concessorId, observacoes);
            await _roleRepository.CreateAsync(rolePermissao);
            await _unitOfWork.SaveChangesAsync();
            return rolePermissao;
        }

        /// <summary>
        /// Associar um usuário a uma role específica.
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="roleId">ID da role</param>
        /// <param name="atribuidorId">ID do usuário que está atribuindo a role</param>
        /// <param name="observacoes">Observações (opcional)</param>
        public async Task AssociarUsuarioARoleAsync(int usuarioId, int roleId, int atribuidorId, string? observacoes = null)
        {
            try
            {
                var role = await _roleRepository.GetRoleWithIncludes(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                // Verifique se o usuário já está associado à role
                var usuarioRoleExistente = role.UsuarioRoles?.FirstOrDefault(ur => ur.UsuarioId == usuarioId && ur.RoleId == roleId);
                if (usuarioRoleExistente != null)
                    throw new InvalidOperationException("Usuário já está associado a esta role.");

                // Crie a associação
                var usuarioRole = await CriarUsuarioRole(usuarioId, roleId, atribuidorId, observacoes);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao associar usuário à role: " + ex.Message);
            }
        }
        private async Task<UsuarioRole> CriarUsuarioRole(int usuarioId, int roleId, int atribuidorId, string? observacoes = null)
        {
            var usuarioRole = new UsuarioRole(usuarioId, roleId, atribuidorId, observacoes);
            await _roleRepository.CreateAsync(usuarioRole);
            await _unitOfWork.SaveChangesAsync();
            return usuarioRole;
        }

        /// <summary>
        /// Remove a associação de um usuário a uma role.
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="roleId">ID da role</param>
        public async Task RemoverUsuarioDaRoleAsync(int usuarioId, int roleId)
        {
            try
            {
                var role = await _roleRepository.GetRoleWithIncludes(roleId);
                if (role == null)
                    throw new InvalidOperationException("Role não encontrada.");

                var usuarioRole = role.UsuarioRoles?.FirstOrDefault(ur => ur.UsuarioId == usuarioId && ur.RoleId == roleId);
                if (usuarioRole == null)
                    throw new InvalidOperationException("Usuário não está associado a esta role.");

                await _unitOfWork.BeginTransactionAsync();
                _roleRepository.Remove(usuarioRole);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
