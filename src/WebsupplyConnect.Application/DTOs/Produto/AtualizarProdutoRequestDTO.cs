namespace WebsupplyConnect.Application.DTOs.Produto
{
    public class AtualizarProdutoRequestDTO
    {
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Url { get; set; }
        public decimal? ValorReferencia { get; set; }
    }
}
