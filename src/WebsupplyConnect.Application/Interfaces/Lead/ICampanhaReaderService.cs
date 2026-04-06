using WebsupplyConnect.Application.DTOs.Lead.Campanha;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ICampanhaReaderService
    {
        Task<CampanhaPaginadaDTO> ListarCampanhasAsync(FiltroCampanhaDTO filtroCampanhaDTO);
        Task<Campanha?> CampanhaExistsByCodigoAsync(string codigo, int empresaId);
        Task<IEnumerable<ListCampanhaResponseDTO>> ListagemSimplesAsync(int empresaId);
        Task<Campanha> CampanhaExistsByIdAsync(int campanhaId, int empresaId);
        Task<CampanhaDTO> ListarCampanhaByIdAsync(int campanhaId);
        /// <summary>Lista campanhas não excluídas para uso em ETL (sincronização dimensões).</summary>
        Task<List<Campanha>> ListarCampanhasNaoExcluidasParaETLAsync();
    }
}