namespace WebsupplyConnect.Application.DTOs.Produto
{
    public class ProdutoFiltroRequestDTO
    {
        public string? Busca { get; set; }
        public bool? Ativo { get; set; }
        public int EmpresaId { get; set; }
        public int Pagina { get; set; } 
        public int TamanhoPagina { get; set; }

        public string OrdenarPor { get; set; } = "Nome";
        public string DirecaoOrdenacao { get; set; } = "ASC";
    }
}
