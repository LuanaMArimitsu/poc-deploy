using Microsoft.AspNetCore.Http;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IMidiaReaderService
    {
        Task<Midia> GetMidiaByIdAsync(int midiaId);
        Task<MidiaStatusProcessamento> GetMidiaStatusProcessamentoAsync(string codigo);
        Task<Midia> GetMidiaByMensagemIdAsync(int mensagemId);
        public ResultadoValidacaoArquivo ValidarArquivo(IFormFile arquivo);
        Task<string?> GetTranscricaoByMensagemIdAsync(int mensagemId);
    }
}
