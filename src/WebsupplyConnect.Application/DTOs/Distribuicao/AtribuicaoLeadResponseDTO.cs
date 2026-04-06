using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para resposta de atribuição de lead
    /// </summary>
    public class AtribuicaoLeadResponseDTO
    {
        /// <summary>
        /// ID da atribuição
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do lead atribuído
        /// </summary>
        public int LeadId { get; set; }

        /// <summary>
        /// ID do vendedor que recebeu o lead
        /// </summary>
        public int UsuarioAtribuidoId { get; set; }

        /// <summary>
        /// Nome do vendedor que recebeu o lead
        /// </summary>
        public string NomeVendedor { get; set; } = string.Empty;

        /// <summary>
        /// Email do vendedor que recebeu o lead
        /// </summary>
        public string? EmailVendedor { get; set; }

        /// <summary>
        /// ID do tipo de atribuição
        /// </summary>
        public int TipoAtribuicaoId { get; set; }

        /// <summary>
        /// Nome do tipo de atribuição
        /// </summary>
        public string NomeTipoAtribuicao { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora da atribuição
        /// </summary>
        public DateTime DataAtribuicao { get; set; }

        /// <summary>
        /// Motivo/descrição da atribuição
        /// </summary>
        public string MotivoAtribuicao { get; set; } = string.Empty;

        /// <summary>
        /// Indica se foi uma atribuição automática ou manual
        /// </summary>
        public bool AtribuicaoAutomatica { get; set; }

        /// <summary>
        /// ID da configuração de distribuição utilizada (para atribuição automática)
        /// </summary>
        public int? ConfiguracaoDistribuicaoId { get; set; }

        /// <summary>
        /// Nome da configuração de distribuição utilizada
        /// </summary>
        public string? NomeConfiguracaoDistribuicao { get; set; }

        /// <summary>
        /// ID da regra que foi decisiva na atribuição (para atribuição automática)
        /// </summary>
        public int? RegraDistribuicaoId { get; set; }

        /// <summary>
        /// Nome da regra que foi decisiva na atribuição
        /// </summary>
        public string? NomeRegraDistribuicao { get; set; }

        /// <summary>
        /// Score calculado para o vendedor (para atribuição automática)
        /// </summary>
        public decimal? ScoreVendedor { get; set; }

        /// <summary>
        /// ID do usuário que fez a atribuição manual (para atribuição manual)
        /// </summary>
        public int? UsuarioAtribuiuId { get; set; }

        /// <summary>
        /// Nome do usuário que fez a atribuição manual
        /// </summary>
        public string? NomeUsuarioAtribuiu { get; set; }

        /// <summary>
        /// ID do histórico de status do lead relacionado a esta atribuição
        /// </summary>
        public int? LeadStatusHistoricoId { get; set; }

        /// <summary>
        /// JSON com parâmetros aplicados na atribuição
        /// </summary>
        public string? ParametrosAplicados { get; set; }

        /// <summary>
        /// JSON com lista de vendedores que eram elegíveis para receber o lead
        /// </summary>
        public string? VendedoresElegiveis { get; set; }

        /// <summary>
        /// JSON com scores calculados para os vendedores elegíveis
        /// </summary>
        public string? ScoresCalculados { get; set; }

        /// <summary>
        /// Indica se o fallback de horário foi aplicado
        /// </summary>
        public bool FallbackHorarioAplicado { get; set; }

        /// <summary>
        /// Detalhes sobre o fallback de horário aplicado
        /// </summary>
        public string? DetalhesFallbackHorario { get; set; }

        /// <summary>
        /// Data e hora quando o fallback foi aplicado
        /// </summary>
        public DateTime? DataFallbackHorario { get; set; }

        /// <summary>
        /// Data de criação da atribuição
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data da última modificação da atribuição
        /// </summary>
        public DateTime? DataModificacao { get; set; }
    }
}
