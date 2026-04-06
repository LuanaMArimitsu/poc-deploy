namespace WebsupplyConnect.Application.DTOs.Permissao.Role
{
    public class RolePaginadoDTO
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public IEnumerable<RoleDTO> Itens { get; set; }
    }
}
