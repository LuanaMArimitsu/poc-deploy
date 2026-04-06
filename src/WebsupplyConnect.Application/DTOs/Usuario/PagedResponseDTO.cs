namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class PagedResponseDTO<T>
    {
        public List<T> Itens { get; set; } = new();
        public int? PaginaAtual { get; set; }
        public int? TamanhoPagina { get; set; }
        public int? TotalItens { get; set; }
        public int? TotalPaginas { get; set; }
    }
}
