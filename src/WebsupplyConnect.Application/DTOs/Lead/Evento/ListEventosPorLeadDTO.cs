namespace WebsupplyConnect.Application.DTOs.Lead.Evento
{
    public class ListEventosPorLeadDTO
    {
        public int LeadId { get; set; }
        public string Lead { get; set; } = string.Empty;

        public List<ListEventoCampanhaSimplesDTO> Eventos { get; set; } = new();
    }
}
