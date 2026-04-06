using WebsupplyConnect.Application.DTOs.Comum;

namespace WebsupplyConnect.Application.Interfaces.Comum
{
    public interface IFeriadoWriterService
    {
        Task<FeriadoDTO> AdicionarAsync(FeriadoCriarDTO feriadoDTO);
        Task<FeriadoDTO> AtualizarAsync(FeriadoAtualizarDTO feriadoDTO);
        Task<bool> RemoverAsync(int id);
    }
}
