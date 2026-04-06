namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class CreateOportunidadeDTO 
    {
        public int LeadId { get; set; }

        public int ProdutoId { get; set; }

        public int EtapaId { get; set; }

        public decimal? Valor { get; set; }

        public int OrigemId { get; set; }

        public int EmpresaId { get; set; }

        public int? TipoInteresseId { get; set; }

        public string? Observacao { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }

        public int? LeadEventoId { get; set; }

    }
}
