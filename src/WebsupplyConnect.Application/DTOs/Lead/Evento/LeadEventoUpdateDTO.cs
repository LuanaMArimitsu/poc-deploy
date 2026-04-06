namespace WebsupplyConnect.Application.DTOs.Lead.Evento
{
    public class LeadEventoUpdateDTO
    {
        public int? OrigemId { get; set; }
        public int? CanalId { get; set; }
        public int? CampanhaId { get; set; }
        public string? Observacao { get; set; }
    }
}