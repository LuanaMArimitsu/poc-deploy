using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    public class TemplateOrigem : EntidadeBase
    {
        public int TemplateId { get; private set; }
        public int OrigemId { get; private set; }
        // Propriedades de navegação
        public virtual Template Template { get; private set; }
        public virtual Origem Origem { get; private set; }
        // Construtor protegido para EF
        protected TemplateOrigem() : base() { }
        /// <summary>
        /// Cria uma nova associação entre template e origem
        /// </summary>
        public TemplateOrigem(int templateId, int origemId)
        {
            TemplateId = templateId;
            OrigemId = origemId;
        }

        public void Atualizar(int templateId, int origemId)
        {
            TemplateId = templateId;
            OrigemId = origemId;
            AtualizarDataModificacao();
        }

    }
}
