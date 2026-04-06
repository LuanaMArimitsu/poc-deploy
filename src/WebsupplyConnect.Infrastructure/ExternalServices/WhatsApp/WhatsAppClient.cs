using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class WhatsAppClient(HttpClient httpClient, IOptions<WebhookMetaConfig> config, ILogger<WhatsAppClient> logger) : IWhatsAppClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<WhatsAppClient> _logger = logger;
        private readonly string _metaUrl = config?.Value?.MetaUrl ?? throw new ArgumentNullException(nameof(config));
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };


        public async Task<HttpResponseMessage> EnviarMensagemTextoAsync(string telefoneDestino, string mensagem, string token, string telefoneId)
        {
            try
            {
                var body = new
                {
                    messaging_product = "whatsapp",
                    to = telefoneDestino,
                    type = "text",
                    text = new { body = mensagem }
                };

                var json = JsonSerializer.Serialize(body, _jsonOptions);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_metaUrl}/{telefoneId}/messages");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem de texto para {Telefone}", telefoneDestino);
                throw new AppException("Erro ao enviar mensagem de texto.", ex);
            }
        }

        public async Task<HttpResponseMessage> EnviarTemplateMontadoAsync(object corpoTemplate, string token, string telefoneId)
        {
            try
            {
                var json = JsonSerializer.Serialize(corpoTemplate, _jsonOptions);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_metaUrl}/{telefoneId}/messages");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar template montado para o WhatsApp.");
                throw new AppException("Erro ao enviar mensagem template montada.", ex);
            }
        }

        public async Task<HttpResponseMessage> EnviarMidiaPorIdAsync(string telefoneDestino, string tipoMidia, string mediaMetaId, string token, string telefoneId, string filename , string? caption = null)
        {
            try
            {
                object midiaBody = tipoMidia.ToLower() switch
                {
                    "image" => new
                    {
                        messaging_product = "whatsapp",
                        to = telefoneDestino,
                        type = "image",
                        image = new
                        {
                            id = mediaMetaId,
                            caption = caption
                        }
                    },
                    "document" => new
                    {
                        messaging_product = "whatsapp",
                        to = telefoneDestino,
                        type = "document",
                        document = new
                        {
                            id = mediaMetaId,
                            caption,
                            filename
                        }
                    },
                    "audio" => new
                    {
                        messaging_product = "whatsapp",
                        to = telefoneDestino,
                        type = "audio",
                        audio = new
                        {
                            id = mediaMetaId,
                            voice = true
                        }
                    },
                    "video" => new
                    {
                        messaging_product = "whatsapp",
                        to = telefoneDestino,
                        type = "video",
                        video = new
                        {
                            id = mediaMetaId,
                            caption = caption
                        }
                    },

                    _ => throw new AppException($"Tipo de mídia não suportado: {tipoMidia}")
                };

                var json = JsonSerializer.Serialize(midiaBody, _jsonOptions);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_metaUrl}/{telefoneId}/messages");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mídia (via ID) do tipo {TipoMidia} para {Telefone}", tipoMidia, telefoneDestino);
                throw new AppException("Erro ao enviar mídia por ID.", ex);
            }
        }

        public async Task<HttpResponseMessage> MarcarMensagemComoLidaAsync(string mensagemMetaId, string token, string telefoneId)
        {
            object bodyRequest = new
            {
                messaging_product = "whatsapp",
                status = "read",
                message_id = mensagemMetaId
            };
            var json = JsonSerializer.Serialize(bodyRequest, _jsonOptions);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_metaUrl}/{telefoneId}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}
