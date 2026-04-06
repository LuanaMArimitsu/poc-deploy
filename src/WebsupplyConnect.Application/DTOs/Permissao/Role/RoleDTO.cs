using WebsupplyConnect.Application.DTOs.Permissao.Permissao;

namespace WebsupplyConnect.Application.DTOs.Permissao.Role
{
    public class RoleDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int Nivel { get; set; }
        public int? EmpresaId { get; set; }
        public string? Empresa { get; set; }
        public string Contexto { get; set; }
        public bool IsSistema { get; set; }
        public bool Ativa { get; set; }
        public int QntdUsuarios { get; set; } = 0;
        public List<PermissaoDTO> Permissoes { get; set; }
    }
}
