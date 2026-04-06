namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class UpdateOportunidadeDTO 
    { 
        public int Id { get; set; }
        public int? ProdutoId { get; set; }
        public int? TipoInteresseId { get; set; }
        public decimal? Valor { get; set; }
        public int? Probabilidade { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }
        public string? Observacao { get; set; } = string.Empty;
        public DateTime? DataFechamento { get; set; }
        public decimal? ValorFinal { get; set; }
        public DateTime? DataUltimaInteracao { get; set; }

        public int ResponsavelId { get; set; }
        public int EmpresaId { get; set; }
        public int? LeadEventoId { get; set; }
    }
}
