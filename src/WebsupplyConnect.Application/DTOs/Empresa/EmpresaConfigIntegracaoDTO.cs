using System.Text.Json.Serialization;
using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.DTOs.Empresa
{
    public class EmpresaConfigIntegracaoDTO
    {
        public Openai? OpenAI { get; set; }
        public Localizacao? Localizacao { get; set; }
    }

    public class Openai
    {
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
        public required string Model { get; set; }
        public required string OrganizationId { get; set; }
        public required string ProjectId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int QuantidadeMensagens { get; set; }
    }
}
