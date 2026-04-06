namespace WebsupplyConnect.Application.DTOs.Permissao.Permissao
{
    public class PermissaoFiltroDTO
    {
        public string? Nome { get; set; }
        public string? Modulo { get; set; }
        public bool Criticas { get; set; } = false;
        public string? Categoria { get; set; } 
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 5;
    }
}
