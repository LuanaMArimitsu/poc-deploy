namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class GetOportunidadeDTO
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
        public required string NomeLead { get; set; }
        public string? NivelInteresse { get; set; }
        public int ProdutoId { get; set; }
        public required string NomeProduto { get; set; }
        public int EtapaId { get; set; }
        public required string NomeEtapa { get; set; }
        public decimal? Valor { get; set; }
        public int ResponsavelId { get; set; }
        public required string NomeResponsavel { get; set; }
        public int OrigemId { get; set; }
        public required string NomeOrigem { get; set; }
        public int EmpresaId { get; set; }
        public required string NomeEmpresa { get; set; }
        public int? TipoInteresseId { get; set; }
        public string? NomeInteresse { get; set; }
        public string? CodEventoNBS { get; set; }
        public bool ConvertidaNBS { get; set; }
        public int? Probabilidade { get; set; }
        public string? Observacoes { get; set; }
        public decimal? ValorFinal { get; set; }    
        public DateTime? DataPrevisaoFechamento { get; set; }
        public DateTime? DataFechamento { get; set; }
        public DateTime? DataUltimaInteracao { get; set; }
        public DateTime? DataCriacao { get; set; }

        //Evento
        public bool TemEvento { get; set; }
        public int? IdEvento { get; set; }
        public string? CampanhaDoEventoNome { get; set; }
        public string? CanalDoEvento { get; set; }
        public string? ObservacaoDoEvento { get; set; }
    }
}
