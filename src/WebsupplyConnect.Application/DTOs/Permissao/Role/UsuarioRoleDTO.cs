namespace WebsupplyConnect.Application.DTOs.Permissao.Role
{
    public class UsuarioRoleDTO
    {
        public int UsuarioId { get; set; }

        public string Nome { get; set; }

        public int RoleId { get; set; }

        public DateTime DataAtribuicao { get; set; }

        public DateTime? DataExpiracao { get; set; }

        public bool Ativo { get; set; }

        public int AtribuidorId { get; set; }
        public string AtribuidorNome { get; set; }

        public string? Justificativa { get; set; }
    }
}
