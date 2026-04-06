namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class UsuarioListagemDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Cargo { get; set; }
        public string? Departamento { get; set; }
        public bool Ativo { get; set; }
        public int EmpresaPrincipalId { get; set; }
        public string EmpresaPrincipal { get; set; }
        public string InicialAvatar { get; set; }
        public string CorAvatar { get; set; }
    }
}
