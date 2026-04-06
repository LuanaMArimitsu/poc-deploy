using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Entidade de tipificação que define os possíveis status de um vendedor na fila de distribuição.
    /// Herda de EntidadeTipificacao para manter o padrão do projeto.
    /// </summary>
    public class StatusFilaDistribuicao : EntidadeTipificacao
    {
        /// <summary>
        /// Indica se este status permite que o vendedor receba leads
        /// </summary>
        public bool PermiteRecebimento { get; private set; }

        // Propriedade de navegação
        public virtual ICollection<FilaDistribuicao> FilasDistribuicao { get; private set; }

        // Construtor protegido para EF Core
        protected StatusFilaDistribuicao() : base()
        {
            FilasDistribuicao = new HashSet<FilaDistribuicao>();
        }

        /// <summary>
        /// Cria um novo status de fila de distribuição
        /// </summary>
        public StatusFilaDistribuicao(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? icone,
            string? cor,
            bool permiteRecebimento) 
            : base(codigo, nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            PermiteRecebimento = permiteRecebimento;
            FilasDistribuicao = new HashSet<FilaDistribuicao>();
        }

        /// <summary>
        /// Cria um novo status de fila de distribuição sem ID predefinido (para uso em runtime)
        /// </summary>
        public StatusFilaDistribuicao(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            bool permiteRecebimento,
            string? icone = null,
            string? cor = null)
            : base(codigo, nome, descricao, ordem, icone, cor)
        {
            PermiteRecebimento = permiteRecebimento;
            FilasDistribuicao = new HashSet<FilaDistribuicao>();
        }

        /// <summary>
        /// Define se o status permite recebimento de leads
        /// </summary>
        public void DefinirPermissaoRecebimento(bool permiteRecebimento)
        {
            PermiteRecebimento = permiteRecebimento;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza as informações do status
        /// </summary>
        public override void Atualizar(string nome, string descricao, int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }

        /// <summary>
        /// Atualiza todas as informações do status incluindo permissão de recebimento
        /// </summary>
        public void AtualizarCompleto(
            string nome, 
            string descricao, 
            int ordem, 
            string icone, 
            string cor, 
            bool permiteRecebimento)
        {
            base.Atualizar(nome, descricao, ordem);
            AtualizarIcone(icone);
            AtualizarCor(cor);
            PermiteRecebimento = permiteRecebimento;
        }
    }
}