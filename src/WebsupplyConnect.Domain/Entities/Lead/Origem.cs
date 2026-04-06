using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    /// <summary>
    /// Entidade que representa a origem de um lead no sistema.
    /// </summary>
    public class Origem : EntidadeBase
    {
        /// <summary>
        /// Nome da origem
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição da origem
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// ID do tipo de origem
        /// </summary>
        public int OrigemTipoId { get; private set; }

        /// <summary>
        /// Navegação para o tipo de origem
        /// </summary>
        public virtual OrigemTipo OrigemTipo { get; private set; }

        public virtual ICollection<LeadEvento> LeadEventos { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Origem() : base()
        {
        }

        /// <summary>
        /// Construtor para criar uma nova origem
        /// </summary>
        /// <param name="nome">Nome da origem</param>
        /// <param name="origemTipoId">ID do tipo de origem</param>
        /// <param name="descricao">Descrição da origem</param>
        public Origem(
            string nome,
            int origemTipoId,
            string descricao = null) : base()
        {
            ValidarDominio(nome, origemTipoId, descricao);

            Nome = nome;
            Descricao = descricao;
            OrigemTipoId = origemTipoId;
        }

        /// <summary>
        /// Construtor para criar uma nova origem Seed
        /// </summary>
        /// <param name="nome">Nome da origem</param>
        /// <param name="origemTipoId">ID do tipo de origem</param>
        /// <param name="descricao">Descrição da origem</param>
        public Origem(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string nome,
            int origemTipoId,
            string? descricao = null) : base()
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            Nome = nome;
            Descricao = descricao;
            OrigemTipoId = origemTipoId;
        }

        /// <summary>
        /// Atualiza as informações da origem
        /// </summary>
        /// <param name="nome">Nome da origem</param>
        /// <param name="descricao">Descrição da origem</param>
        /// <param name="parametrosIntegracao">Parâmetros de integração (JSON)</param>
        public void Atualizar(
            string nome,
            int origemTipoId,
            string descricao = null)
        {
            ValidarDominio(nome, OrigemTipoId, descricao);

            Nome = nome;
            OrigemTipoId = origemTipoId;
            Descricao = descricao;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Altera o tipo de origem
        /// </summary>
        /// <param name="origemTipoId">ID do novo tipo de origem</param>
        public void AlterarTipoOrigem(int origemTipoId)
        {
            if (origemTipoId <= 0)
                throw new DomainException("O ID do tipo de origem deve ser maior que zero.", nameof(Origem));

            OrigemTipoId = origemTipoId;

            AtualizarDataModificacao();
        }


        /// <summary>
        /// Valida as regras de domínio para a origem
        /// </summary>
        private void ValidarDominio(string nome, int origemTipoId, string descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da origem é obrigatório.", nameof(Origem));

            if (nome.Length > 100)
                throw new DomainException("O nome da origem não pode ter mais que 100 caracteres.", nameof(Origem));

            if (!string.IsNullOrWhiteSpace(descricao) && descricao.Length > 500)
                throw new DomainException("A descrição não pode ter mais que 500 caracteres.", nameof(Origem));

            if (origemTipoId <= 0)
                throw new DomainException("O ID do tipo de origem deve ser maior que zero.", nameof(Origem));
        }
    }
}