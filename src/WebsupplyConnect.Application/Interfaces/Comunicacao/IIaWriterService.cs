using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IIaWriterService
    {
        Task<List<MensagemSugestaoDTO>> GerarSugestoesPorEmpresa(SuggestionRequestDTO dto);
        Task<ResumoIaResponseDTO?> GerarResumoPorEmpresa(ResumoIaRequestDTO dto);
        Task<TranscricaoResponseDTO?> GerarTranscricaoAudio(TranscricaoAudioRequestDTO dto);
    }
}
