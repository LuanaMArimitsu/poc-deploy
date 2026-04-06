using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    /// <summary>
    /// Entidade que representa o histórico de mudanças de status de um lead.
    /// </summary>
    public class LeadStatusHistorico
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID do lead relacionado
        /// </summary>
        public int LeadId { get; private set; }

        /// <summary>
        /// ID do status anterior
        /// </summary>
        public int? StatusAnteriorId { get; private set; }

        /// <summary>
        /// ID do novo status
        /// </summary>
        public int StatusNovoId { get; private set; }

        /// <summary>
        /// Data e hora em que o lead entrou no status anterior
        /// </summary>
        public DateTime DataInicio { get; private set; }

        /// <summary>
        /// Data e hora da mudança para o novo status
        /// </summary>
        public DateTime DataMudanca { get; private set; }

        /// <summary>
        /// ID do usuário responsável pela mudança (opcional)
        /// </summary>
        public int? ResponsavelId { get; private set; }

        /// <summary>
        /// Observação sobre a mudança de status
        /// </summary>
        public string Observacao { get; private set; }

        /// <summary>
        /// Navegação para o lead relacionado
        /// </summary>
        public virtual Lead Lead { get; private set; }

        /// <summary>
        /// Navegação para o status anterior
        /// </summary>
        public virtual LeadStatus StatusAnterior { get; private set; }

        /// <summary>
        /// Navegação para o novo status
        /// </summary>
        public virtual LeadStatus StatusNovo { get; private set; }

        /// <summary>
        /// Navegação para o usuário responsável
        /// </summary>
        public virtual Usuario.Usuario Responsavel { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected LeadStatusHistorico() : base()
        {
        }

        /// <summary>
        /// Construtor para criar um novo registro de histórico de status
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <param name="statusAnteriorId">ID do status anterior</param>
        /// <param name="statusNovoId">ID do novo status</param>
        /// <param name="dataMudanca">Data e hora da mudança</param>
        /// <param name="responsavelId">ID do usuário responsável (opcional)</param>
        /// <param name="observacao">Observação sobre a mudança</param>
        public LeadStatusHistorico(
            int leadId,
            int statusAnteriorId,
            int statusNovoId,
            DateTime dataMudanca,
            int? responsavelId = null,
            string observacao = null) : base()
        {
            ValidarDominio(leadId, statusAnteriorId, statusNovoId, dataMudanca, responsavelId);

            LeadId = leadId;
            StatusAnteriorId = statusAnteriorId;
            StatusNovoId = statusNovoId;
            DataInicio = TimeHelper.GetBrasiliaTime().AddSeconds(-1); // Convenção para considerar o momento imediatamente anterior
            DataMudanca = dataMudanca;
            ResponsavelId = responsavelId;
            Observacao = observacao;
        }

        /// <summary>
        /// Atualiza a observação sobre a mudança de status
        /// </summary>
        /// <param name="observacao">Nova observação</param>
        public void AtualizarObservacao(string observacao)
        {
            Observacao = observacao;

            DataMudanca = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Calcula a duração que o lead permaneceu no status anterior
        /// </summary>
        /// <returns>TimeSpan representando a duração</returns>
        public TimeSpan CalcularDuracaoNoStatusAnterior()
        {
            return DataMudanca - DataInicio;
        }

        /// <summary>
        /// Valida as regras de domínio para o histórico de status
        /// </summary>
        private void ValidarDominio(int leadId, int statusAnteriorId, int statusNovoId, DateTime dataMudanca, int? responsavelId)
        {
            if (leadId <= 0)
                throw new DomainException("O ID do lead deve ser maior que zero.", nameof(LeadStatusHistorico));

            if (statusAnteriorId <= 0)
                throw new DomainException("O ID do status anterior deve ser maior que zero.", nameof(LeadStatusHistorico));

            if (statusNovoId <= 0)
                throw new DomainException("O ID do novo status deve ser maior que zero.", nameof(LeadStatusHistorico));

            if (dataMudanca > DateTime.UtcNow)
                throw new DomainException("A data da mudança não pode ser futura.", nameof(LeadStatusHistorico));

            if (responsavelId.HasValue && responsavelId.Value <= 0)
                throw new DomainException("O ID do responsável deve ser maior que zero.", nameof(LeadStatusHistorico));
        }
    }
}