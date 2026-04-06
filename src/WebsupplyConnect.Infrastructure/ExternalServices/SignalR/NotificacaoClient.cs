using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.ExternalServices.SignalR
{
    public class NotificacaoClient : INotificacaoClient
    {
        private readonly HttpClient _httpClient;
        private readonly APIsConnectConfig _config;
        private readonly ILogger<NotificacaoClient> _logger;

        public NotificacaoClient(HttpClient httpClient, IOptions<APIsConnectConfig> config, ILogger<NotificacaoClient> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_config.UrlBaseAPIsConnect);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
        }


        public async Task<HttpResponseMessage> NovoLead(NotificarNovoLeadDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/NovoLead", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar NovoLead. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de novo lead. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> NovoLeadVendedor(NotificarNovoLeadVendedorDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/NovoLeadVendedor", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar NovoLead. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de novo lead. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> LeadAlterado(NotificarNovoLeadDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/LeadAtualizado", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar LeadAlterado. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de lead alterado. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> NovaMensagem(NotificarNovaMensagemDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/NovaMensagem", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar NovaMensagem. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de nova mensagem. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> AtualizarMensagemStatus(NotificarStatusMensagemAtualizadoDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/AtualizarMensagemStatus", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar AtualizarMensagemStatus. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de atualização de status da mensagem. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> EscalonamentoAutomaticoLider(NotificacaoEscalonamentoDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/EscalonamentoAutomaticoLider", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar escalonamento automático. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de escalonamento automático ao lider. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> EscalonamentoAutomaticoVendedor(NotificacaoEscalonamentoDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/EscalonamentoAutomaticoVendedor", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar escalonamento automático. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de escalonamento automático ao vendedor. Detalhes: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> NovoLeadEvento(NotificarNovoLeadDTO request)
        {
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notificacao/LeadEvento", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InfraException(
                        $"Erro ao chamar NovoLeadEvento. StatusCode: {response.StatusCode}, Conteúdo: {content}");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao chamar API de notificação de novo lead evento. Detalhes: {ex}", ex.Message);
                throw;
            }
        }
    }
}
