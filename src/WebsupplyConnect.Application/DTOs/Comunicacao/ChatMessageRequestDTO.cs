namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ChatMessageRequestDTO
    {
        public string Message { get; set; } = string.Empty;
        public int ConversationId { get; set; }
    }
}
