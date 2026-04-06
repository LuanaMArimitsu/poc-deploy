using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Permissao
{
    /// <summary>
    /// Entidade associativa que representa a relação entre Usuário e Role
    /// Define quais roles cada usuário possui
    /// </summary>
    public class UsuarioRole
    {
        /// <summary>
        /// ID do usuário
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// ID da role
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Data em que a role foi atribuída ao usuário
        /// </summary>
        public DateTime DataAtribuicao { get; private set; }

        /// <summary>
        /// Data de expiração da atribuição (opcional)
        /// </summary>
        public DateTime? DataExpiracao { get; private set; }

        /// <summary>
        /// Indica se a atribuição está ativa
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// ID do usuário que fez a atribuição
        /// </summary>
        public int AtribuidorId { get; private set; }

        /// <summary>
        /// Justificativa para a atribuição da role
        /// </summary>
        public string? Justificativa { get; private set; }

        /// <summary>
        /// Navegação para o usuário
        /// </summary>
        public virtual Usuario.Usuario Usuario { get; set; }

        /// <summary>
        /// Navegação para a role
        /// </summary>
        public virtual Role Role { get; set; }

        /// <summary>
        /// Navegação para o usuário atribuidor
        /// </summary>
        public virtual Usuario.Usuario Atribuidor { get; set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected UsuarioRole()
        {
            DataAtribuicao = TimeHelper.GetBrasiliaTime();
            Ativo = true;
        }

        /// <summary>
        /// Construtor para criar uma nova atribuição usuário-role
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="roleId">ID da role</param>
        /// <param name="atribuidorId">ID do usuário que está atribuindo</param>
        /// <param name="justificativa">Justificativa para a atribuição</param>
        /// <param name="dataExpiracao">Data de expiração (opcional)</param>
        public UsuarioRole(
            int usuarioId,
            int roleId,
            int atribuidorId,
            string? justificativa = null,
            DateTime? dataExpiracao = null)
        {
            ValidarDominio(usuarioId, roleId, atribuidorId, dataExpiracao);

            UsuarioId = usuarioId;
            RoleId = roleId;
            AtribuidorId = atribuidorId;
            Justificativa = justificativa;
            DataExpiracao = dataExpiracao;
            DataAtribuicao = TimeHelper.GetBrasiliaTime();
            Ativo = true;
        }

        /// <summary>
        /// Verifica se a atribuição está vigente (ativa e não expirada)
        /// </summary>
        /// <returns>True se está vigente, false caso contrário</returns>
        public bool EstaVigente()
        {
            return Ativo && (!DataExpiracao.HasValue || DataExpiracao.Value > TimeHelper.GetBrasiliaTime());
        }

        /// <summary>
        /// Verifica se a atribuição expirou
        /// </summary>
        /// <returns>True se expirou, false caso contrário</returns>
        public bool EstaExpirada()
        {
            return DataExpiracao.HasValue && DataExpiracao.Value <= TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Verifica se a atribuição expira em breve (próximos 7 dias)
        /// </summary>
        /// <returns>True se expira em breve, false caso contrário</returns>
        public bool ExpiraEmBreve()
        {
            if (!DataExpiracao.HasValue)
                return false;

            var diasParaExpiracao = (DataExpiracao.Value - TimeHelper.GetBrasiliaTime()).TotalDays;
            return diasParaExpiracao <= 7 && diasParaExpiracao > 0;
        }

        /// <summary>
        /// Ativa a atribuição da role
        /// </summary>
        public void Ativar()
        {
            if (EstaExpirada())
                throw new DomainException("Não é possível ativar uma atribuição expirada.", nameof(UsuarioRole));

            Ativo = true;
        }

        /// <summary>
        /// Desativa a atribuição da role
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
        }

        /// <summary>
        /// Prorroga a data de expiração da atribuição
        /// </summary>
        /// <param name="novaDataExpiracao">Nova data de expiração</param>
        public void ProrrogarExpiracao(DateTime? novaDataExpiracao)
        {
            if (novaDataExpiracao.HasValue && novaDataExpiracao.Value <= TimeHelper.GetBrasiliaTime())
                throw new DomainException("A nova data de expiração deve ser futura.", nameof(UsuarioRole));

            if (novaDataExpiracao.HasValue && DataExpiracao.HasValue &&
                novaDataExpiracao.Value <= DataExpiracao.Value)
                throw new DomainException("A nova data de expiração deve ser posterior à atual.", nameof(UsuarioRole));

            DataExpiracao = novaDataExpiracao;
        }

        /// <summary>
        /// Remove a data de expiração (torna permanente)
        /// </summary>
        public void TornarPermanente()
        {
            DataExpiracao = null;
        }

        /// <summary>
        /// Atualiza a justificativa da atribuição
        /// </summary>
        /// <param name="novaJustificativa">Nova justificativa</param>
        public void AtualizarJustificativa(string? novaJustificativa)
        {
            if (!string.IsNullOrWhiteSpace(novaJustificativa) && novaJustificativa.Length > 1000)
                throw new DomainException("A justificativa não pode ter mais que 1000 caracteres.", nameof(UsuarioRole));

            Justificativa = novaJustificativa;
        }

        /// <summary>
        /// Obtém o número de dias até a expiração
        /// </summary>
        /// <returns>Número de dias até a expiração (null se não expira)</returns>
        public int? ObterDiasParaExpiracao()
        {
            if (!DataExpiracao.HasValue)
                return null;

            var diasParaExpiracao = (DataExpiracao.Value - TimeHelper.GetBrasiliaTime()).TotalDays;
            return diasParaExpiracao > 0 ? (int)Math.Ceiling(diasParaExpiracao) : 0;
        }

        /// <summary>
        /// Obtém a idade da atribuição em dias
        /// </summary>
        /// <returns>Número de dias desde a atribuição</returns>
        public int ObterIdadeEmDias()
        {
            return (int)TimeHelper.GetBrasiliaTime().Subtract(DataAtribuicao).TotalDays;
        }

        /// <summary>
        /// Verifica se a atribuição é recente (últimas 24 horas)
        /// </summary>
        /// <returns>True se é recente, false caso contrário</returns>
        public bool EhAtribuicaoRecente()
        {
            return TimeHelper.GetBrasiliaTime().Subtract(DataAtribuicao).TotalHours <= 24;
        }

        /// <summary>
        /// Verifica se a atribuição foi feita por um usuário específico
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se foi atribuída pelo usuário, false caso contrário</returns>
        public bool FoiAtribuidaPor(int usuarioId)
        {
            return AtribuidorId == usuarioId;
        }

        /// <summary>
        /// Verifica se é uma atribuição temporária (com data de expiração)
        /// </summary>
        /// <returns>True se é temporária, false se é permanente</returns>
        public bool EhTemporaria()
        {
            return DataExpiracao.HasValue;
        }

        /// <summary>
        /// Verifica se é uma atribuição permanente (sem data de expiração)
        /// </summary>
        /// <returns>True se é permanente, false se é temporária</returns>
        public bool EhPermanente()
        {
            return !DataExpiracao.HasValue;
        }

        /// <summary>
        /// Obtém o status da atribuição
        /// </summary>
        /// <returns>Status da atribuição (Ativa, Inativa, Expirada)</returns>
        public string ObterStatus()
        {
            if (!Ativo)
                return "Inativa";

            if (EstaExpirada())
                return "Expirada";

            if (ExpiraEmBreve())
                return "Expira em Breve";

            return "Ativa";
        }

        /// <summary>
        /// Verifica se a atribuição pode ser renovada (está expirada ou prestes a expirar)
        /// </summary>
        /// <returns>True se pode ser renovada, false caso contrário</returns>
        public bool PodeSerRenovada()
        {
            return EhTemporaria() && (EstaExpirada() || ExpiraEmBreve());
        }

        /// <summary>
        /// Renova a atribuição por um período específico
        /// </summary>
        /// <param name="diasRenovacao">Número de dias para renovar</param>
        public void Renovar(int diasRenovacao)
        {
            if (diasRenovacao <= 0)
                throw new DomainException("O período de renovação deve ser maior que zero.", nameof(UsuarioRole));

            if (!PodeSerRenovada())
                throw new DomainException("Esta atribuição não pode ser renovada no momento.", nameof(UsuarioRole));

            var novaDataExpiracao = TimeHelper.GetBrasiliaTime().AddDays(diasRenovacao);
            ProrrogarExpiracao(novaDataExpiracao);

            if (!Ativo)
                Ativar();
        }

        /// <summary>
        /// Valida as regras de domínio para a atribuição usuário-role
        /// </summary>
        private void ValidarDominio(
            int usuarioId,
            int roleId,
            int atribuidorId,
            DateTime? dataExpiracao)
        {
            if (usuarioId <= 0)
                throw new DomainException("O ID do usuário deve ser maior que zero.", nameof(UsuarioRole));

            if (roleId <= 0)
                throw new DomainException("O ID da role deve ser maior que zero.", nameof(UsuarioRole));

            if (atribuidorId <= 0)
                throw new DomainException("O ID do atribuidor deve ser maior que zero.", nameof(UsuarioRole));

            if (dataExpiracao.HasValue && dataExpiracao.Value <= TimeHelper.GetBrasiliaTime())
                throw new DomainException("A data de expiração deve ser futura.", nameof(UsuarioRole));
        }
    }
}
