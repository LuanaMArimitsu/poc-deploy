namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IWhatsAppClient
    {
        Task<HttpResponseMessage> EnviarMensagemTextoAsync(string telefoneDestino, string mensagem, string token, string telefoneId);
        Task<HttpResponseMessage> EnviarTemplateMontadoAsync(object corpoTemplate, string token, string telefoneId);
        Task<HttpResponseMessage> EnviarMidiaPorIdAsync(string telefoneDestino, string tipoMidia, string mediaMetaId, string token, string telefoneId, string filename, string? caption = null);
        Task<HttpResponseMessage> MarcarMensagemComoLidaAsync(string mensagemMetaId, string token, string telefoneId);
    }
}

