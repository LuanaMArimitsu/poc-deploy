namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ConversasEncerradasLeadResultDTO
    {
        public int TotalEncerradas { get; set; }
        public List<ListConversasEncerradasLeadDTO> Conversas { get; set; }
    }
}
