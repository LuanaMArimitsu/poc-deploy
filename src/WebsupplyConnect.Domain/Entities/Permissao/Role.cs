using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Permissao
{
    /// <summary>
    /// Entidade que representa uma role (papel) auto-suficiente no sistema
    /// </summary>
    public class Role : EntidadeBase
    {
        /// <summary>
        /// Nome da role (ex: Vendedor, Gerente de Vendas)
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada da role
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// ID da empresa (null para roles globais)
        /// </summary>
        public int? EmpresaId { get; private set; }

        /// <summary>
        /// Contexto da role (GLOBAL ou EMPRESA)
        /// </summary>
        public string Contexto { get; private set; }

        /// <summary>
        /// Indica se é uma role de sistema (não pode ser excluída)
        /// </summary>
        public bool IsSistema { get; private set; }

        /// <summary>
        /// Indica se a role está ativa no sistema
        /// </summary>
        public bool Ativa { get; private set; }

        public virtual Empresa.Empresa Empresa { get; private set; }

        /// <summary>
        /// Coleção de associações entre esta role e permissões
        /// </summary>
        public virtual ICollection<RolePermissao> RolePermissoes { get; private set; }

        /// <summary>
        /// Coleção de associações entre usuários e esta role
        /// </summary>
        public virtual ICollection<UsuarioRole> UsuarioRoles { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Role()
        {
            RolePermissoes = new HashSet<RolePermissao>();
            UsuarioRoles = new HashSet<UsuarioRole>();
        }

        /// <summary>
        /// Construtor para criar uma nova role
        /// </summary>
        /// <param name="nome">Nome da role</param>
        /// <param name="descricao">Descrição detalhada</param>
        /// <param name="empresaId">ID da empresa (null para roles globais)</param>
        /// <param name="nivel">Nível hierárquico</param>
        /// <param name="contexto">Contexto (GLOBAL ou EMPRESA)</param>
        /// <param name="isSistema">Se é role de sistema</param>
        public Role(
            string nome,
            string descricao,
            int? empresaId,
            string contexto,
            bool isSistema = false)
        {
            ValidarDominio(nome, descricao, empresaId, contexto);

            Nome = nome;
            Descricao = descricao;
            EmpresaId = empresaId;
            Contexto = contexto.ToUpper();
            IsSistema = isSistema;
            Ativa = true;

            RolePermissoes = new HashSet<RolePermissao>();
            UsuarioRoles = new HashSet<UsuarioRole>();
        }

        /// <summary>
        /// Atualiza as informações básicas da role
        /// </summary>
        /// <param name="nome">Nome da role</param>
        /// <param name="descricao">Descrição detalhada</param>
        public void AtualizarInformacoes(string nome, string descricao, bool ativa)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da role é obrigatório.", nameof(Role));

            if (nome.Length > 100)
                throw new DomainException("O nome da role não pode ter mais que 100 caracteres.", nameof(Role));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição da role é obrigatória.", nameof(Role));

            if (descricao.Length > 500)
                throw new DomainException("A descrição da role não pode ter mais que 500 caracteres.", nameof(Role));

            Nome = nome;
            Descricao = descricao;
            Ativa = ativa;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Marca a role como role de sistema
        /// </summary>
        public void MarcarComoSistema()
        {
            IsSistema = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove a marcação de role de sistema
        /// </summary>
        public void RemoverMarcacaoSistema()
        {
            IsSistema = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa a role no sistema
        /// </summary>
        public void Desativar()
        {
            if (IsSistema)
                throw new DomainException("Não é possível desativar uma role de sistema.", nameof(Role));

            if (TemUsuariosAtivos())
                throw new DomainException("Não é possível desativar uma role que possui usuários ativos.", nameof(Role));

            Ativa = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Adiciona uma permissão à role
        /// </summary>
        /// <param name="rolePermissao">Associação role-permissão a ser adicionada</param>
        public void AdicionarPermissao(RolePermissao rolePermissao)
        {
            if (rolePermissao == null)
                throw new DomainException("A associação role-permissão não pode ser nula.", nameof(Role));

            if (rolePermissao.RoleId != Id)
                throw new DomainException("A associação não pertence a esta role.", nameof(Role));

            RolePermissoes.Add(rolePermissao);
        }

        /// <summary>
        /// Remove uma permissão da role
        /// </summary>
        /// <param name="permissaoId">ID da permissão a ser removida</param>
        public void RemoverPermissao(int permissaoId)
        {
            var rolePermissao = RolePermissoes.FirstOrDefault(rp => rp.PermissaoId == permissaoId);
            if (rolePermissao == null)
                throw new DomainException("Esta role não possui a permissão especificada.", nameof(Role));

            RolePermissoes.Remove(rolePermissao);
        }

        /// <summary>
        /// Verifica se a role é global (não específica de empresa)
        /// </summary>
        /// <returns>True se é global, false se é específica de empresa</returns>
        public bool EhGlobal()
        {
            return !EmpresaId.HasValue && string.Equals(Contexto, "GLOBAL", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifica se a role pertence a uma empresa específica
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>True se pertence à empresa, false caso contrário</returns>
        public bool PertenceAEmpresa(int empresaId)
        {
            return EmpresaId.HasValue && EmpresaId.Value == empresaId;
        }

        /// <summary>
        /// Verifica se a role já possui uma permissão específica
        /// </summary>
        /// <param name="permissaoId">ID da permissão</param>
        /// <returns>True se possui a permissão, false caso contrário</returns>
        public bool JaPossuiPermissao(int permissaoId)
        {
            return RolePermissoes.Any(rp => rp.PermissaoId == permissaoId);
        }

        /// <summary>
        /// Verifica se a role tem usuários ativos
        /// </summary>
        /// <returns>True se tem usuários ativos, false caso contrário</returns>
        public bool TemUsuariosAtivos()
        {
            return UsuarioRoles.Any(ur => ur.Ativo && ur.EstaVigente());
        }

        /// <summary>
        /// Obtém o total de permissões da role
        /// </summary>
        /// <returns>Número total de permissões</returns>
        public int ObterTotalPermissoes()
        {
            return RolePermissoes.Count;
        }

        /// <summary>
        /// Obtém o total de usuários ativos com esta role
        /// </summary>
        /// <returns>Número total de usuários ativos</returns>
        public int ObterTotalUsuariosAtivos()
        {
            return UsuarioRoles.Count(ur => ur.Ativo && ur.EstaVigente());
        }

        
        /// <summary>
        /// Valida as regras de domínio para a role
        /// </summary>
        private void ValidarDominio(
            string nome,
            string descricao,
            int? empresaId,
            string contexto)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da role é obrigatório.", nameof(Role));

            if (nome.Length > 100)
                throw new DomainException("O nome da role não pode ter mais que 100 caracteres.", nameof(Role));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição da role é obrigatória.", nameof(Role));

            if (descricao.Length > 500)
                throw new DomainException("A descrição da role não pode ter mais que 500 caracteres.", nameof(Role));

            if (empresaId.HasValue && empresaId.Value <= 0)
                throw new DomainException("O ID da empresa deve ser maior que zero.", nameof(Role));

            if (string.IsNullOrWhiteSpace(contexto))
                throw new DomainException("O contexto da role é obrigatório.", nameof(Role));

            if (!ValidarContexto(contexto))
                throw new DomainException("Contexto inválido. Contextos válidos: GLOBAL, EMPRESA.", nameof(Role));

            // Validar consistência entre contexto e empresaId
            if (string.Equals(contexto, "GLOBAL", StringComparison.OrdinalIgnoreCase) && empresaId.HasValue)
                throw new DomainException("Role global não pode ter empresa associada.", nameof(Role));

            if (string.Equals(contexto, "EMPRESA", StringComparison.OrdinalIgnoreCase) && !empresaId.HasValue)
                throw new DomainException("Role de empresa deve ter uma empresa associada.", nameof(Role));
        }

        /// <summary>
        /// Valida se o contexto é válido
        /// </summary>
        private bool ValidarContexto(string contexto)
        {
            var contextosValidos = new[] { "GLOBAL", "EMPRESA" };
            return contextosValidos.Contains(contexto.ToUpper());
        }
    }
}
