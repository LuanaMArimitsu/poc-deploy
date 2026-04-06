using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa o status de uma mensagem
    /// </summary>
    public class MensagemStatus : EntidadeTipificacao
    {
        /// <summary>
        /// Indica se é um status final (não haverá mais transições)
        /// </summary>
        public bool FinalStatus { get; private set; }

        // Construtor protegido para EF
        protected MensagemStatus() : base()
        {
        }

        /// <summary>
        /// Cria um novo status de mensagem
        /// </summary>
        public MensagemStatus(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            bool finalStatus = false) : base(codigo, nome, descricao, ordem, icone, null)
        {
            FinalStatus = finalStatus;
        }

        /// <summary>
        /// Cria um novo status de mensagem Seed
        /// </summary>
        public MensagemStatus(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            bool finalStatus = false) : base(codigo, nome, descricao, ordem, icone, null)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            FinalStatus = finalStatus;
        }

        /// <summary>
        /// Atualiza as informações do status
        /// </summary>
        public override void Atualizar(
            string nome,
            string descricao,
            int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }

        /// <summary>
        /// Atualiza as propriedades específicas do status de mensagem
        /// </summary>
        public void AtualizarPropriedadesEspecificas(bool finalStatus)
        {
            FinalStatus = finalStatus;
            AtualizarDataModificacao();
        }
    }
}