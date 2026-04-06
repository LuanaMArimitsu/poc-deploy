using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa uma mensagem em uma conversa
    /// </summary>
    public class Mensagem : EntidadeBase
    {
        /// <summary>
        /// ID da conversa à qual a mensagem pertence
        /// </summary>
        public int ConversaId { get; private set; }

        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Conteudo { get; private set; }

        /// <summary>
        /// Indica de qual sentido a mensagem veio (E = Enviada pelo usuário, R = recebida do cliente)
        /// </summary>
        public char Sentido { get; private set; }

        /// <summary>
        /// ID do usuário que enviou a mensagem (somente para mensagens enviadas)
        /// </summary>
        public int? UsuarioId { get; private set; }

        /// <summary>
        /// Data e hora de envio da mensagem
        /// </summary>
        public DateTime? DataEnvio { get; private set; }

        /// <summary>
        /// Data e hora de recebimento pelo WhatsApp
        /// </summary>
        public DateTime? DataRecebimento { get; private set; }

        /// <summary>
        /// Data e hora de leitura pelo destinatário
        /// </summary>
        public DateTime? DataLeitura { get; private set; }

        /// <summary>
        /// ID externo da mensagem na plataforma Meta
        /// </summary>
        public string? IdExternoMeta { get; private set; }

        /// <summary>
        /// ID do status atual da mensagem
        /// </summary>
        public int? StatusId { get; private set; }

        /// <summary>
        /// ID do tipo da mensagem
        /// </summary>
        public int TipoId { get; private set; }

        /// <summary>
        /// ID do template utilizado (se aplicável)
        /// </summary>
        public int? TemplateId { get; private set; }

        /// <summary>
        /// Indica se a mensagem foi destacada/favorita
        /// </summary>
        public bool Destacada { get; private set; }

        /// <summary>
        /// Indica se foi usado assistente de IA para gerar/editar a mensagem
        /// </summary>
        public bool UsouAssistenteIA { get; private set; }

        /// <summary>
        /// Indica se a mensagem é um aviso para o cliente retormar a conversa
        /// </summary>
        public bool EhAviso { get; private set; }

        // Propriedades de navegação
        public virtual Conversa Conversa { get; private set; }
        public virtual Midia? Midia { get; private set; }
        public virtual MensagemTipo Tipo { get; private set; }
        public virtual MensagemStatus Status { get; private set; }
        public virtual Template Template { get; private set; }
        public virtual Usuario.Usuario Usuario { get; private set; }
        public virtual ICollection<MensagemSugestao> Sugestoes { get; private set; }

        // Construtor protegido para EF
        protected Mensagem() : base()
        {
            Sugestoes = new HashSet<MensagemSugestao>();
        }

        /// <summary>
        /// Cria uma nova mensagem enviada
        /// </summary>
        public Mensagem(
            int conversaId,
            int tipoId,
            int usuarioId,
            bool usouAssistenteIA,
            bool ehAviso,
            string? idExternoMeta,
            string? conteudo,
            DateTime? dataEnvio = null,
            int? statusId = null,
            int? templateId = null) : this()
        {
            if (conversaId <= 0)
                throw new DomainException("ID da conversa deve ser maior que zero", nameof(Conversa));

            if (tipoId <= 0)
                throw new DomainException("ID do tipo deve ser maior que zero", nameof(Conversa));

            if (usuarioId <= 0)
                throw new DomainException("ID do usuário deve ser maior que zero", nameof(Conversa));

            ConversaId = conversaId;
            Conteudo = conteudo ?? string.Empty;
            StatusId = statusId;
            TipoId = tipoId;
            UsuarioId = usuarioId;
            Sentido = 'E';
            DataEnvio = dataEnvio ?? TimeHelper.GetBrasiliaTime();
            IdExternoMeta = idExternoMeta ?? string.Empty;
            TemplateId = templateId;
            UsouAssistenteIA = usouAssistenteIA;
            Destacada = false;
            EhAviso = ehAviso;
        }

        /// <summary>
        /// Cria uma nova mensagem recebida
        /// </summary>
        public static Mensagem CriarMensagemRecebida(
            int conversaId,
            int tipoId,
            string idExternoMeta,
            int statusId,
            string? conteudo = null,
            DateTime? dataEnvio = null)
        {
            var mensagem = new Mensagem();

            if (conversaId <= 0)
                throw new DomainException("ID da conversa deve ser maior que zero", nameof(Conversa));

            if (idExternoMeta == null)
                throw new DomainException("ID externo da meta não pode ser nulo.", nameof(Conversa));

            if (tipoId <= 0)
                throw new DomainException("ID do tipo deve ser maior que zero", nameof(Conversa));

            mensagem.ConversaId = conversaId;
            mensagem.Conteudo = conteudo ?? string.Empty;
            mensagem.StatusId = statusId;
            mensagem.TipoId = tipoId;
            mensagem.Sentido = 'R';
            mensagem.UsuarioId = null;
            mensagem.DataEnvio = dataEnvio ?? TimeHelper.GetBrasiliaTime();
            mensagem.IdExternoMeta = idExternoMeta;
            mensagem.Destacada = false;
            mensagem.UsouAssistenteIA = false;
            mensagem.EhAviso = false;

            return mensagem;
        }

        /// <summary>
        /// Atualiza o conteúdo da mensagem
        /// </summary>
        public void AtualizarConteudo(string conteudo)
        {
            Conteudo = conteudo ?? throw new DomainException("O conteudo da mensagem não pode ser nulo.", nameof(Conversa));
            AtualizarDataModificacao();
        }


        /// <summary>
        /// Atualiza o status da mensagem
        /// </summary>
        public void AtualizarStatus(int statusId)
        {
            if (statusId <= 0)
                throw new DomainException("ID do status deve ser maior que zero", nameof(Conversa));

            StatusId = statusId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra o recebimento da mensagem pelo WhatsApp
        /// </summary>
        public void RegistrarRecebimento(DateTime dataRecebimento)
        {
            DataRecebimento = dataRecebimento;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra a leitura da mensagem pelo destinatário
        /// </summary>
        public void RegistrarLeitura(DateTime dataLeitura)
        {
            DataLeitura = dataLeitura;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra o envio da mensagem pelo WhatsApp
        /// </summary>
        public void RegistrarEnvio(DateTime dataEnvio)
        {
            DataEnvio = dataEnvio;
            AtualizarDataModificacao();
        }


        /// <summary>
        /// Atualiza o ID externo da Meta
        /// </summary>
        public void AtualizarIdExternoMeta(string idExternoMeta)
        {
            IdExternoMeta = idExternoMeta;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Destaca/favorita a mensagem
        /// </summary>
        public void Destacar()
        {
            if (!Destacada)
            {
                Destacada = true;
                AtualizarDataModificacao();
            }
        }

        /// <summary>
        /// Remove o destaque da mensagem
        /// </summary>
        public void RemoverDestaque()
        {
            if (Destacada)
            {
                Destacada = false;
                AtualizarDataModificacao();
            }
        }

    }
}