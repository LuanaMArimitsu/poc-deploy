namespace WebsupplyConnect.Application.DTOs.Empresa
{
    public class EmpresaVinculoDTO
    {
        public int EmpresaId { get; set; }
        public bool? EhPrincipal { get; set; }
        public int CanalPadraoId { get; set; }
        public int EquipePadraoId { get; set; }
        public string? CodVendedorNBS { get; set; }
    }

    public class AtualizarVinculosRequestDTO
    {
        public List<EmpresaVinculoDTO> EmpresasVinculos { get; set; } = new();
    }
}
