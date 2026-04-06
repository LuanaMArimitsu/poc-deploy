namespace WebsupplyConnect.Application.DTOs.Lead.Historico
{
    public class LeadEventoDTO
    {
        public int LeadId { get; set; }
        public int OrigemId { get; set; }
        public int? CanalId { get; set; }
        public int? CampanhaId { get; set; }
        public string? Observacao { get; set; }
    }
}
