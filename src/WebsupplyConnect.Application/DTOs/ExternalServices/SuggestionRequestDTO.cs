namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class SuggestionRequestDTO
    {
        public int ConversaId { get; set; }
        public int EmpresaId { get; set; }
        public string? Rascunho { get; set; } = null;
    }
}
