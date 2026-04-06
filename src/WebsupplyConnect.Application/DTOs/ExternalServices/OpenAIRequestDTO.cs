using Newtonsoft.Json;

namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class OpenAIRequestDTO
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("messages")]
        public List<OpenAIMessageDTO> Messages { get; set; } = new();

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("n")]
        public int N { get; set; } = 1;
    }

    public class OpenAIMessageDTO
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }
}
