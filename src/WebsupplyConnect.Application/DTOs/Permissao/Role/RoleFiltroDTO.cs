namespace WebsupplyConnect.Application.DTOs.Permissao.Role
{
    public class RoleFiltroDTO
    {
        public string? Nome { get; set; }
        public int EmpresaId { get; set; }
        public string? Contexto { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 5;
    }
}
