using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa um feedback dado a uma sugestão de IA
    /// </summary>
    public class MensagemSugestaoFeedback
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID da sugestão que recebeu o feedback
        /// </summary>
        public int SugestaoId { get; private set; }

        /// <summary>
        /// ID do usuário que deu o feedback
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// Indica se o feedback foi positivo ou negativo
        /// </summary>
        public bool Positivo { get; private set; }

        /// <summary>
        /// Comentário opcional sobre o feedback
        /// </summary>
        public string Comentario { get; private set; }

        /// <summary>
        /// Data e hora em que o feedback foi dado
        /// </summary>
        public DateTime DataFeedback { get; private set; }

        // Propriedades de navegação
        public virtual MensagemSugestao Sugestao { get; private set; }
        public virtual Usuario.Usuario Usuario { get; private set; }

        // Construtor protegido para EF
        protected MensagemSugestaoFeedback() : base()
        {
        }

        /// <summary>
        /// Cria um novo feedback para uma sugestão
        /// </summary>
        public MensagemSugestaoFeedback(
            int sugestaoId,
            int usuarioId,
            bool positivo,
            string comentario = null) : this()
        {
            if (sugestaoId <= 0)
                throw new DomainException("ID da sugestão deve ser maior que zero", nameof(sugestaoId));

            if (usuarioId <= 0)
                throw new DomainException("ID do usuário deve ser maior que zero", nameof(usuarioId));

            SugestaoId = sugestaoId;
            UsuarioId = usuarioId;
            Positivo = positivo;
            Comentario = comentario;
            DataFeedback = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza o comentário do feedback
        /// </summary>
        public void AtualizarComentario(string comentario)
        {
            Comentario = comentario;
        }
    }
}