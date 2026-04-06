using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Entidade de tipificação que define os tipos de regras disponíveis no sistema de distribuição.
    /// Herda de EntidadeTipificacao para manter o padrão do projeto.
    /// </summary>
    public class TipoRegraDistribuicao : EntidadeTipificacao
    {
        /// <summary>
        /// Categoria da regra (ex: "PERFORMANCE", "SEQUENCIAL", "TEMPORAL")
        /// </summary>
        public string Categoria { get; private set; }

        // Propriedade de navegação
        public virtual ICollection<RegraDistribuicao> RegrasDistribuicao { get; private set; }

        // Construtor protegido para EF Core
        protected TipoRegraDistribuicao() : base()
        {
            RegrasDistribuicao = new HashSet<RegraDistribuicao>();
        }

        /// <summary>
        /// Cria um novo tipo de regra de distribuição
        /// </summary>
        public TipoRegraDistribuicao(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? icone,
            string? cor,
            string categoria) 
            : base(codigo, nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            Categoria = categoria ?? "GERAL";
            RegrasDistribuicao = new HashSet<RegraDistribuicao>();
        }

        /// <summary>
        /// Atualiza as informações do tipo
        /// </summary>
        public override void Atualizar(string nome, string descricao, int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }

        /// <summary>
        /// Atualiza a categoria da regra de distribuição
        /// </summary>
        public void AtualizarCategoria(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                categoria = "GERAL";
                
            Categoria = categoria;
            AtualizarDataModificacao();
        }
    }
}