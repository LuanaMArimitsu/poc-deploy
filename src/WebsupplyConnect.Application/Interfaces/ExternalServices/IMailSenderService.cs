namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IMailSenderService
    {
        Task EnviarAsync(string destinatarioEmail, string destinatarioNome, string assunto, string mensagemTexto, string? mensagemHtml = null, byte[] ? anexoBytes = null, string? nomeAnexo = null);
    }
}

