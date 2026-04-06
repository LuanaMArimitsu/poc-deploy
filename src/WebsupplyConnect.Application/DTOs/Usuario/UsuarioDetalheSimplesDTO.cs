namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class UsuarioDetalheSimplesDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Departamento { get; set; }
        public bool Ativo { get; set; }
        public bool Cadastrado { get; set; }
        public DateTime? UltimoAcesso { get; set; }

        // Avatar
        public string InicialAvatar { get; set; }
        public string CorAvatar { get; set; }

        // Hierarquia
        public int? UsuarioSuperiorId { get; set; }
        public string UsuarioSuperiorNome { get; set; }
    }
}
