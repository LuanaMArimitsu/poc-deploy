using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa o status de uma conversa
    /// </summary>
    public class ConversaStatus : EntidadeTipificacao
    {
        // Construtor protegido para EF
        protected ConversaStatus() : base()
        {
        }

        /// <summary>
        /// Cria um novo status de conversa
        /// </summary>
        public ConversaStatus(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? cor,
            string? icone) : base(codigo.ToUpperInvariant(), nome, descricao, ordem, icone, cor)
        {

        }

        /// <summary>
        /// Cria um novo seed status de conversa 
        /// </summary>
        public ConversaStatus(
            int id,
            DateTime dataCricao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string? cor,
            string? icone) : base(codigo.ToUpperInvariant(), nome, descricao, ordem, icone, cor)
        {
            Id = id;
            DataCriacao = dataCricao;
            DataModificacao = dataModificacao;
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

    }
}