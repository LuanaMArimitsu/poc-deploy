namespace WebsupplyConnect.Application.DTOs.Produto
{
    public class ProdutoListagemDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal? ValorReferencia { get; set; }
        public bool Ativo { get; set; }
    }
}
