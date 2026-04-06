using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.Chatbot
{
    public class ChatBotClient(ILogger<ChatBotClient> logger, HttpClient httpClient) : IChatBotClient
    {
        private readonly ILogger<ChatBotClient> _logger = logger;
        private readonly HttpClient _httpClient = httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task SendToChatBot(ChatMessageRequestDTO request, string urlChatBot)
        {            
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(urlChatBot, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                _logger.LogError(
                    "❌ Erro ao enviar mensagem para o ChatBot.\n" +
                    "StatusCode: {StatusCode}\n" +
                    "Motivo: {ReasonPhrase}\n" +
                    "URL: {Url}\n" +
                    "CorpoEnviado: {Json}\n" +
                    "RespostaServidor: {ErrorContent}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    urlChatBot,
                    json,
                    errorContent
                );
            }          
        }
    }
}
