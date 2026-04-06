using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Notificacao
{
    public class NotificacaoStatus : EntidadeTipificacao
    {
        /// <summary>
        /// Indica se este status representa um estado final da notificação
        /// </summary>
        public bool StatusFinal { get; private set; }

        // Propriedade de navegação
        public virtual ICollection<Notificacao> Notificacoes { get; private set; }

        // Construtor para EF Core
        protected NotificacaoStatus()
        {
            Notificacoes = new HashSet<Notificacao>();
        }

        /// <summary>
        /// Construtor para criação de novo status de notificação
        /// </summary>
        public NotificacaoStatus(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string cor,
            bool statusFinal = false) : base(codigo, nome, descricao, ordem, null, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            StatusFinal = statusFinal;
            Notificacoes = new HashSet<Notificacao>();
        }

        /// <summary>
        /// Define se o status é final
        /// </summary>
        public void DefinirComoStatusFinal(bool statusFinal)
        {
            StatusFinal = statusFinal;
            AtualizarDataModificacao();
        }
    }
}
