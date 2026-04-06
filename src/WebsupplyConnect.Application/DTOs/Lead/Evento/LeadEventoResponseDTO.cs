namespace WebsupplyConnect.Application.DTOs.Lead.Historico
{
    public class LeadEventoResponseDTO
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
        public string? LeadNome { get; set; }
        public int OrigemId { get; set; }
        public string? OrigemNome { get; set; }
        public int? CanalId { get; set; }
        public string? CanalNome { get; set; }
        public int? CampanhaId { get; set; }
        public string? CampanhaNome { get; set; }
        public DateTime DataEvento { get; set; }
        public string? Observacao { get; set; }
        public string[]? OportunidadesVinculadas { get; set; }
    }
}
