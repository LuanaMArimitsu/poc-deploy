using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Permissao
{
    /// <summary>
    /// Entidade associativa que representa a relação entre Role e Permissão
    /// Define quais permissões cada role possui
    /// </summary>
    public class RolePermissao
    {
        /// <summary>
        /// ID da role
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// ID da permissão
        /// </summary>
        public int PermissaoId { get; set; }

        /// <summary>
        /// Data em que a permissão foi concedida à role
        /// </summary>
        public DateTime DataConcessao { get; private set; }

        /// <summary>
        /// ID do usuário que concedeu a permissão
        /// </summary>
        public int ConcessorId { get; private set; }

        /// <summary>
        /// Observações sobre a concessão da permissão (opcional)
        /// </summary>
        public string? Observacoes { get; private set; }

        /// <summary>
        /// Navegação para a role
        /// </summary>
        public virtual Role Role { get; set; }

        /// <summary>
        /// Navegação para a permissão
        /// </summary>
        public virtual Permissao Permissao { get; set; }

        /// <summary>
        /// Navegação para o usuário concessor
        /// </summary>
        public virtual Usuario.Usuario Concessor { get; set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected RolePermissao()
        {
            DataConcessao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Construtor para criar uma nova associação role-permissão
        /// </summary>
        /// <param name="roleId">ID da role</param>
        /// <param name="permissaoId">ID da permissão</param>
        /// <param name="concessorId">ID do usuário que está concedendo</param>
        /// <param name="observacoes">Observações sobre a concessão (opcional)</param>
        public RolePermissao(
            int roleId,
            int permissaoId,
            int concessorId,
            string? observacoes = null)
        {
            ValidarDominio(roleId, permissaoId, concessorId);

            RoleId = roleId;
            PermissaoId = permissaoId;
            ConcessorId = concessorId;
            Observacoes = observacoes;
            DataConcessao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza as observações da concessão
        /// </summary>
        /// <param name="observacoes">Novas observações</param>
        public void AtualizarObservacoes(string? observacoes)
        {
            if (!string.IsNullOrWhiteSpace(observacoes) && observacoes.Length > 1000)
                throw new DomainException("As observações não podem ter mais que 1000 caracteres.", nameof(RolePermissao));

            Observacoes = observacoes;
        }

        /// <summary>
        /// Verifica se a concessão é recente (últimas 24 horas)
        /// </summary>
        /// <returns>True se é recente, false caso contrário</returns>
        public bool EhConcessaoRecente()
        {
            return TimeHelper.GetBrasiliaTime().Subtract(DataConcessao).TotalHours <= 24;
        }

        /// <summary>
        /// Obtém a idade da concessão em dias
        /// </summary>
        /// <returns>Número de dias desde a concessão</returns>
        public int ObterIdadeEmDias()
        {
            return (int)TimeHelper.GetBrasiliaTime().Subtract(DataConcessao).TotalDays;
        }

        /// <summary>
        /// Verifica se a concessão foi feita por um usuário específico
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se foi concedida pelo usuário, false caso contrário</returns>
        public bool FoiConcedidaPor(int usuarioId)
        {
            return ConcessorId == usuarioId;
        }

        /// <summary>
        /// Valida as regras de domínio para a associação role-permissão
        /// </summary>
        private void ValidarDominio(int roleId, int permissaoId, int concessorId)
        {
            if (roleId <= 0)
                throw new DomainException("O ID da role deve ser maior que zero.", nameof(RolePermissao));

            if (permissaoId <= 0)
                throw new DomainException("O ID da permissão deve ser maior que zero.", nameof(RolePermissao));

            if (concessorId <= 0)
                throw new DomainException("O ID do concessor deve ser maior que zero.", nameof(RolePermissao));
        }
    }
}
