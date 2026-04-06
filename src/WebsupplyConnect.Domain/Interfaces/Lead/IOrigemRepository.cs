using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Lead
{
    public interface IOrigemRepository : IBaseRepository
    {
        /// <summary>
        /// Lista todas as origens disponíveis.
        /// </summary>
        /// <returns>Lista de entidades Origem.</returns>
        Task<List<Origem>> ListarOrigensAsync();
        Task<Origem?> GetOrigemByIdAsync(int id);
        Task<Origem?> GetOrigemByName(string name);
    }
}