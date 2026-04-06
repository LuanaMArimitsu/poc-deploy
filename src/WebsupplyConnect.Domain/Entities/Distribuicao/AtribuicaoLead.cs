using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Registra a atribuição de um lead a um vendedor, seja por distribuição automática
    /// ou atribuição manual.
    /// </summary>
    public class AtribuicaoLead : EntidadeBase
    {
        /// <summary>
        /// ID do lead atribuído
        /// </summary>
        public int LeadId { get; private set; }

        /// <summary>
        /// ID do vendedor que recebeu o lead
        /// </summary>
        public int MembroAtribuidoId { get; private set; }

        /// <summary>
        /// ID do tipo de atribuição
        /// </summary>
        public int TipoAtribuicaoId { get; private set; }

        /// <summary>
        /// Data e hora da atribuição
        /// </summary>
        public DateTime DataAtribuicao { get; private set; }

        /// <summary>
        /// Motivo/descrição da atribuição
        /// </summary>
        public string MotivoAtribuicao { get; private set; }

        /// <summary>
        /// Indica se foi uma atribuição automática ou manual
        /// </summary>
        public bool AtribuicaoAutomatica { get; private set; }

        /// <summary>
        /// ID da configuração de distribuição utilizada (para atribuição automática)
        /// </summary>
        public int? ConfiguracaoDistribuicaoId { get; private set; }

        /// <summary>
        /// ID da regra que foi decisiva na atribuição (para atribuição automática)
        /// </summary>
        public int? RegraDistribuicaoId { get; private set; }

        /// <summary>
        /// Score calculado para o vendedor (para atribuição automática)
        /// </summary>
        public decimal? ScoreVendedor { get; private set; }

        /// <summary>
        /// ID do usuário que fez a atribuição manual (para atribuição manual)
        /// </summary>
        public int? MembroAtribuiuId { get; private set; }

        /// <summary>
        /// ID do histórico de status do lead relacionado a esta atribuição
        /// </summary>
        public int? LeadStatusHistoricoId { get; private set; }

        /// <summary>
        /// JSON com parâmetros aplicados na atribuição
        /// </summary>
        public string? ParametrosAplicados { get; private set; }

        /// <summary>
        /// JSON com lista de vendedores que eram elegíveis para receber o lead
        /// </summary>
        public string? VendedoresElegiveis { get; private set; }

        /// <summary>
        /// JSON com scores calculados para os vendedores elegíveis
        /// </summary>
        public string? ScoresCalculados { get; private set; }

        /// <summary>
        /// Indica se o fallback de horário foi aplicado (quando não há vendedores no horário atual)
        /// </summary>
        public bool FallbackHorarioAplicado { get; private set; }

        /// <summary>
        /// Detalhes sobre o fallback de horário aplicado
        /// </summary>
        public string? DetalhesFallbackHorario { get; private set; }

        /// <summary>
        /// Data e hora quando o fallback foi aplicado
        /// </summary>
        public DateTime? DataFallbackHorario { get; private set; }

        // Propriedades de navegação
        public virtual Lead.Lead Lead { get; private set; }
        public virtual MembroEquipe MembroAtribuido { get; private set; }
        public virtual TipoAtribuicaoLead TipoAtribuicao { get; private set; }
        public virtual ConfiguracaoDistribuicao? ConfiguracaoDistribuicao { get; private set; }
        public virtual RegraDistribuicao? RegraDistribuicao { get; private set; }
        public virtual MembroEquipe? MembroAtribuiu { get; private set; }
        public virtual Lead.LeadStatusHistorico? LeadStatusHistorico { get; private set; }

        // Construtor para EF Core
        protected AtribuicaoLead() { }

        /// <summary>
        /// Construtor completo para atribuição de lead
        /// </summary>
        public AtribuicaoLead(
            int leadId,
            int membroAtribuidoId,
            int tipoAtribuicaoId,
            string motivoAtribuicao,
            bool atribuicaoAutomatica,
            int? configuracaoDistribuicaoId,
            int? regraDistribuicaoId,
            decimal? scoreVendedor = null,
            int? membroAtribuiuId = null,
            string? parametrosAplicados = null,
            string? vendedoresElegiveis = null,
            string? scoresCalculados = null)
        {
            if (leadId <= 0)
                throw new DomainException("ID do lead deve ser maior que zero", nameof(AtribuicaoLead));

            if (membroAtribuidoId <= 0)
                throw new DomainException("ID do membro atribuído deve ser maior que zero", nameof(AtribuicaoLead));

            if (tipoAtribuicaoId <= 0)
                throw new DomainException("ID do tipo de atribuição deve ser maior que zero", nameof(AtribuicaoLead));

            if (string.IsNullOrEmpty(motivoAtribuicao))
                throw new DomainException("Motivo da atribuição é obrigatório", nameof(AtribuicaoLead));

            // Validações específicas para atribuição manual
            if (!atribuicaoAutomatica && !membroAtribuiuId.HasValue)
                throw new DomainException("ID do membro que atribuiu é obrigatório para atribuição manual", nameof(AtribuicaoLead));

            // Validações específicas para atribuição automática
            if (atribuicaoAutomatica && !configuracaoDistribuicaoId.HasValue)
                throw new DomainException("ID da configuração de distribuição é obrigatório para atribuição automática", nameof(AtribuicaoLead));

            LeadId = leadId;
            MembroAtribuidoId = membroAtribuidoId;
            TipoAtribuicaoId = tipoAtribuicaoId;
            DataAtribuicao = TimeHelper.GetBrasiliaTime();
            MotivoAtribuicao = motivoAtribuicao;
            AtribuicaoAutomatica = atribuicaoAutomatica;
            ConfiguracaoDistribuicaoId = configuracaoDistribuicaoId;
            RegraDistribuicaoId = regraDistribuicaoId;
            ScoreVendedor = scoreVendedor;
            MembroAtribuiuId = membroAtribuiuId;
            ParametrosAplicados = parametrosAplicados;
            VendedoresElegiveis = vendedoresElegiveis;
            ScoresCalculados = scoresCalculados;
        }

        /// <summary>
        /// Vincula esta atribuição a um histórico de status do lead
        /// </summary>
        public void VincularHistoricoStatus(int leadStatusHistoricoId)
        {
            if (leadStatusHistoricoId <= 0)
                throw new DomainException("ID do histórico de status deve ser maior que zero", nameof(AtribuicaoLead));

            LeadStatusHistoricoId = leadStatusHistoricoId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra que o fallback de horário foi aplicado nesta atribuição
        /// </summary>
        /// <param name="detalhes">Detalhes sobre o fallback aplicado</param>
        public void RegistrarFallbackHorario(string detalhes)
        {
            if (string.IsNullOrEmpty(detalhes))
                throw new DomainException("Detalhes do fallback são obrigatórios", nameof(AtribuicaoLead));

            FallbackHorarioAplicado = true;
            DetalhesFallbackHorario = detalhes;
            DataFallbackHorario = TimeHelper.GetBrasiliaTime();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se o fallback de horário foi aplicado
        /// </summary>
        /// <returns>True se o fallback foi aplicado, false caso contrário</returns>
        public bool FoiAplicadoFallbackHorario()
        {
            return FallbackHorarioAplicado;
        }
    }
}