using WebsupplyConnect.Application.DTOs.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Equipe
{
    public interface IEquipeWriterService
    {
        Task<int> CreateEquipe(CriarEquipeDto dto);
        Task UpdateEquipeAsync(int id, AtualizarEquipeDto dto);
        Task DeleteEquipeAsync(int id);
    }
}
