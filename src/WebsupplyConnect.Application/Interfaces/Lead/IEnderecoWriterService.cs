using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface IEnderecoWriterService
    {
        Task<int> CriarEnderecoAsync(Endereco endereco);
        Task<bool> ExcluirEnderecoAsync(int enderecoId);
        Task EditarEnderecoAsync(EditarEnderecoDTO dto);
    }
}