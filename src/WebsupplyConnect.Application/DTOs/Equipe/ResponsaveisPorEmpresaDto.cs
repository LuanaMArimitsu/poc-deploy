namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class ResponsaveisPorEmpresaDto
    {
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;
        public List<ResponsavelEquipeDto> Responsaveis { get; set; } = new();
    }
}
