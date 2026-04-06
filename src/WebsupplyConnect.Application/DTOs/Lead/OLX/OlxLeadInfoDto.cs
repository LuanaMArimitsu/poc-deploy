using System.Text.Json.Serialization;

namespace WebsupplyConnect.Application.DTOs.Lead.OLX
{
    public class OlxLeadInfoDto
    {
        public string? Subject { get; set; }

        [JsonPropertyName("vehicle_tag")]
        public string? VehicleTag { get; set; }
    }
}
