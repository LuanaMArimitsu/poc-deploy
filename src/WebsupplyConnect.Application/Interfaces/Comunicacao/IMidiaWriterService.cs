using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IMidiaWriterService
    {
        Task<Midia> ProcessarMidiaOutboundAsync(MensagemRequestDTO dto, Mensagem mensagem);
        Task<Midia> ProcessarMidiaInboundAsync(MidiaInboundDTO dto);
        Task UpdateMidiaStorageAsync(int midiaId, string urlStorage, string? thumbnail);
        Task UpdateStatusProcessamentoMidiaAsync(int midiaId, int novoStatusId);
        Task UpdateMidiaDadosMetaAsync(int midiaId, long size, string mediaIdMeta);
        Task<Midia> CreateMidiaAsync(Midia midia);
        Task SalvarTranscricaoAsync(int midiaId, string transcricao);
    }
}
