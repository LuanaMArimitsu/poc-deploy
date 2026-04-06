using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Equipe
{
    public interface IMembroEquipeWriterService
    {
        Task<int> AddMembroAsync(int equipeId, AdicionarMembroDto dto);
        Task<int> AtualizarStatusAsync(AtualizarMembroEquipeDto dto);
        Task<(int? LiderAnteriorMembroId, int NovoLiderMembroId)> TransferirLiderancaAsync(TransferirLiderancaRequestDto dto);        
        Task<(int quantidadeRemovidos, List<MembroEquipe> membrosRemovidos)> DeleteTodosDaEquipeAsync(int equipeId);
        Task DeleteMembroAsync(int membroId);
        Task<MembroEquipe> GetMembroEquipePorEmail(string email, int empresaId, string? statusCodigo = null);
    }
}
