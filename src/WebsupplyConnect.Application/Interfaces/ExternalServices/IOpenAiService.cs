using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Empresa;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IOpenAiService
    {
        public void GetConfig(Openai config);
        Task<List<string>> GenerateSuggestionsAsync(List<MensagemDTO> mensagens, Openai config, string prompt, string promptSistema, string? rascunho = null);
        Task<string> GenerateResumoAsync(List<MensagemDTO> mensagens, Openai config, string prompt, string promptSistema);
        Task<string> GenerateClassificacaoAsync(Openai config, string promptSistema, string payloadJson);
        Task<string> GenerateTranscricaoAsync(Byte[] audioBytes, string tipoAudio, Openai config);
    }
}
