using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa uma mídia associada a uma mensagem
    /// </summary>
    public class Midia : EntidadeBase
    {
        /// <summary>
        /// ID da mensagem associada (opcional, pois a mídia pode ser criada antes da mensagem)
        /// </summary>
        public int MensagemId { get; private set; }

        /// <summary>
        /// Conteúdo de mensagem enviado junto a mídia.
        /// </summary>
        public string? Caption { get; private set; }

        /// <summary>
        /// Nome do arquivo de mídia
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Formato do arquivo (extensão)
        /// </summary>
        public string Formato { get; private set; }

        /// <summary>
        /// Tamanho do arquivo em bytes
        /// </summary>
        public long? TamanhoBytes { get; private set; }

        /// <summary>
        /// URL para acesso à mídia no storage
        /// </summary>
        public string? UrlStorage { get; private set; }

        /// <summary>
        /// URL para acesso à thumbnail da mídia no storage
        /// </summary>
        public string? ThumbnailUrlStorage { get; private set; }

        /// <summary>
        /// ID externo da mídia na plataforma Meta
        /// </summary>
        public string? IdExternoMeta { get; private set; }

        /// <summary>
        /// ID do blob no sistema de armazenamento
        /// </summary>
        public string BlobId { get; private set; }

        /// <summary>
        /// Nome do container no sistema de armazenamento
        /// </summary>
        public string ContainerName { get; private set; }

        /// <summary>
        /// ID do status de processamento da mídia
        /// </summary>
        public int MidiaStatusProcessamentoId { get; private set; }

        /// <summary>
        /// Transcrição gerada a partir do áudio da mídia
        /// </summary>
        public string? Transcricao { get; private set; }

        // Propriedades de navegação
        public virtual Mensagem Mensagem { get; private set; }

        public virtual MidiaStatusProcessamento MidiaStatusProcessamento { get; private set; }

        // Construtor protegido para EF
        protected Midia() : base()
        {
        }

        /// <summary>
        /// Cria uma nova mídia
        /// </summary>
        public Midia(
            string nome,
            string blobId,
            string containerName,
            int midiaStatusProcessamentoId,
            int mensagemId,
            string formato,
            string? caption = null,
            long? tamanhoBytes = null,
            string? urlStorage = null,
            string? thumbnailUrlStorage = null,
            string? idExternoMeta = null) : this()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da mídia não pode ser vazio", nameof(nome));

            if (string.IsNullOrWhiteSpace(blobId))
                throw new DomainException("BlobId não pode ser vazio", nameof(blobId));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new DomainException("Nome do container não pode ser vazio", nameof(containerName));

            if (midiaStatusProcessamentoId <= 0)
                throw new DomainException("ID do status de processamento deve ser maior que zero", nameof(midiaStatusProcessamentoId));

            Nome = nome;
            Formato = formato ?? string.Empty;
            TamanhoBytes = tamanhoBytes;
            BlobId = blobId;
            ContainerName = containerName;
            MidiaStatusProcessamentoId = midiaStatusProcessamentoId;
            UrlStorage = urlStorage ?? string.Empty;
            ThumbnailUrlStorage = thumbnailUrlStorage ?? string.Empty;
            IdExternoMeta = idExternoMeta;
            MensagemId = mensagemId;
            Caption = caption;
        }

        public void AtualizarMidiaDadosMeta(long tamanhoBytes, string idExternoMeta)
        {
            if (tamanhoBytes <= 0)
                throw new DomainException("Tamanho de bytes não pode ser nulo.", nameof(tamanhoBytes));
            if (String.IsNullOrEmpty(idExternoMeta))
            {
                throw new DomainException("O Id externo da mídia vindo da meta não pode ser nulo.");
            }

            TamanhoBytes = tamanhoBytes;
            IdExternoMeta = idExternoMeta;
            AtualizarDataModificacao();
        }
        /// <summary>
        /// Associa a mídia a uma mensagem
        /// </summary>
        public void AssociarMensagem(int mensagemId)
        {
            if (mensagemId <= 0)
                throw new DomainException("ID da mensagem deve ser maior que zero", nameof(mensagemId));

            MensagemId = mensagemId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o status de processamento da mídia
        /// </summary>
        public void AtualizarStatusProcessamento(int midiaStatusProcessamentoId)
        {
            if (MidiaStatusProcessamentoId <= 0)
                throw new DomainException("ID do status de processamento deve ser maior que zero", nameof(MidiaStatusProcessamentoId));

            MidiaStatusProcessamentoId = midiaStatusProcessamentoId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza as URLs de acesso à mídia no storage
        /// </summary>
        public void AtualizarUrlsStorage(string urlStorage, string? thumbnailUrlStorage = null)
        {
            if (string.IsNullOrWhiteSpace(urlStorage))
                throw new DomainException("URL de storage não pode ser vazia", nameof(urlStorage));

            UrlStorage = urlStorage;
            ThumbnailUrlStorage = thumbnailUrlStorage;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o ID externo da mídia na plataforma Meta
        /// </summary>
        public void AtualizarIdExternoMeta(string idExternoMeta)
        {
            IdExternoMeta = idExternoMeta;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra a transcrição do áudio desta mídia
        /// </summary>
        public void RegistrarTranscricao(string transcricao)
        {
            if (string.IsNullOrWhiteSpace(transcricao))
                throw new DomainException("Transcrição não pode ser vazia.", nameof(transcricao));

            if (!string.IsNullOrWhiteSpace(Transcricao))
                throw new DomainException("Esta mídia já possui uma transcrição registrada.", nameof(Transcricao));

            Transcricao = transcricao;
            AtualizarDataModificacao();
        }
    }
}
