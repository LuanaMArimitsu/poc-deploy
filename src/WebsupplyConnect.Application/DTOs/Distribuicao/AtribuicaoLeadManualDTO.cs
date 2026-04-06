using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para solicitação de atribuição manual de lead
    /// </summary>
    public class AtribuicaoLeadManualDTO
    {
        /// <summary>
        /// ID do lead a ser atribuído
        /// </summary>
        [Required(ErrorMessage = "O ID do lead é obrigatório")]
        public int LeadId { get; set; }
        
        /// <summary>
        /// ID do vendedor que receberá o lead
        /// </summary>
        [Required(ErrorMessage = "O ID do vendedor é obrigatório")]
        public int VendedorId { get; set; }
        
        /// <summary>
        /// Motivo da atribuição manual
        /// </summary>
        [Required(ErrorMessage = "O motivo da atribuição é obrigatório")]
        [StringLength(500, ErrorMessage = "O motivo deve ter no máximo {1} caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO para retorno de informações de atribuição de lead
    /// </summary>
    public class AtribuicaoLeadInfoDTO
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
        public int VendedorId { get; set; }
        
                /// <summary>
        /// Nome do vendedor que recebeu o lead
        /// </summary>
        public string NomeVendedor { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de atribuição (Automática, Manual, etc)
        /// </summary>
        public string TipoAtribuicao { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora da atribuição
        /// </summary>
        public DateTime DataAtribuicao { get; set; }
        
        /// <summary>
        /// Motivo da atribuição
        /// </summary>
        public string MotivoAtribuicao { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se foi uma atribuição automática
        /// </summary>
        public bool AtribuicaoAutomatica { get; set; }
        
        /// <summary>
        /// ID da configuração de distribuição utilizada
        /// </summary>
        public int? ConfiguracaoDistribuicaoId { get; set; }
        
        /// <summary>
        /// ID da regra de distribuição utilizada
        /// </summary>
        public int? RegraDistribuicaoId { get; set; }
        
        /// <summary>
        /// Nome da regra de distribuição utilizada
        /// </summary>
        public string NomeRegraDistribuicao { get; set; } = string.Empty;
        
        /// <summary>
        /// Score calculado para o vendedor nesta atribuição
        /// </summary>
        public decimal? ScoreVendedor { get; set; }
        
        /// <summary>
        /// ID do usuário que realizou a atribuição manual
        /// </summary>
        public int? UsuarioAtribuiuId { get; set; }
        
        /// <summary>
        /// Nome do usuário que realizou a atribuição manual
        /// </summary>
        public string NomeUsuarioAtribuiu { get; set; } = string.Empty;
    }
}