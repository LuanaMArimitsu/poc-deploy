using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface IOrigemReaderService
    {
        /// <summary>
        /// Lista todas as origens disponíveis.
        /// </summary>
        /// <returns>Lista de OrigemDTO.</returns>
        Task<List<OrigemDTO>> ListarOrigensAsync();
        Task<OrigemDTO> GetOrigemByIdAsync(int id);
        Task<List<TipoOrigemDTO>> GetAllOrigemTiposAsync();
        Task<List<OrigemSimplesDTO>> ListarOrigensSimplesAsync();
        Task<Origem> GetOrigemByName(string name);
        /// <summary>Lista origens não excluídas para uso em ETL (sincronização dimensões).</summary>
        Task<List<Origem>> ListarOrigensNaoExcluidasParaETLAsync();

        /// <summary>Todas as origens transacionais para sincronizar exclusão lógica na dimensão OLAP.</summary>
        Task<List<Origem>> ListarTodasOrigensParaETLAsync();
    }
}