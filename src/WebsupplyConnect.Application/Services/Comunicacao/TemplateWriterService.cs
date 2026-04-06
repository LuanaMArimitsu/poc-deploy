using System.Text.Json;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class TemplateWriterService(ILogger<TemplateWriterService> logger, IWhatsAppClient whatsAppClient) : ITemplateWriterService
    {
        private readonly ILogger<TemplateWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IWhatsAppClient _whatsAppClient = whatsAppClient ?? throw new ArgumentNullException(nameof(whatsAppClient));
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public object MontarJsonTemplateMeta(string nomeTemplateMeta, string numeroRemetente)
        {
            return new
            {
                messaging_product = "whatsapp",
                to = $"{numeroRemetente}",
                type = "template",
                template = new
                {
                    name = nomeTemplateMeta,
                    language = new { code = "pt_BR" },
                }
            };
        }

        public object MontarJsonTemplateIntegracao(string nomeTemplateMeta, string numeroRemetente, List<TemplateParamIntegracao> templateParamIntegracaos)
        {
            return new
            {
                messaging_product = "whatsapp",
                to = $"{numeroRemetente}",
                type = "template",
                template = new
                {
                    name = nomeTemplateMeta,
                    language = new { code = "pt_BR" },
                    components = templateParamIntegracaos.Select(param => new
                    {
                        type = "body",
                        parameters = new[]
                        {
                            new
                            {
                                type = param.Tipo,
                                text = param.Parametro
                            }
                        }
                    }).ToArray()
                }
            };
        }

        public string MontarPreviewTemplate(string conteudoTemplate, List<TemplateParamIntegracao> parametros)
        {
            if (string.IsNullOrWhiteSpace(conteudoTemplate))
                return string.Empty;

            var mensagem = conteudoTemplate;

            for (int i = 0; i < parametros.Count; i++)
            {
                var placeholder = $"{{{{{i + 1}}}}}";
                mensagem = mensagem.Replace(placeholder, parametros[i].Parametro ?? string.Empty);
            }

            return mensagem;
        }

        public async Task<string> EnviarTemplateAsync(string nomeTemplate, string numeroRemetente, string token, string telefoneId)
        {
            try
            {
                var corpoTemplate = MontarJsonTemplateMeta(nomeTemplate, numeroRemetente);

                var response = await _whatsAppClient.EnviarTemplateMontadoAsync(corpoTemplate, token, telefoneId);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao enviar template {template} para {numero}. Status: {status}. Erro: {erro}",
                        nomeTemplate, numeroRemetente, response.StatusCode, erro);

                    throw new AppException($"Falha ao enviar template para {numeroRemetente}. Erro: {erro}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseMeta = JsonSerializer.Deserialize<ResponseEnvioMetaDTO>(responseContent, _jsonOptions) ?? throw new AppException("Erro ao desserializar o response da Meta do envio do template.");

                return responseMeta.messages?.FirstOrDefault()?.id
                    ?? throw new AppException("ID da mensagem Meta não encontrado no retorno.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar template {template} para {numero}", nomeTemplate, numeroRemetente);
                throw;
            }
        }

        public async Task<string> EnviarTemplateAsync(string nomeTemplate, string numeroRemetente, string token, string telefoneId, object corpoTemplate)
        {
            try
            {
                var response = await _whatsAppClient.EnviarTemplateMontadoAsync(corpoTemplate, token, telefoneId);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao enviar template {template} para {numero}. Status: {status}. Erro: {erro}",
                        nomeTemplate, numeroRemetente, response.StatusCode, erro);

                    throw new AppException($"Falha ao enviar template para {numeroRemetente}. Erro: {erro}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseMeta = JsonSerializer.Deserialize<ResponseEnvioMetaDTO>(responseContent, _jsonOptions) ?? throw new AppException("Erro ao desserializar o response da Meta do envio do template.");

                return responseMeta.messages?.FirstOrDefault()?.id
                    ?? throw new AppException("ID da mensagem Meta não encontrado no retorno.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar template {template} para {numero}", nomeTemplate, numeroRemetente);
                throw;
            }
        }
    }
}
