using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Equipe
{
    public interface IEquipeRepository : IBaseRepository
    {
        Task<bool> ExistsNomeNaEmpresaAsync(string nome, int empresaId, int? ignorarEquipeId = null);
        Task<List<Entities.Equipe.Equipe>> GetByEmpresaAsync(int empresaId);
        Task<(List<Domain.Entities.Equipe.Equipe> Itens, int TotalItens)> ListarPorEmpresaFiltradoAsync(
            int empresaId,
            int? tipoEquipeId,
            bool? ativas,
            int? responsavelMembroId,
            string? busca,
            int? pagina = null,
            int? tamanhoPagina = null);

        Task<Entities.Equipe.Equipe?> GetByIdEquipeDetailsAsync (int equipeId);
        Task<List<WebsupplyConnect.Domain.Entities.Equipe.Equipe>> ListEquipesComResponsavelAsync(int empresaId);

        Task<List<Domain.Entities.Equipe.Equipe>> GetByEmpresaWithMembersAsync(int empresaId);
        Task<Domain.Entities.Equipe.Equipe?> GetEquipeIntegracaoPorEmpresaIdAsync(int empresaId);
        Task<Domain.Entities.Equipe.Equipe?> ObterEquipeOlxIdAsync(int empresaId);
        Task<List<Domain.Entities.Equipe.Equipe>> GetEquipeComMembrosByResponsavelAsync(List<int> responsavelMembroId);
    }
}
