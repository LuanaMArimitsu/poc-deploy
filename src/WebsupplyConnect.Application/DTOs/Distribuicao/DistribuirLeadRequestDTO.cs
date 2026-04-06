using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para distribuição de leads
    /// </summary>
    public class DistribuirLeadRequestDTO
    {
        /// <summary>
        /// ID do lead a ser distribuído
        /// </summary>
        [Required(ErrorMessage = "ID do lead é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID do lead deve ser maior que zero")]
        public int LeadId { get; set; }
        
        /// <summary>
        /// ID da empresa
        /// </summary>
        [Required(ErrorMessage = "ID da empresa é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID da empresa deve ser maior que zero")]
        public int EmpresaId { get; set; }
        
        /// <summary>
        /// ID da configuração de distribuição (opcional)
        /// </summary>
        public int? ConfiguracaoId { get; set; }
    }
}
