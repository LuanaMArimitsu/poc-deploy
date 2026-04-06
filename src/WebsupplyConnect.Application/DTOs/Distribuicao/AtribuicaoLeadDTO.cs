using System.Text.Json;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para representar uma atribuição de lead
    /// </summary>
    public class AtribuicaoLeadDTO
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
        public string? NomeVendedor { get; set; }

        /// <summary>
        /// ID do tipo de atribuição
        /// </summary>
        public int TipoAtribuicaoId { get; set; }

        /// <summary>
        /// Nome do tipo de atribuição
        /// </summary>
        public string? NomeTipoAtribuicao { get; set; }

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
        /// ID da configuração de distribuição utilizada
        /// </summary>
        public int? ConfiguracaoDistribuicaoId { get; set; }

        /// <summary>
        /// Nome da configuração de distribuição
        /// </summary>
        public string? NomeConfiguracaoDistribuicao { get; set; }

        /// <summary>
        /// ID da regra que foi decisiva na atribuição
        /// </summary>
        public int? RegraDistribuicaoId { get; set; }

        /// <summary>
        /// Nome da regra que foi decisiva
        /// </summary>
        public string? NomeRegraDistribuicao { get; set; }

        /// <summary>
        /// Score calculado para o vendedor
        /// </summary>
        public decimal? ScoreVendedor { get; set; }

        /// <summary>
        /// ID do usuário que fez a atribuição manual
        /// </summary>
        public int? UsuarioAtribuiuId { get; set; }

        /// <summary>
        /// Nome do usuário que fez a atribuição manual
        /// </summary>
        public string? NomeUsuarioAtribuiu { get; set; }

        /// <summary>
        /// Parâmetros aplicados na atribuição
        /// </summary>
        public Dictionary<string, object>? ParametrosAplicados { get; set; }

        /// <summary>
        /// Lista de vendedores que eram elegíveis
        /// </summary>
        public List<object>? VendedoresElegiveis { get; set; }

        /// <summary>
        /// Scores calculados para os vendedores elegíveis
        /// </summary>
        public Dictionary<string, object>? ScoresCalculados { get; set; }

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
        /// Converte a string JSON de parâmetros para Dictionary
        /// </summary>
        public void ConverterParametrosJson(string? jsonParametros)
        {
            if (!string.IsNullOrEmpty(jsonParametros))
            {
                try
                {
                    ParametrosAplicados = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonParametros);
                }
                catch
                {
                    ParametrosAplicados = new Dictionary<string, object> { { "erro", "Falha ao deserializar parâmetros" } };
                }
            }
        }

        /// <summary>
        /// Converte a string JSON de vendedores elegíveis para List
        /// </summary>
        public void ConverterVendedoresElegiveisJson(string? jsonVendedores)
        {
            if (!string.IsNullOrEmpty(jsonVendedores))
            {
                try
                {
                    VendedoresElegiveis = JsonSerializer.Deserialize<List<object>>(jsonVendedores);
                }
                catch
                {
                    VendedoresElegiveis = new List<object> { new { erro = "Falha ao deserializar vendedores elegíveis" } };
                }
            }
        }

        /// <summary>
        /// Converte a string JSON de scores para Dictionary
        /// </summary>
        public void ConverterScoresJson(string? jsonScores)
        {
            if (!string.IsNullOrEmpty(jsonScores))
            {
                try
                {
                    ScoresCalculados = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonScores);
                }
                catch
                {
                    ScoresCalculados = new Dictionary<string, object> { { "erro", "Falha ao deserializar scores" } };
                }
            }
        }
    }
}
