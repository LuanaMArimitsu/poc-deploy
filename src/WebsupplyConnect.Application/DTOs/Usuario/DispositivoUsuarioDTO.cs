namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class DispositivoUsuarioDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Cargo { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public string EmpresaPrincipal { get; set; } = null!;
    }
}
