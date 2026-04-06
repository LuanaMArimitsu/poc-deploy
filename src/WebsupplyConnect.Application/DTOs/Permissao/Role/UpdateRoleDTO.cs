namespace WebsupplyConnect.Application.DTOs.Permissao.Role
{
    public class UpdateRoleDTO
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Ativa { get; set; }
        public int[] Permissoes { get; set; }

        public int EmpresaId { get; set; }
    }
}
