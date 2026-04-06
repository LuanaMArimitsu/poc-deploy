using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Lead
{
    public interface ICampanhaRepository : IBaseRepository
    {
        Task<(List<Campanha> Itens, int TotalItens)> ListarCampanhasFiltroAsync(
            string? busca,
            int? empresaId,
            string? codigo,
            bool? ativa,
            bool? temporaria,
            int? equipeId,
            DateTime? dataCadastro,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? pagina,
            int? tamanhoPagina
        );

        Task<IEnumerable<Campanha>> ListagemSimplesAsync(int empresaId);
    }
}
