using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IWhatsAppMediaClient
    {
        Task<MidiaMetaDTO> GetMediaInfoAsync(string midiaId, string acessTokenMeta);
        Task<Stream> DownloadMediaAsync(MidiaMetaDTO mediaInfo, string whatsAppToken);
        Task<string?> EnviarMidiaParaMetaAsync(
            byte[] fileBytes,
            string mimeType,
            string fileName,
            string telefoneId,
            string token,
            string tipoMensagem);
    }
}
