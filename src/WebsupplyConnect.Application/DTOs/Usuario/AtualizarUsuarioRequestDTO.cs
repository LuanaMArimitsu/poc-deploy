namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class AtualizarUsuarioRequestDTO
    {
        public string? Cargo { get; set; }
        public string? Departamento { get; set; }
        public bool Ativo { get; set; }
        public int? UsuarioSuperiorId { get; set; }
    }
}
