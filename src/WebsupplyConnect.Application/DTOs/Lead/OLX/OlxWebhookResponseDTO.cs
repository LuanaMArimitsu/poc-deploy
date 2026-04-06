namespace WebsupplyConnect.Application.DTOs.Lead.OLX
{
    public class OlxWebhookResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? ResponseId { get; set; }
    }
}
