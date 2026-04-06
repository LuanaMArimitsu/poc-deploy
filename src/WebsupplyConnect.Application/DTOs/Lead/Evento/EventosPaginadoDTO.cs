namespace WebsupplyConnect.Application.DTOs.Lead.Evento
{
    public class EventosPaginadoDto
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public List<ListEventosPorLeadDTO> Itens { get; set; } = new();
    }
}
