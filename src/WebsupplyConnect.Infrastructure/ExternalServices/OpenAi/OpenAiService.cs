using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.ExternalServices.OpenAi
{
    public class OpenAiService(ILogger<OpenAiService> logger, HttpClient httpClient) : IOpenAiService, IDisposable
    {
        private readonly ILogger<OpenAiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public void GetConfig(Openai config)
        {

            if (config is null)
            {
                throw new InfraException("Configuração de integração não pode ser nula.");
            }
            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                throw new InfraException("OPENAI_API_KEY não configurada. Defina a variável de ambiente 'OPENAI_API_KEY' ou preencha 'OpenAI:ApiKey' em appsettings.json (evite commitar).");
            }

            var sanitizedKey = config.ApiKey.Trim();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sanitizedKey);


            var headers = _httpClient.DefaultRequestHeaders;
            headers.Accept.Clear();
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Evita headers duplicados
            const string OrgHeader = "OpenAI-Organization";
            const string ProjHeader = "OpenAI-Project";
            if (headers.Contains(OrgHeader)) headers.Remove(OrgHeader);
            if (headers.Contains(ProjHeader)) headers.Remove(ProjHeader);

            // Se a chave NÃO for project-scoped, envia Organization/Project quando houver
            var isProjectScopedKey = sanitizedKey.StartsWith("sk-proj-", StringComparison.OrdinalIgnoreCase);
            if (!isProjectScopedKey)
            {
                if (!string.IsNullOrWhiteSpace(config.OrganizationId))
                    headers.Add(OrgHeader, config.OrganizationId.Trim());

                if (!string.IsNullOrWhiteSpace(config.ProjectId))
                    headers.Add(ProjHeader, config.ProjectId.Trim());
            }

            _logger.LogDebug("Headers da OpenAI aplicados (project-scoped: {IsProj})", isProjectScopedKey);

        }

        public async Task<List<string>> GenerateSuggestionsAsync(List<MensagemDTO> mensagens, Openai config, string prompt, string promptSistema, string? rascunho = null)
        {
            try
            {
                var promptMontado = BuildPromptSuggestions(mensagens, prompt, rascunho);
                var systemContent = promptSistema;

                var request = new OpenAIRequestDTO
                {
                    Model = config.Model,
                    Messages =
                    [
                        new() { Role = "system", Content = systemContent },
                        new() { Role = "user", Content = promptMontado }
                    ],
                    MaxTokens = 400,
                    Temperature = 0.7
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando requisição para OpenAI: {Model}", config.Model);

                var response = await _httpClient.PostAsync($"{config.BaseUrl}/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponseDTO>(responseJson);

                    if (openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content != null)
                    {
                        return ParseSuggestions(openAIResponse.Choices.First().Message.Content);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API OpenAI: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Erro ao chamar OpenAI API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar sugestões");
                throw;
            }
        }

        public async Task<string> GenerateResumoAsync(List<MensagemDTO> mensagens, Openai config, string prompt, string promptSistema)
        {
            try
            {
                var contextoMensagens = BuildConversationContext(mensagens);
                var systemContent = promptSistema;

                var promptMontado = $"""
                    {prompt}

                    Contexto:
                    {contextoMensagens} 
                 """;


                var request = new OpenAIRequestDTO
                {
                    Model = config.Model,
                    Messages =
                    [
                        new() { Role = "system", Content = systemContent },
                        new() { Role = "user", Content = promptMontado }
                    ],
                    MaxTokens = 400,
                    Temperature = 0.7
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando requisição para OpenAI: {Model}", config.Model);

                var response = await _httpClient.PostAsync($"{config.BaseUrl}/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponseDTO>(responseJson);

                    if (openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content != null)
                    {
                        return openAIResponse.Choices.First().Message.Content;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API OpenAI: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Erro ao chamar OpenAI API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar resumo");
                throw;
            }
        }

        public async Task<string> GenerateClassificacaoAsync(Openai config, string promptSistema, string payloadJson)
        {
            try
            {
                var request = new OpenAIRequestDTO
                {
                    Model = config.Model,
                    Messages =
                    [
                        new() { Role = "system", Content = promptSistema },
                        new() { Role = "user", Content = payloadJson }
                    ],
                    MaxTokens = 1500,
                    Temperature = 0.3
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando requisição de classificação para OpenAI: {Model}", config.Model);

                var response = await _httpClient.PostAsync($"{config.BaseUrl}/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponseDTO>(responseJson);

                    if (openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content != null)
                    {
                        return openAIResponse.Choices.First().Message.Content;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API OpenAI classificação: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Erro ao chamar OpenAI API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar classificação");
                throw;
            }
        }

        public async Task<string> GenerateTranscricaoAsync(Byte[] audioBytes, string tipoAudio, Openai config)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
               
                var audioContent = new ByteArrayContent(audioBytes);
                audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue((tipoAudio));

                var extensao = tipoAudio switch
                {
                    "audio/mpeg" => "mp3",
                    "audio/ogg" => "ogg",
                    "audio/aac" => "aac",
                    "audio/mp4" => "mp4",
                    _ => "mp3"
                };

                formData.Add(audioContent, "file", $"audio.{extensao}");
                formData.Add(new StringContent("gpt-4o-transcribe"), "model");
                formData.Add(new StringContent("json"), "response_format");
                formData.Add(new StringContent("pt"), "language");

                var response = await _httpClient.PostAsync(
                    $"{config.BaseUrl}/audio/transcriptions",
                    formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponseTranscricaoDTO.Rootobject>(responseJson);

                    if (!string.IsNullOrEmpty(openAIResponse?.text))
                    {
                        return openAIResponse.text;
                    }

                    _logger.LogError("Resposta da API não contém texto de transcrição: {StatusCode} - {Error}", response.StatusCode, responseJson);
                    throw new Exception($"Erro ao chamar OpenAI transcrição API: {response.StatusCode}");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API OpenAI transcrição: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Erro ao chamar OpenAI transcrição API: {response.StatusCode}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar sugestões");
                throw;
            }
        }


        private static string BuildConversationContext(List<MensagemDTO> mensagens)
        {
            var sb = new StringBuilder();

            // Ordenar mensagens por data de envio
            var mensagensOrdenadas = mensagens.OrderBy(m => m.DataEnvio).ToList();

            foreach (var msg in mensagensOrdenadas)
            {
                // Determinar o autor baseado no tipo de remetente
                string autor = DeterminarAutor(msg.TipoRemetente);

                // Adicionar apenas mensagens com conteúdo textual
                if (!string.IsNullOrWhiteSpace(msg.Conteudo))
                {
                    sb.AppendLine($"{autor}: {msg.Conteudo}");
                }
                else if (msg.Midia)
                {
                    // Indicar que foi enviada uma mídia
                    var tipoMidia = msg.TipoMensagem?.ToLower() ?? "mídia";
                    sb.AppendLine($"{autor}: [{tipoMidia} enviada]");
                }
                else if (msg.Template)
                {
                    sb.AppendLine($"{autor}: [template enviado]");
                }
            }

            return sb.ToString();
        }

        private static string DeterminarAutor(char tipoRemetente)
        {
            // Ajustar conforme os valores reais do seu campo TipoRemetente
            return char.ToLower(tipoRemetente) switch
            {
                'r' => "CLIENTE",
                'e' => "VENDEDOR",
                _ => tipoRemetente.ToString().ToUpper() ?? "DESCONHECIDO"
            };
        }

        private static string BuildPromptSuggestions(List<MensagemDTO> mensagens, string prompt, string? rascunho = null)
        {
            var conversationContext = BuildConversationContext(mensagens);

            {

                return $"""
                    {prompt}

                    Contexto:
                    {conversationContext}
               
                    Caso o vendedor envie um rascunho, utilize ele como base também, melhorando e completando as ideias.
                    Rascunho do vendedor:
                    {rascunho ?? "Nenhum rascunho fornecido. Crie respostas baseadas apenas no contexto."}
                    """;

            }
        }

        private static List<string> ParseSuggestions(string response)
        {
            var suggestions = new List<string>();
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("1.") ||
                    trimmedLine.StartsWith("2.") ||
                    trimmedLine.StartsWith("3."))
                {
                    var suggestion = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            return suggestions.Count > 0 ? suggestions : new List<string> { response };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

}
