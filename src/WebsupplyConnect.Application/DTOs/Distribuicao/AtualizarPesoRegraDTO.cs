using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para atualização de peso de uma regra de distribuição
    /// </summary>
    public class AtualizarPesoRegraDTO
    {
        /// <summary>
        /// Novo peso da regra (0-100)
        /// </summary>
        [Required(ErrorMessage = "O peso é obrigatório")]
        [Range(0, 100, ErrorMessage = "O peso deve estar entre 0 e 100")]
        public int NovoPeso { get; set; }
        
        /// <summary>
        /// Motivo da alteração do peso
        /// </summary>
        [StringLength(500, ErrorMessage = "O motivo deve ter no máximo 500 caracteres")]
        public string? Motivo { get; set; }
        
        /// <summary>
        /// Indica se deve recalcular scores automaticamente após a alteração
        /// </summary>
        public bool RecalcularScores { get; set; } = true;
    }
    
    /// <summary>
    /// DTO para resposta de atualização de peso
    /// </summary>
    public class AtualizarPesoRegraResponseDTO
    {
        /// <summary>
        /// ID da regra atualizada
        /// </summary>
        public int RegraId { get; set; }
        
        /// <summary>
        /// Nome da regra
        /// </summary>
        public string NomeRegra { get; set; } = string.Empty;
        
        /// <summary>
        /// Peso anterior
        /// </summary>
        public int PesoAnterior { get; set; }
        
        /// <summary>
        /// Novo peso
        /// </summary>
        public int NovoPeso { get; set; }
        
        /// <summary>
        /// Diferença no peso
        /// </summary>
        public int DiferencaPeso { get; set; }
        
        /// <summary>
        /// Data da atualização
        /// </summary>
        public DateTime DataAtualizacao { get; set; }
        
        /// <summary>
        /// Indica se os scores foram recalculados
        /// </summary>
        public bool ScoresRecalculados { get; set; }
        
        /// <summary>
        /// Mensagem de confirmação
        /// </summary>
        public string Mensagem { get; set; } = string.Empty;
    }
}
