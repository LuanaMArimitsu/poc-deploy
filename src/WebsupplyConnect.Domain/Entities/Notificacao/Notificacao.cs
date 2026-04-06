using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Notificacao
{
    public class Notificacao : EntidadeBase
    {
        /// <summary>
        /// Título da notificação
        /// </summary>
        public string Titulo { get; private set; }

        /// <summary>
        /// Conteúdo detalhado da notificação
        /// </summary>
        public string Conteudo { get; private set; }

        /// <summary>
        /// Data e hora de quando a notificação deve ser exibida/enviada
        /// </summary>
        public DateTime DataHora { get; private set; }

        /// <summary>
        /// Data e hora de quando a notificação foi efetivamente enviada
        /// </summary>
        public DateTime? DataEnvio { get; private set; }

        /// <summary>
        /// Data e hora de quando a notificação foi visualizada pelo usuário
        /// </summary>
        public DateTime? DataVisualizacao { get; private set; }

        /// <summary>
        /// ID do usuário que receberá a notificação
        /// </summary>
        public int UsuarioDestinatarioId { get; private set; }

        /// <summary>
        /// ID do usuário que enviou/gerou a notificação (pode ser null para notificações do sistema)
        /// </summary>
        public int? UsuarioRemetenteId { get; private set; }

        /// <summary>
        /// ID do tipo de notificação
        /// </summary>
        public int NotificacaoTipoId { get; private set; }

        /// <summary>
        /// ID do status atual da notificação
        /// </summary>
        public int StatusId { get; private set; }

        /// <summary>
        /// ID da entidade relacionada à notificação (Lead, Conversa, Oportunidade, etc.)
        /// </summary>
        public int? EntidadeAlvoId { get; private set; }

        /// <summary>
        /// Tipo da entidade relacionada (para implementação do padrão polimórfico)
        /// </summary>
        public string TipoEntidadeAlvo { get; private set; }

        /// <summary>
        /// Indica se a notificação foi enviada via push notification
        /// </summary>
        public bool EnviadoPush { get; private set; }

        /// <summary>
        /// Indica se a notificação foi enviada via SignalR (tempo real)
        /// </summary>
        public bool EnviadaSignalR { get; private set; }

        /// <summary>
        /// Indica se a notificação foi enviada via email
        /// </summary>
        public bool EnviadaEmail { get; private set; }

        /// <summary>
        /// Indica se a notificação foi exibida na interface web
        /// </summary>
        public bool ExibidaWeb { get; private set; }

        // Propriedades de navegação
        public virtual Usuario.Usuario UsuarioDestinatario { get; private set; }
        public virtual Usuario.Usuario UsuarioRemetente { get; private set; }
        public virtual NotificacaoTipo NotificacaoTipo { get; private set; }
        public virtual NotificacaoStatus Status { get; private set; }

        // Construtor para EF Core
        protected Notificacao() { }

        /// <summary>
        /// Construtor para criação de nova notificação
        /// </summary>
        public Notificacao(
            string titulo,
            string conteudo,
            int usuarioDestinatarioId,
            int notificacaoTipoId,
            int statusId,
            DateTime? dataHora = null,
            int? usuarioRemetenteId = null,
            int? entidadeAlvoId = null,
            string tipoEntidadeAlvo = null)
        {
            ValidarParametros(titulo, conteudo, usuarioDestinatarioId, notificacaoTipoId, statusId);

            Titulo = titulo;
            Conteudo = conteudo;
            UsuarioDestinatarioId = usuarioDestinatarioId;
            NotificacaoTipoId = notificacaoTipoId;
            StatusId = statusId;
            DataHora = dataHora ?? TimeHelper.GetBrasiliaTime();
            UsuarioRemetenteId = usuarioRemetenteId;
            EntidadeAlvoId = entidadeAlvoId;
            TipoEntidadeAlvo = tipoEntidadeAlvo ?? string.Empty;

            // Inicializa flags de envio como false
            EnviadoPush = false;
            EnviadaSignalR = false;
            EnviadaEmail = false;
            ExibidaWeb = false;
        }

        /// <summary>
        /// Marca a notificação como exibida na web
        /// </summary>
        public void MarcarSignalR(bool atividade)
        {
            EnviadaSignalR = atividade;
        }

        /// <summary>
        /// Marca a notificação como exibida na web
        /// </summary>
        public void MarcarPush(bool atividade)
        {
            EnviadoPush = atividade;
        }

        /// <summary>
        /// Marca a notificação como exibida na web
        /// </summary>
        public void MarcarComoExibidaWeb()
        {
            ExibidaWeb = true;
        }

        /// <summary>
        /// Marca a notificação como visualizada pelo usuário
        /// </summary>
        public void MarcarComoVisualizada(DateTime date)
        {
            DataVisualizacao = date;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o status da notificação
        /// </summary>
        public void AtualizarStatus(int novoStatusId)
        {
            if (novoStatusId <= 0)
                throw new DomainException("Status ID deve ser maior que zero", nameof(Notificacao));

            StatusId = novoStatusId;
            AtualizarDataModificacao();
        }

        private void ValidarParametros(string titulo, string conteudo, int usuarioDestinatarioId, int notificacaoTipoId, int statusId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new DomainException("Título da notificação é obrigatório", nameof(Notificacao));

            if (string.IsNullOrWhiteSpace(conteudo))
                throw new DomainException("Conteúdo da notificação é obrigatório", nameof(Notificacao));

            if (usuarioDestinatarioId <= 0)
                throw new DomainException("Usuário destinatário é obrigatório", nameof(Notificacao));

            if (notificacaoTipoId <= 0)
                throw new DomainException("Tipo de notificação é obrigatório", nameof(Notificacao));

            if (statusId <= 0)
                throw new DomainException("Status da notificação é obrigatório", nameof(Notificacao));
        }

    }
}
