using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface IOrigemWriterService
    {
        Task CreateAsync(OrigemRequest request);
        Task UpdateOrigemAsync(int id, UpdateOrigemDTO updateOrigemDTO);
        Task DeleteAsync(int id);
    }
}
