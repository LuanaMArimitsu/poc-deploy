using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa uma conversa entre um lead e um usuário através de um canal
    /// </summary>
    public class Conversa : EntidadeBase
    {
        /// <summary>
        /// Título da conversa
        /// </summary>
        public string Titulo { get; private set; }

        /// <summary>
        /// ID do lead associado à conversa
        /// </summary>
        public int LeadId { get; private set; }

        ///// <summary>
        ///// ID da oportunidade associada à conversa (opcional)
        ///// </summary>
        //public int? OportunidadeId { get; private set; }

        /// <summary>
        /// ID do usuário responsável pela conversa
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// ID do canal utilizado na conversa
        /// </summary>
        public int CanalId { get; private set; }

        /// <summary>
        /// ID do status atual da conversa
        /// </summary>
        public int StatusId { get; private set; }

        /// <summary>
        /// ID do status atual da conversa
        /// </summary>
        public int JanelaId { get; private set; }

        /// <summary>
        /// Data de início da conversa
        /// </summary>
        public DateTime DataInicio { get; private set; }

        /// <summary>
        /// Data da última mensagem na conversa
        /// </summary>
        public DateTime? DataUltimaMensagem { get; private set; }

        /// <summary>
        /// ID externo da conversa na plataforma Meta
        /// </summary>
        public string? IdExternoMeta { get; private set; }

        /// <summary>
        /// Indica se a conversa possui mensagens não lidas
        /// </summary>
        public bool PossuiMensagensNaoLidas { get; private set; }

        /// <summary>
        ///  Indica a qual equipe a conversa pertence
        /// </summary>
        public int? EquipeId { get; private set; }

        /// <summary>
        /// Indica se a conversa está fixada ou não
        /// </summary>
        public bool Fixada { get; private set; } = false;

        // Propriedades de navegação
        public virtual Canal Canal { get; private set; }
        public virtual Usuario.Usuario Usuario { get; private set; }
        public virtual ConversaStatus Status { get; private set; }
        public virtual Lead.Lead Lead { get; private set; }
        public virtual ICollection<Mensagem> Mensagens { get; private set; }
        public virtual ICollection<WebhookMeta> WebhooksMeta { get; private set; }
        public virtual Domain.Entities.Equipe.Equipe Equipe { get; private set; }

        // Construtor protegido para EF
        protected Conversa() : base()
        {
            Mensagens = new HashSet<Mensagem>();
            WebhooksMeta = new HashSet<WebhookMeta>();
        }

        /// <summary>
        /// Cria uma nova conversa
        /// </summary>
        public Conversa(
            string titulo,
            int leadId,
            int usuarioId,
            int canalId,
            int statusId,
            int equipeId,
            string? idExternoMeta = null) : this()
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new DomainException("Título da conversa não pode ser vazio", nameof(Conversa));

            if (leadId <= 0)
                throw new DomainException("ID do lead deve ser maior que zero", nameof(Conversa));

            if (usuarioId <= 0)
                throw new DomainException("ID do usuário deve ser maior que zero", nameof(Conversa));

            if (canalId <= 0)
                throw new DomainException("ID do canal deve ser maior que zero", nameof(Conversa));

            if (statusId <= 0)
                throw new DomainException("ID do status deve ser maior que zero", nameof(Conversa));

            if(equipeId <= 0)
                throw new DomainException("ID da equipe deve ser maior que zero", nameof(Conversa));

            Titulo = titulo;
            LeadId = leadId;
            UsuarioId = usuarioId;
            CanalId = canalId;
            StatusId = statusId;
            //OportunidadeId = oportunidadeId;
            IdExternoMeta = idExternoMeta ?? string.Empty;
            DataInicio = TimeHelper.GetBrasiliaTime();
            DataUltimaMensagem = null;
            PossuiMensagensNaoLidas = false;
            EquipeId = equipeId;
        }

        /// <summary>
        /// Atualiza o id conversa externo da Meta
        /// </summary>
        public void AtualizarIdMetaConversa(string idExternoMeta)
        {
            if (string.IsNullOrWhiteSpace(idExternoMeta))
                throw new DomainException("O Id externo da meta não pode ser vazio", nameof(Conversa));

            IdExternoMeta = idExternoMeta;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o título da conversa
        /// </summary>
        public void AtualizarTitulo(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new DomainException("Título da conversa não pode ser vazio", nameof(Conversa));

            Titulo = titulo;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o responsável pela conversa
        /// </summary>
        public void AtualizarResponsavel(int usuarioId, int canalId, int equipeId)
        {
            if (usuarioId <= 0)
                throw new DomainException("ID do usuário deve ser maior que zero", nameof(Conversa));
            if (canalId <= 0)
                throw new DomainException("ID do canal deve ser maior que zero", nameof(Conversa));
            if(equipeId <= 0)
                throw new DomainException("ID da equipe deve ser maior que zero", nameof(Conversa));

            UsuarioId = usuarioId;
            CanalId = canalId;
            EquipeId = equipeId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o status da conversa
        /// </summary>
        public void AtualizarStatus(int statusId)
        {
            if (statusId <= 0)
                throw new DomainException("ID do status deve ser maior que zero", nameof(Conversa));

            StatusId = statusId;
            AtualizarDataModificacao();
        }

        ///// <summary>
        ///// Associa uma oportunidade à conversa
        ///// </summary>
        //public void AssociarOportunidade(int oportunidadeId)
        //{
        //    if (oportunidadeId <= 0)
        //        throw new DomainException("ID da oportunidade deve ser maior que zero", nameof(oportunidadeId));

        //    OportunidadeId = oportunidadeId;
        //    AtualizarDataUltimaModificacao();
        //}

        ///// <summary>
        ///// Desassocia a oportunidade da conversa
        ///// </summary>
        //public void DesassociarOportunidade()
        //{
        //    OportunidadeId = null;
        //    AtualizarDataUltimaModificacao();
        //}

        /// <summary>
        /// Marca todas as mensagens como lidas
        /// </summary>
        public void MarcarTodasMensagensComoLidas()
        {
            if (PossuiMensagensNaoLidas)
            {
                PossuiMensagensNaoLidas = false;
                AtualizarDataModificacao();
            }
        }

        public void AtualizarPossuiMensagensNaoLidas(bool possuiMensagensNaoLidas)
        {
            PossuiMensagensNaoLidas = possuiMensagensNaoLidas;
            AtualizarDataModificacao();
        }   

        /// <summary>
        /// Atualiza informações da última mensagem
        /// </summary>
        public void AtualizarUltimaMensagem(DateTime dataUltimaMensagem, bool possuiMensagensNaoLidas = false)
        {
            DataUltimaMensagem = dataUltimaMensagem;
            PossuiMensagensNaoLidas = possuiMensagensNaoLidas;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza a data da última mensagem
        /// </summary>
        public void AtualizarDataUltimaMensagem(DateTime dataUltimaMensagem)
        {
            DataUltimaMensagem = dataUltimaMensagem;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Fixar uma conversa
        /// </summary>
        public void Fixar()
        {
            Fixada = true;
        }

        /// <summary>
        /// Desafixar uma conversa
        /// </summary>
        public void Desafixar()
        {
            Fixada = false;
        }
    }
}
