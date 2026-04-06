namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class UsuarioEmpresaDTO
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; }
        public string EmpresaCnpj { get; set; }
        public string Logo { get; set; }
        public bool IsPrincipal { get; set; }
        public DateTime DataAssociacao { get; set; }
        public string GrupoEmpresaNome { get; set; }
        public int CanalPadraoId { get; set; }
        public string CanalPadraoNome { get; set; }
        public string? CodVendedorNBS { get; set; }
        public int? EquipePadraoId { get; set; }
        public string EquipePadraoNome { get; set; }
    }
}
