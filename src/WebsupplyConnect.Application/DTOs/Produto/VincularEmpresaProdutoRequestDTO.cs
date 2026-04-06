namespace WebsupplyConnect.Application.DTOs.Produto
{
    public class VincularEmpresaProdutoRequestDTO
    {
        public int ProdutoId { get; set; }
        public int EmpresaId { get; set; }
        public decimal? ValorPersonalizado { get; set; }
    }
}
