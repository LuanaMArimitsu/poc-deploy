namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ConversasEncerradasResultDTO
    {
        public int TotalEncerradas { get; set; }
        public List<ListConversasEncerradaDTO> Conversas { get; set; }
    }
}
