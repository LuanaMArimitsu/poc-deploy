using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class WhatsAppMediaClient(HttpClient httpClient, IOptions<WebhookMetaConfig> config, ILogger<WhatsAppMediaClient> logger) : IWhatsAppMediaClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<WhatsAppMediaClient> _logger = logger;
        private readonly string _urlMeta = config?.Value?.MetaUrl ?? throw new ArgumentNullException(nameof(config));
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<MidiaMetaDTO> GetMediaInfoAsync(string midiaId, string acessTokenMeta)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_urlMeta}{midiaId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", acessTokenMeta);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MidiaMetaDTO>(json, _jsonOptions);

                return data ?? throw new AppException("Não foi possível desseralizar as informações da midia vinda da meta.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações da mídia ID: {MediaId}", midiaId);
                throw new AppException($"Erro ao obter informações da mídia ID: {midiaId}", ex);
            }
        }

        public async Task<Stream> DownloadMediaAsync(MidiaMetaDTO midiaInfo, string whatsAppToken)
        {
            try
            {
                var tokenMeta = whatsAppToken;
                var mediaId = midiaInfo.Id;

                var request = new HttpRequestMessage(HttpMethod.Get, midiaInfo.Url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenMeta);

                var response = await _httpClient.SendAsync(request);
                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar mídia ID: {MediaId}", midiaInfo.Id);
                throw new AppException($"Erro ao baixar mídia ID: {midiaInfo.Id}", ex);
            }
        }

        public async Task<string?> EnviarMidiaParaMetaAsync(
            byte[] fileBytes,
            string mimeType,
            string fileName,
            string telefoneId,
            string token,
            string tipoMensagem)
        {
            try
            {

                using var client = new HttpClient();
                using var content = new MultipartFormDataContent();

                var byteArrayContent = new ByteArrayContent(fileBytes);
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                content.Add(byteArrayContent, "file", fileName);
                content.Add(new StringContent(tipoMensagem.ToLowerInvariant()), "type");
                content.Add(new StringContent("whatsapp"), "messaging_product");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_urlMeta}{telefoneId}/media")
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                    Content = content
                };

                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InfraException($"Erro ao enviar mídia e recuperar Midia Meta ID: {responseContent}");
                }

                using var json = JsonDocument.Parse(responseContent);
                if (json.RootElement.TryGetProperty("id", out var idProperty))
                {
                    return idProperty.GetString();
                }

                return null; 
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao enviar mídia para Meta: {ex}", ex);
                throw new InfraException($"Erro ao enviar mídia para Meta: {ex}");
               
            }
        }
    }
}
