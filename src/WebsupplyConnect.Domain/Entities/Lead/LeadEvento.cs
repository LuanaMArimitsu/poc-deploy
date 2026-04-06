using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    public class LeadEvento : EntidadeBase
    {
        public int LeadId { get; private set; }

        /// <summary>
        /// ID da Origem (Instagram, WhatsApp, Website, etc.)
        /// Origem tipificada do sistema
        /// </summary>
        public int OrigemId { get; private set; }

        /// <summary>
        /// Data/hora deste evento
        /// </summary>
        public DateTime DataEvento { get; private set; }

        public int? CanalId { get; private set; }

        public int? CampanhaId { get; private set; }

        public string? Observacao { get; private set; }

        public virtual Lead Lead { get; private set; }
        public virtual Origem Origem { get; private set; }
        public virtual Canal Canal { get; private set; }
        public virtual Campanha Campanha { get; private set; }

        public virtual ICollection<Oportunidade.Oportunidade> Oportunidades { get; private set; }

        protected LeadEvento() { }

        public LeadEvento(
            int leadId,
            int origemId,
            int? canalId = null,
            int? campanhaId = null,
            string? campanhaCodigo = null,
            string? observacao = null)
        {
            if (leadId <= 0)
                throw new DomainException("LeadId inválido", nameof(LeadEvento));

            if (origemId <= 0)
                throw new DomainException("OrigemId inválido", nameof(LeadEvento));

            LeadId = leadId;
            OrigemId = origemId;
            DataEvento = TimeHelper.GetBrasiliaTime();
            CanalId = canalId;
            CampanhaId = campanhaId;
            Observacao = observacao;
        }

        /// <summary>
        /// Associa uma campanha que foi criada posteriormente
        /// Usado na reconciliação de campanhas temporárias
        /// </summary>
        public void AssociarCampanha(int campanhaId)
        {
            if (campanhaId <= 0)
                throw new DomainException("CampanhaId inválido", nameof(LeadEvento));

            CampanhaId = campanhaId;

                if (string.IsNullOrWhiteSpace(Observacao))
                    Observacao = "Campanha transferida";
               else
                   Observacao = $"{Observacao}; Campanha transferida";

            AtualizarDataModificacao();
        }

        public void AtualizarEvento(int? origemId, int? canalId, int? campanhaId, string observacaoAtualizada)
        {
             if (origemId <= 0)
                throw new DomainException("OrigemId inválido", nameof(LeadEvento));

            if (origemId.HasValue)
                OrigemId = origemId.Value;

            if (canalId.HasValue)
                CanalId = canalId.Value;

            if (campanhaId.HasValue)
                CampanhaId = campanhaId.Value;

            if (!string.IsNullOrWhiteSpace(observacaoAtualizada))
            {
                Observacao = observacaoAtualizada;
            }

            AtualizarDataModificacao();
        }
    }
}
