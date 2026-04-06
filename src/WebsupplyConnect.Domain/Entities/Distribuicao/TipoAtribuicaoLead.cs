using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Entidade de tipificação que define os tipos de atribui��o de leads.
    /// Herda de EntidadeTipificacao para manter o padrão do projeto.
    /// </summary>
    public class TipoAtribuicaoLead : EntidadeTipificacao
    {
        // Propriedade de navegação
        public virtual ICollection<AtribuicaoLead> AtribuicoesLead { get; private set; }

        // Construtor protegido para EF Core
        protected TipoAtribuicaoLead() : base()
        {
            AtribuicoesLead = new HashSet<AtribuicaoLead>();
        }

        /// <summary>
        /// Cria um novo tipo de atribui��o de lead
        /// </summary>
        public TipoAtribuicaoLead(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? icone,
            string? cor) 
            : base(codigo, nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            AtribuicoesLead = new HashSet<AtribuicaoLead>();
        }

        /// <summary>
        /// Atualiza as informações do tipo
        /// </summary>
        public override void Atualizar(string nome, string descricao, int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }
    }
}