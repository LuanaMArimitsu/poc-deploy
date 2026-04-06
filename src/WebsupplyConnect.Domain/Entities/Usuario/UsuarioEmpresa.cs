using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Usuario
{
    /// <summary>
    /// Entidade que representa a associação entre um usuário e uma empresa.
    /// Esta é uma entidade associativa que materializa o relacionamento muitos-para-muitos
    /// entre usuário e empresa, permitindo que um usuário tenha acesso a múltiplas empresas.
    /// </summary>
    public class UsuarioEmpresa 
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID do usuário
        /// </summary>
        public int UsuarioId { get;  set; }

        /// <summary>
        /// ID da empresa
        /// </summary>
        public int EmpresaId { get;  set; }

        /// <summary>
        /// Indica se esta é a empresa principal do usuário
        /// </summary>
        public bool IsPrincipal { get; set; }

        /// <summary>
        /// Canal Id padrão que o usuário atende.
        /// </summary>
        public int CanalPadraoId { get; private set; }

        /// <summary>
        /// Data em que a associação foi criada
        /// </summary>
        public DateTime DataAssociacao { get; private set; }

        /// <summary>
        /// Navegação para o usuário
        /// </summary>
        public virtual Usuario Usuario { get; private set; }

        /// <summary>
        /// Navegação para a empresa
        /// </summary>
        public virtual Empresa.Empresa Empresa { get; private set; }

        /// <summary>
        /// Referência para o canal padrão do usuário
        /// </summary>
        public virtual Canal CanalPadrao { get; private set; }

        /// <summary>
        /// Código do vendedor NBS associado a este usuário e empresa usado para criar evento no NBS
        /// </summary>
        public string? CodVendedorNBS { get; private set; }

        /// <summary>
        /// ID da equipe padrão do usuário para esta empresa
        /// </summary>
        public int? EquipePadraoId { get; set; }

        /// <summary>
        /// Referência para a equipe padrão do usuário
        /// </summary>
        public virtual Equipe.Equipe EquipePadrao { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected UsuarioEmpresa()
        {
        }

        /// <summary>
        /// Construtor para criar uma nova associação entre usuário e empresa
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="canalPadraoId">ID do canal</param>
        /// <param name="codVendedorNBS">Código do vendedor NBS associado</param>
        /// <param name="isPrincipal">Indica se é a empresa principal do usuário</param>
        public UsuarioEmpresa(int usuarioId, int empresaId, int canalPadraoId, int equipePadraoId, string? codVendedorNBS = null, bool isPrincipal = false)
        {
            ValidarDominio(usuarioId, empresaId, canalPadraoId, equipePadraoId);

            UsuarioId = usuarioId;
            EmpresaId = empresaId;
            CanalPadraoId = canalPadraoId;
            EquipePadraoId = equipePadraoId;
            IsPrincipal = isPrincipal;
            CodVendedorNBS = codVendedorNBS?.Trim();
            DataAssociacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Define esta associação como a empresa principal do usuário
        /// </summary>
        public void DefinirComoPrincipal()
        {
            IsPrincipal = true;
        }

        public void DefinirComoPrincipal(bool isPrincipal)
        {
            IsPrincipal = isPrincipal;
        }

        /// <summary>
        /// Remove a marcação de empresa principal
        /// </summary>
        public void RemoverComoPrincipal()
        {
            IsPrincipal = false;
        }

        /// <summary>
        /// Atualiza a data de associação
        /// </summary>
        public void AtualizarDataAssociacao()
        {
            DataAssociacao = TimeHelper.GetBrasiliaTime();
        }

        public void AtualizarCanalPadrao(int canalPadraoId)
        {
            if (canalPadraoId <= 0)
                throw new DomainException("O ID do canal padrão deve ser maior que zero.", nameof(UsuarioEmpresa));
            CanalPadraoId = canalPadraoId;
        }

        /// <summary>
        /// Atualiza o código do vendedor NBS associado a este vínculo.
        /// </summary>
        /// <param name="novoCodVendedorNBS">Novo código do vendedor NBS</param>
        public void AtualizarCodVendedorNBS(string? novoCodVendedorNBS)
        {
            CodVendedorNBS = novoCodVendedorNBS?.Trim();
        }

        public void AtualizarEquipePadrao(int equipePadraoId)
        {
            if (equipePadraoId <= 0)
                throw new DomainException("O ID da equipe padrão deve ser maior que zero.", nameof(UsuarioEmpresa));
            EquipePadraoId = equipePadraoId;
        }

        /// <summary>
        /// Valida as regras de domínio para a associação usuário-empresa
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="canalPadraoId">ID do canal</param>
        private void ValidarDominio(int usuarioId, int empresaId, int canalPadraoId, int equipePadraoId)
        {
            if (usuarioId <= 0)
                throw new DomainException("O ID do usuário deve ser maior que zero.", nameof(UsuarioEmpresa));

            if (empresaId <= 0)
                throw new DomainException("O ID da empresa deve ser maior que zero.", nameof(UsuarioEmpresa));

            if (canalPadraoId <= 0)
                throw new DomainException("O ID do canal padrão deve ser maior que zero.", nameof(UsuarioEmpresa));

            if (equipePadraoId <= 0)
                throw new DomainException("O ID da equipe padrão deve ser maior que zero.", nameof(UsuarioEmpresa));
        }
    }
}
