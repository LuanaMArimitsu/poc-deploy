using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.SendGrid
{
    public class MailSenderService : IMailSenderService
    {
        private readonly MailSenderOptions _options;
        private readonly ILogger<MailSenderService> _logger;

        public MailSenderService(IOptions<MailSenderOptions> options, ILogger<MailSenderService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task EnviarAsync(string destinatarioEmail, string destinatarioNome, string assunto, string mensagemTexto, string? mensagemHtml = null, byte[]? anexoBytes = null, string? nomeAnexo = null)
        {
            var client = new SendGridClient(_options.SGKey);
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(destinatarioEmail, destinatarioNome);
            var msg = MailHelper.CreateSingleEmail(from, to, assunto, mensagemTexto, mensagemHtml ?? mensagemTexto);

            if (anexoBytes != null && !string.IsNullOrWhiteSpace(nomeAnexo))
            {
                string base64File = Convert.ToBase64String(anexoBytes);
                msg.AddAttachment(nomeAnexo, base64File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao enviar e-mail para {Email}. Status: {StatusCode}", destinatarioEmail, response.StatusCode);
            }
            else
            {
                _logger.LogInformation("E-mail enviado para {Email} com sucesso.", destinatarioEmail);
            }
        }
    }
}
