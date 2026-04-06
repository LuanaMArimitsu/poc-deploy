using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    /// <summary>
    /// Entidade que representa o tipo de origem de leads no sistema.
    /// Herda de EntidadeTipificacao para implementar o padrão TPH (Table Per Hierarchy).
    /// </summary>
    public class OrigemTipo : EntidadeTipificacao
    {
        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected OrigemTipo() : base()
        {
        }

        /// <summary>
        /// Construtor para criar um novo tipo de origem
        /// </summary>
        /// <param name="codigo">Código único do tipo</param>
        /// <param name="nome">Nome do tipo</param>
        /// <param name="descricao">Descrição do tipo</param>
        /// <param name="ordem">Ordem de exibição</param>
        /// <param name="icone">Nome do ícone</param>
        public OrigemTipo(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone) : base(codigo, nome, descricao, ordem, icone, null)
        {

        }

        /// <summary>
        /// Construtor para criar um novo tipo de origem
        /// </summary>
        /// <param name="codigo">Código único do tipo</param>
        /// <param name="nome">Nome do tipo</param>
        /// <param name="descricao">Descrição do tipo</param>
        /// <param name="ordem">Ordem de exibição</param>
        /// <param name="icone">Nome do ícone</param>
        public OrigemTipo(
            int id, 
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone,
            DateTime dataCriacao,
            DateTime dataModificacao) : base(codigo, nome, descricao, ordem, icone, null)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
        }

        /// <summary>
        /// Atualiza as propriedades do tipo de origem, incluindo as da classe base
        /// </summary>
        /// <param name="nome">Nome do tipo</param>
        /// <param name="descricao">Descrição do tipo</param>
        /// <param name="ordem">Ordem de exibição</param>
        /// <param name="icone">Nome do ícone</param>
        public void AtualizarPropriedades(string nome, string descricao, int ordem)
        {
            base.Atualizar(nome, descricao, ordem);

            AtualizarDataModificacao();
        }

    }
}