using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Notificacao
{
    public class NotificacaoTipo : EntidadeTipificacao
    {
        /// <summary>
        /// Categoria do tipo de notificação (ex: "sistema", "vendas", "suporte")
        /// </summary>
        public string Categoria { get; private set; }

        /// <summary>
        /// Origem do sistema que gera este tipo de notificação
        /// </summary>
        public string OrigemSistema { get; private set; }

        /// <summary>
        /// Indica se este tipo de notificação deve ser exibido na interface web
        /// </summary>
        public bool AtivoParaWeb { get; private set; }

        /// <summary>
        /// Indica se este tipo de notificação deve ser enviado para o app mobile
        /// </summary>
        public bool AtivoParaMobile { get; private set; }

        // Propriedade de navegação
        public virtual ICollection<Notificacao> Notificacoes { get; private set; }

        // Construtor para EF Core
        protected NotificacaoTipo()
        {
            Notificacoes = new HashSet<Notificacao>();
        }

        /// <summary>
        /// Construtor para criação de novo tipo de notificação
        /// </summary>
        public NotificacaoTipo(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            string cor = null,
            string categoria = null,
            string origemSistema = "Sistema",
            bool ativoParaWeb = true,
            bool ativoParaMobile = true) : base(codigo, nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            Categoria = categoria;
            OrigemSistema = origemSistema;
            AtivoParaWeb = ativoParaWeb;
            AtivoParaMobile = ativoParaMobile;

            Notificacoes = new HashSet<Notificacao>();
        }

        /// <summary>
        /// Atualiza a disponibilidade do tipo de notificação por plataforma
        /// </summary>
        public void AtualizarDisponibilidadePlataforma(bool ativoParaWeb, bool ativoParaMobile)
        {
            AtivoParaWeb = ativoParaWeb;
            AtivoParaMobile = ativoParaMobile;
            AtualizarDataModificacao();
        }
    }
}
