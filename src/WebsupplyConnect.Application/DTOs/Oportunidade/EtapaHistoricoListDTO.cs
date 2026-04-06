namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class EtapaHistoricoListDTO
    {
        public required string NomeEtapa { get; set; }
        public DateTime DataMudanca { get; set; }
        public string? Observacao { get; set; }
        public string? Cor { get; set; }
    }
}
