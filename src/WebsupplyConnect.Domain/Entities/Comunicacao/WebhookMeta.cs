using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa um registro de webhook da Meta (WhatsApp)
    /// </summary>
    public class WebhookMeta : EntidadeBase
    {
        /// <summary>
        /// ID externo do webhook na plataforma Meta
        /// </summary>
        public string IdExterno { get; private set; }

        /// <summary>
        /// Data de registro do webhook
        /// </summary>
        public DateTime DataRegistro { get; private set; }

        /// <summary>
        /// Payload completo do webhook (JSON)
        /// </summary>
        public string Payload { get; private set; }

        /// <summary>
        /// Assinatura HMAC para validação de segurança
        /// </summary>
        public string AssinaturaHMAC { get; private set; }

        /// <summary>
        /// Tipo de evento do webhook (mensagem, status, etc.)
        /// </summary>
        public int? WebhookMetaTipoEventoId { get; private set; }

        /// <summary>
        /// Indica se o webhook já foi processado
        /// </summary>
        public bool Processado { get; private set; }

        /// <summary>
        /// ID da conversa associada ao webhook (pode ser null antes do processamento)
        /// </summary>
        public int? ConversaId { get; private set; }

        /// <summary>
        /// Tempo de resposta em milissegundos
        /// </summary>
        public int? TempoRespostaMs { get; private set; }

        // Propriedades de navegação
        public virtual Conversa Conversa { get; private set; }
        public virtual WebhookMetaTipoEvento WebhookMetaTipoEvento { get; private set; }

        // Construtor protegido para EF
        protected WebhookMeta() : base()
        {
        }

        /// <summary>
        /// Cria um novo registro de webhook
        /// </summary>
        public WebhookMeta(
            string idExterno,
            string payload,
            string assinaturaHMAC) : this()
        {
            if (string.IsNullOrWhiteSpace(idExterno))
                throw new DomainException("ID externo não pode ser vazio", nameof(idExterno));

            if (string.IsNullOrWhiteSpace(payload))
                throw new DomainException("Payload não pode ser vazio", nameof(payload));

            IdExterno = idExterno;
            Payload = payload;
            AssinaturaHMAC = assinaturaHMAC;
            DataRegistro = TimeHelper.GetBrasiliaTime();
            Processado = false;
        }

        /// <summary>
        /// Associa o webhook a uma conversa e marca como processado
        /// </summary>
        public void MarcarProcessado(int conversaId)
        {
            if (conversaId <= 0)
                throw new DomainException("ID da conversa deve ser maior que zero", nameof(conversaId));

            ConversaId = conversaId;
            Processado = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Marca o webhook como processado sem associar a uma conversa
        /// </summary>
        public void MarcarProcessado()
        {
            Processado = true;
            AtualizarDataModificacao();
        }
    }
}