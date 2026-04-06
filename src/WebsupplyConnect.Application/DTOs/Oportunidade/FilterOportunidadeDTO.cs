namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class FilterOportunidadeDTO
    {
        public int? LeadId { get; set; }
        public int? ProdutoId { get; set; }
        public int? EtapaId { get; set; }
        public decimal? ValorMinimo { get; set; }
        public decimal? ValorMaximo { get; set; }
        public int? ResponsavelId { get; set; }
        public int? OrigemId { get; set; }
        public int? EmpresaId { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }
        public int Pagina { get; set; } 
        public int TamanhoPagina { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
