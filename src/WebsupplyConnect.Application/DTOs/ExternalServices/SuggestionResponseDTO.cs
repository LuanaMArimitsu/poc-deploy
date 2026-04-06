namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class SuggestionResponseDTO
    {
        public int ConversaId { get; set; }
        public List<SuggestionItemDTO> Suggestions { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string? Error { get; set; }
        public int TotalMessages { get; set; }
    }

    public class SuggestionItemDTO
    {
        public int Order { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double? Confidence { get; set; }
    }
}
