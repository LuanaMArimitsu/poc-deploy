namespace WebsupplyConnect.Application.DTOs.Permissao.Role
{
    public class CreateRoleDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int? EmpresaId { get; set; }
        public int Nivel { get; set; }
        public string Contexto { get; set; } = string.Empty;
    }
}
