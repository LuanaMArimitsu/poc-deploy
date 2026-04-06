using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa o status de processamento de uma mídia
    /// </summary>
    public class MidiaStatusProcessamento : EntidadeTipificacao
    {
        /// <summary>
        /// Indica se o processamento está finalizado
        /// </summary>
        public bool Finalizado { get; private set; }

        // Construtor protegido para EF
        protected MidiaStatusProcessamento() : base()
        {
        }

        /// <summary>
        /// Cria um novo status de processamento de mídia
        /// </summary>
        public MidiaStatusProcessamento(
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            bool finalizado = false) : base(codigo, nome, descricao, ordem, icone, null)
        {
            Finalizado = finalizado;
        }

        /// <summary>
        /// Cria um novo seed status de processamento de mídia
        /// </summary>
        public MidiaStatusProcessamento(
            int id,
            DateTime dataCriacao,
            DateTime dataModificacao,
            string codigo,
            string nome,
            string descricao,
            int ordem,
            string icone = null,
            bool finalizado = false) : base(codigo, nome, descricao, ordem, icone, null)
        {
            Id = id;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
            Finalizado = finalizado;
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
        /// Atualiza as propriedades específicas do status de processamento
        /// </summary>
        public void AtualizarPropriedadesEspecificas(
            bool finalizado)
        {
            Finalizado = finalizado;
            AtualizarDataModificacao();
        }
    }
}