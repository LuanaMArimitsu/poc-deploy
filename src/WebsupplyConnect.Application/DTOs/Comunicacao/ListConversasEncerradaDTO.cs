namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ListConversasEncerradaDTO
    {
        public int ConversaId { get; set; }
        public int LeadId { get; set; }
        public string LeadNome { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}
