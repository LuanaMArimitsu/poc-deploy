namespace WebsupplyConnect.Application.DTOs.Produto
{
   public class AdicionarProdutoRequestDTO
   {
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }
        public decimal? ValorReferencia { get; set; }
        public string? Url { get; set; }
        public int EmpresaId { get; set; }
   }
}
