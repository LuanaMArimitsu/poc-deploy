using System.Text.RegularExpressions;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Usuario
{
    /// <summary>
    /// Entidade que representa um usuário do sistema (funcionário/vendedor)
    /// </summary>
    public class Usuario : EntidadeBase
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Email institucional do usuário
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Cargo do usuário na empresa
        /// </summary>
        public string? Cargo { get; private set; }

        /// <summary>
        /// Departamento do usuário na empresa
        /// </summary>
        public string? Departamento { get; private set; }

        /// <summary>
        /// Indica se o usuário está ativo no sistema
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// ID do usuário superior (gerente/supervisor) - pode ser nulo
        /// </summary>
        public int? UsuarioSuperiorId { get; private set; }

        /// <summary>
        /// ID de objeto do Active Directory/Azure AD
        /// </summary>
        public string ObjectId { get; private set; }

        /// <summary>
        /// UPN (User Principal Name) do usuário no Active Directory/Azure AD
        /// </summary>
        public string Upn { get; private set; }

        /// <summary>
        /// Nome de exibição do usuário no Active Directory/Azure AD
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Indica se o usuário é externo (convidado)
        /// </summary>
        public bool IsExternal { get; private set; }

        /// <summary>
        /// Indica se o usuário é um bot
        /// </summary>
        public bool IsBot { get; private set; }

        /// <summary>
        /// Referência para o usuário superior na hierarquia
        /// </summary>
        public virtual Usuario UsuarioSuperior { get; private set; }

        /// <summary>
        /// Coleção de usuários subordinados a este usuário
        /// </summary>
        public virtual ICollection<Usuario> Subordinados { get; private set; }

        /// <summary>
        /// Coleção de dispositivos associados a este usuário
        /// </summary>
        public virtual ICollection<Dispositivo> Dispositivos { get; private set; }

        /// <summary>
        /// Coleção de associações entre o usuário e empresas
        /// </summary>
        public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; private set; }

        /// <summary>
        /// Coleção de horários de trabalho do usuário
        /// </summary>
        public virtual ICollection<UsuarioHorario> HorariosUsuario { get; private set; }

        public virtual ICollection<RolePermissao> RolePermissoesConcedidas { get; private set; }

        public virtual ICollection<UsuarioRole> UsuarioRoles { get; private set; }
        public virtual ICollection<UsuarioRole> UsuarioRolesAtribuidos { get; private set; }

        ///// <summary>
        ///// Coleção de oportunidades sob responsabilidade deste usuário
        ///// </summary>
        //public virtual ICollection<Oportunidade.Oportunidade> OportunidadesSobResponsabilidade { get; private set; }

        /// <summary>
        /// Coleção de conversas associadas a este usuário
        /// </summary>
        public virtual ICollection<Conversa> Conversas { get; private set; }

        /// <summary>
        /// Coleção de mensagens enviadas por este usuário
        /// </summary>
        public virtual ICollection<Mensagem> Mensagens { get; private set; }

        /// <summary>
        ///  Coleção de vínculos de equipe associados a este usuário
        /// </summary>
        public virtual ICollection<MembroEquipe> MembrosEquipe { get; private set; } 

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Usuario()
        {
            Subordinados = new HashSet<Usuario>();
            Dispositivos = new HashSet<Dispositivo>();
            UsuarioEmpresas = new HashSet<UsuarioEmpresa>();
            HorariosUsuario = new HashSet<UsuarioHorario>();
            //OportunidadesSobResponsabilidade = new HashSet<Oportunidade.Oportunidade>();
            Conversas = new HashSet<Conversa>();
            Mensagens = new HashSet<Mensagem>();
            MembrosEquipe =  new HashSet<MembroEquipe>();
        }

        /// <summary>
        /// Construtor para criar um novo usuário
        /// </summary>
        /// <param name="nome">Nome completo do usuário</param>
        /// <param name="email">Email institucional do usuário</param>
        /// <param name="cargo">Cargo do usuário</param>
        /// <param name="departamento">Departamento do usuário</param>
        /// <param name="objectId">ID de objeto do Active Directory/Azure AD</param>
        /// <param name="upn">UPN (User Principal Name) do usuário</param>
        /// <param name="displayName">Nome de exibição do usuário</param>
        /// <param name="isExternal">Indica se o usuário é externo</param>
        /// <param name="usuarioSuperiorId">ID do usuário superior (opcional)</param>
        public Usuario(
            string nome,
            string email,
            string? cargo,
            string? departamento,
            string objectId,
            string upn,
            string displayName,
            bool isExternal = false,
            bool isBot = false,
            int? usuarioSuperiorId = null)
        {
            ValidarDominio(nome, email, cargo, departamento, objectId, upn, displayName, usuarioSuperiorId);

            Nome = nome;
            Email = email;
            Cargo = cargo;
            Departamento = departamento;
            ObjectId = objectId;
            Upn = upn;
            DisplayName = displayName;
            IsExternal = isExternal;
            IsBot = isBot;
            UsuarioSuperiorId = usuarioSuperiorId;
            Ativo = true;

            Subordinados = new HashSet<Usuario>();
            Dispositivos = new HashSet<Dispositivo>();
            UsuarioEmpresas = new HashSet<UsuarioEmpresa>();
            HorariosUsuario = new HashSet<UsuarioHorario>();
            //OportunidadesSobResponsabilidade = new HashSet<Oportunidade.Oportunidade>();
            Conversas = new HashSet<Conversa>();
            Mensagens = new HashSet<Mensagem>();
        }

        /// <summary>
        /// Atualiza as informações básicas do usuário
        /// </summary>
        /// <param name="nome">Nome completo do usuário</param>
        /// <param name="cargo">Cargo do usuário</param>
        /// <param name="departamento">Departamento do usuário</param>
        /// <param name="displayName">Nome de exibição do usuário</param>
        public void AtualizarInformacoes(string nome, string cargo, string departamento, string displayName)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do usuário é obrigatório.", nameof(Usuario));

            if (nome.Length > 200)
                throw new DomainException("O nome do usuário não pode ter mais que 200 caracteres.", nameof(Usuario));

            if (!string.IsNullOrWhiteSpace(cargo) && cargo.Length > 100)
                throw new DomainException("O cargo não pode ter mais que 100 caracteres.", nameof(Usuario));

            if (!string.IsNullOrWhiteSpace(departamento) && departamento.Length > 100)
                throw new DomainException("O departamento não pode ter mais que 100 caracteres.", nameof(Usuario));

            if (!string.IsNullOrWhiteSpace(displayName) && displayName.Length > 200)
                throw new DomainException("O nome de exibição não pode ter mais que 200 caracteres.", nameof(Usuario));

            Nome = nome;
            Cargo = cargo;
            Departamento = departamento;
            DisplayName = displayName;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Altera o e-mail do usuário
        /// </summary>
        /// <param name="email">Novo e-mail</param>
        public void AlterarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("O email do usuário é obrigatório.", nameof(Usuario));

            if (!ValidarEmail(email))
                throw new DomainException("O formato do email é inválido.", nameof(Usuario));

            if (email.Length > 100)
                throw new DomainException("O email não pode ter mais que 100 caracteres.", nameof(Usuario));

            Email = email;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Altera o usuário superior (gerente/supervisor)
        /// </summary>
        /// <param name="usuarioSuperiorId">ID do novo usuário superior</param>
        public void AlterarSuperior(int? usuarioSuperiorId)
        {
            if (usuarioSuperiorId.HasValue && usuarioSuperiorId.Value <= 0)
                throw new DomainException("O ID do usuário superior deve ser maior que zero.", nameof(Usuario));

            if (usuarioSuperiorId.HasValue && usuarioSuperiorId.Value == Id)
                throw new DomainException("Um usuário não pode ser superior de si mesmo.", nameof(Usuario));

            UsuarioSuperiorId = usuarioSuperiorId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza as informações de identidade (Active Directory/Azure AD)
        /// </summary>
        /// <param name="objectId">ID de objeto do Azure AD</param>
        /// <param name="upn">UPN do usuário</param>
        /// <param name="isExternal">Indica se é usuário externo</param>
        public void AtualizarInformacoesIdentidade(string objectId, string upn, bool isExternal)
        {
            if (string.IsNullOrWhiteSpace(objectId))
                throw new DomainException("O ObjectId é obrigatório.", nameof(Usuario));

            if (string.IsNullOrWhiteSpace(upn))
                throw new DomainException("O UPN é obrigatório.", nameof(Usuario));

            ObjectId = objectId;
            Upn = upn;
            IsExternal = isExternal;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Ativa o usuário no sistema
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa o usuário no sistema
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Adiciona um dispositivo ao usuário
        /// </summary>
        /// <param name="dispositivo">Dispositivo a ser adicionado</param>
        public void AdicionarDispositivo(Dispositivo dispositivo)
        {
            if (dispositivo == null)
                throw new DomainException("O dispositivo não pode ser nulo.", nameof(Usuario));

            if (dispositivo.UsuarioId != Id)
                throw new DomainException("O dispositivo não pertence a este usuário.", nameof(Usuario));

            Dispositivos.Add(dispositivo);
        }

        /// <summary>
        /// Adiciona uma associação entre o usuário e uma empresa
        /// </summary>
        /// <param name="usuarioEmpresa">Associação a ser adicionada</param>
        public void AdicionarUsuarioEmpresa(UsuarioEmpresa usuarioEmpresa)
        {
            if (usuarioEmpresa == null)
                throw new DomainException("A associação usuário-empresa não pode ser nula.", nameof(Usuario));

            if (usuarioEmpresa.UsuarioId != Id)
                throw new DomainException("A associação não pertence a este usuário.", nameof(Usuario));

            UsuarioEmpresas.Add(usuarioEmpresa);
        }

        /// <summary>
        /// Adiciona um horário de trabalho ao usuário
        /// </summary>
        /// <param name="horario">Horário a ser adicionado</param>
        public void AdicionarHorario(UsuarioHorario horario)
        {
            if (horario == null)
                throw new DomainException("O horário não pode ser nulo.", nameof(Usuario));

            if (horario.UsuarioId != Id)
                throw new DomainException("O horário não pertence a este usuário.", nameof(Usuario));

            // Verifica se já existe um horário para este dia da semana
            var horarioExistente = HorariosUsuario.FirstOrDefault(h => h.DiaSemanaId == horario.DiaSemanaId);
            if (horarioExistente != null)
                throw new DomainException("Já existe um horário definido para este dia da semana.", nameof(Usuario));

            HorariosUsuario.Add(horario);
        }

        /// <summary>
        /// Verifica se o usuário está associado a uma empresa específica
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>True se o usuário está associado à empresa, false caso contrário</returns>
        public bool EstaAssociadoAEmpresa(int empresaId)
        {
            return UsuarioEmpresas.Any(ue => ue.EmpresaId == empresaId);
        }

        /// <summary>
        /// Verifica se o usuário tem uma empresa associada como principal
        /// </summary>
        /// <returns>True se existe uma empresa principal, false caso contrário</returns>
        public bool TemEmpresaPrincipal()
        {
            return UsuarioEmpresas.Any(ue => ue.IsPrincipal);
        }

        /// <summary>
        /// Obtém o ID da empresa principal do usuário
        /// </summary>
        /// <returns>ID da empresa principal ou null se não houver</returns>
        public int? ObterEmpresaPrincipalId()
        {
            var empresaPrincipal = UsuarioEmpresas.FirstOrDefault(ue => ue.IsPrincipal);
            return empresaPrincipal?.EmpresaId;
        }

        /// <summary>
        /// Valida as regras de domínio para o usuário
        /// </summary>
        private void ValidarDominio(
            string nome,
            string email,
            string? cargo,
            string? departamento,
            string objectId,
            string upn,
            string displayName,
            int? usuarioSuperiorId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do usuário é obrigatório.", nameof(Usuario));

            if (nome.Length > 200)
                throw new DomainException("O nome do usuário não pode ter mais que 200 caracteres.", nameof(Usuario));

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!ValidarEmail(email))
                    throw new DomainException("O formato do email é inválido.", nameof(Usuario));

                if (email.Length > 100)
                    throw new DomainException("O email não pode ter mais que 100 caracteres.", nameof(Usuario));
            } else
            {
                throw new DomainException("O email do usuário é obrigatório.", nameof(Usuario));
            }

            if (!string.IsNullOrWhiteSpace(cargo) && cargo.Length > 100)
                throw new DomainException("O cargo não pode ter mais que 100 caracteres.", nameof(Usuario));

            if (!string.IsNullOrWhiteSpace(departamento) && departamento.Length > 100)
                throw new DomainException("O departamento não pode ter mais que 100 caracteres.", nameof(Usuario));

            if (string.IsNullOrWhiteSpace(objectId))
                throw new DomainException("O ObjectId é obrigatório.", nameof(Usuario));

            if (string.IsNullOrWhiteSpace(upn))
                throw new DomainException("O UPN é obrigatório.", nameof(Usuario));

            if (!string.IsNullOrWhiteSpace(displayName) && displayName.Length > 200)
                throw new DomainException("O nome de exibição não pode ter mais que 200 caracteres.", nameof(Usuario));

            if (usuarioSuperiorId.HasValue && usuarioSuperiorId.Value <= 0)
                throw new DomainException("O ID do usuário superior deve ser maior que zero.", nameof(Usuario));
        }

        /// <summary>
        /// Valida o formato do email
        /// </summary>
        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Verifica o formato básico do email usando regex
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}