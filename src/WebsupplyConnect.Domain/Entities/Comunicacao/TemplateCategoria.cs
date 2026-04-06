using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa uma categoria de templates
    /// </summary>
    public class TemplateCategoria : EntidadeTipificacao
    {

        // Construtor protegido para EF
        protected TemplateCategoria() : base()
        {
        }

        /// <summary>
        /// <summary>
        /// Cria uma nova categoria do template
        /// </summary>
        public TemplateCategoria(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? cor,
            string? icone) : base(codigo.ToUpperInvariant(), nome, descricao, ordem, icone, cor)
        {

        }

        /// <summary>
        /// Cria uma nova categoria do template
        /// </summary>
        public TemplateCategoria(
            int id,
            DateTime dataCricao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? cor,
            string? icone) : base(codigo.ToUpperInvariant(), nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCricao;
            DataModificacao = dataModificacao;
        }

        /// <summary>
        /// <summary>
        /// Atualiza as informações da categoria do template
        /// </summary>
        public override void Atualizar(
            string nome,
            string descricao,
            int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }
    }
}