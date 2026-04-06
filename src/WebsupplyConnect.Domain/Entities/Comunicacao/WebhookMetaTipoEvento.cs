using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    public class WebhookMetaTipoEvento : EntidadeTipificacao
    {
        // Construtor protegido para EF
        protected WebhookMetaTipoEvento() : base()
        {

        }

        /// <summary>
        /// Cria um novo tipo de Webhook Meta
        /// </summary>
        public WebhookMetaTipoEvento(int id, string codigo, string nome, string descricao, int ordem, DateTime dataCriacao, DateTime dataModificacao)
        {
            Id = id;
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            DataCriacao = dataCriacao;
            DataModificacao = dataModificacao;
        }

        /// <summary>
        /// Atualiza as informações do tipo
        /// </summary>
        public override void Atualizar(string nome, string descricao, int ordem)
        {
            base.Atualizar(nome, descricao, ordem);
        }
    }
}
