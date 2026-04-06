using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para inicializar posição de um vendedor na fila de distribuição
    /// </summary>
    public class InicializarPosicaoFilaRequestDTO
    {
        /// <summary>
        /// ID do vendedor
        /// </summary>
        [Required(ErrorMessage = "ID do vendedor é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID do vendedor deve ser maior que zero")]
        public int VendedorId { get; set; }
        
        /// <summary>
        /// ID da empresa
        /// </summary>
        [Required(ErrorMessage = "ID da empresa é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID da empresa deve ser maior que zero")]
        public int EmpresaId { get; set; }
    }
}
