using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Empresa
{
    public class TipoPromptEmpresas : EntidadeTipificacao
    {

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected TipoPromptEmpresas() : base()
        {
        }

        public TipoPromptEmpresas(
            string codigo,
            string nome,
            string descricao,
            int ordem) : base(codigo, nome, descricao, ordem, null, null)
        {

        }

        public TipoPromptEmpresas(
            int id,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            DateTime dataCriacao,
            DateTime dataModificacao) : base(codigo, nome, descricao, ordem, null, null)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
        }

        /// <summary>
        /// Atualiza as propriedades do tipo de prompt, incluindo as da classe base
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
