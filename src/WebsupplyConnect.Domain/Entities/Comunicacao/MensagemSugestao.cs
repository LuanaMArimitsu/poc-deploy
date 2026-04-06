using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa uma sugestão de mensagem gerada por IA
    /// </summary>
    public class MensagemSugestao
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID da mensagem associada à sugestão
        /// </summary>
        public int MensagemId { get; private set; }

        /// <summary>
        /// Texto original que gerou a sugestão
        /// </summary>
        public string TextoOriginal { get; private set; }

        /// <summary>
        /// Texto sugerido pela IA
        /// </summary>
        public string TextoSugerido { get; private set; }

        /// <summary>
        /// Tipo de sugestão (reescrita, correção, etc.)
        /// </summary>
        public string Tipo { get; private set; }

        /// <summary>
        /// Foco da sugestão (clareza, persuasão, tom, etc.)
        /// </summary>
        public string Foco { get; private set; }

        /// <summary>
        /// Indica se a sugestão foi selecionada pelo usuário
        /// </summary>
        public bool Selecionada { get; private set; }

        /// <summary>
        /// Pontuação/ranking da sugestão (quanto maior, melhor)
        /// </summary>
        public int Pontuacao { get; private set; }

        // Propriedades de navegação
        public virtual Mensagem Mensagem { get; private set; }
        public virtual ICollection<MensagemSugestaoFeedback> Feedbacks { get; private set; }

        // Construtor protegido para EF
        protected MensagemSugestao() : base()
        {
            Feedbacks = new HashSet<MensagemSugestaoFeedback>();
        }

        /// <summary>
        /// Cria uma nova sugestão de mensagem
        /// </summary>
        public MensagemSugestao(
            int mensagemId,
            string textoOriginal,
            string textoSugerido,
            string tipo,
            string foco,
            int pontuacao) : this()
        {
            if (mensagemId <= 0)
                throw new DomainException("ID da mensagem deve ser maior que zero", nameof(mensagemId));

            if (string.IsNullOrWhiteSpace(textoOriginal))
                throw new DomainException("Texto original não pode ser vazio", nameof(textoOriginal));

            if (string.IsNullOrWhiteSpace(textoSugerido))
                throw new DomainException("Texto sugerido não pode ser vazio", nameof(textoSugerido));

            MensagemId = mensagemId;
            TextoOriginal = textoOriginal;
            TextoSugerido = textoSugerido;
            Tipo = tipo;
            Foco = foco;
            Pontuacao = pontuacao;
            Selecionada = false;
        }

        /// <summary>
        /// Marca a sugestão como selecionada
        /// </summary>
        public void Selecionar()
        {
            if (!Selecionada)
            {
                Selecionada = true;
            }
        }

        /// <summary>
        /// Desmarca a sugestão como selecionada
        /// </summary>
        public void Desselecionar()
        {
            if (Selecionada)
            {
                Selecionada = false;
            }
        }

        /// <summary>
        /// Atualiza a pontuação da sugestão
        /// </summary>
        public void AtualizarPontuacao(int pontuacao)
        {
            Pontuacao = pontuacao;
        }
    }
}