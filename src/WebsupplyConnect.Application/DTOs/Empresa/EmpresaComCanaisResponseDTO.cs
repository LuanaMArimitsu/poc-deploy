namespace WebsupplyConnect.Application.DTOs.Empresa
{
    public class EmpresaComCanaisResponseDTO
    {
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;
        public string EmpresaCnpj { get; set; } = string.Empty;
        public string GrupoEmpresaNome { get; set; } = string.Empty;
        public bool PossuiIntegracaoNBS { get; set; } = false;
        public List<CanalItemDTO> Canais { get; set; } = new();
    }

    public class CanalItemDTO
    {
        public int CanalId { get; set; }
        public string CanalNome { get; set; } = string.Empty;
    }
}
