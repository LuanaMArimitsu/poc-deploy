using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class WebhookReaderService(
        ICanalReaderService canalReaderService,
        ILogger<WebhookReaderService> logger) : IWebhookReaderService
    {
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly ILogger<WebhookReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<AssinaturaMetaValidacaoResult> IsValid(string payload, string assinaturaRecebida)
        {
            try
            {
                var assinaturas = await _canalReaderService.GetlistaConfiguracaoIntegracao();

                if (assinaturas == null || assinaturas.Count == 0)
                    throw new AppException("Nenhuma configuração de integração encontrada.");

                var assinaturaNormalizada = assinaturaRecebida.Replace("sha256=", "", StringComparison.OrdinalIgnoreCase);

                foreach (var assinaturaJson in assinaturas)
                {
                    var config = JsonSerializer.Deserialize<CanalConfigDTO>(assinaturaJson);
                    if (config == null || string.IsNullOrWhiteSpace(config.Assinatura))
                        continue;
                    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.Assinatura));
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    var signatureCalculada = BitConverter.ToString(hash).Replace("-", "").ToLower();

                    if (signatureCalculada.Equals(assinaturaNormalizada, StringComparison.OrdinalIgnoreCase))
                    {
                        return new AssinaturaMetaValidacaoResult(true, config.Assinatura);
                    }
                }

                return new AssinaturaMetaValidacaoResult(false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar assinatura do webhook.");
                throw new AppException("Erro ao validar assinatura do webhook.", ex);
            }
        }
    }
}
