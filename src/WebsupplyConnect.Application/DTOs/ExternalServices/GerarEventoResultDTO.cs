using System.Text.Json.Serialization;

namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class GerarEventoResultDTO
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; set; }

        [JsonPropertyName("codEvento")]
        public string? CodEvento { get; set; }

        [JsonPropertyName("mensagem")]
        public string Mensagem { get; set; }
    }
}
