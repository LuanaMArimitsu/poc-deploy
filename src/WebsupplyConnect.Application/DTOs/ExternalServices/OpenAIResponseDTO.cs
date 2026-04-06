using Newtonsoft.Json;

namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class OpenAIResponseDTO
    {
        [JsonProperty("choices")]
        public List<OpenAIChoiceDTO> Choices { get; set; } = new();

        [JsonProperty("usage")]
        public OpenAIUsageDTO? Usage { get; set; }
    }

    public class OpenAIChoiceDTO
    {
        [JsonProperty("message")]
        public OpenAIMessageDTO Message { get; set; } = new();

        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class OpenAIUsageDTO
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
