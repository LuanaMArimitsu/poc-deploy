using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Produto
{
    public class ProdutoOperacaoTipo : EntidadeTipificacao
    {
        //Construtor protegido para EF
        protected ProdutoOperacaoTipo() : base()
        {

        }

        /// <summary>
        /// Cria um novo tipo de Operação
        /// </summary>
        public ProdutoOperacaoTipo(int id, string codigo, string nome, string descricao, int ordem, string? icone, string? cor, DateTime dataCriacao, DateTime dataModificacao)
        {
            Id = id;
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
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
