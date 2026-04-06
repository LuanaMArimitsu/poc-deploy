using WebsupplyConnect.Application.DTOs.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Equipe
{
    public interface IEquipeReaderService
    {
        Task<EquipePaginadoDto> ListarPorEmpresaAsync(EquipeFiltroRequestDto filtro);
        Task<List<Domain.Entities.Equipe.Equipe>> GetEquipesByEmpresaId(int empresaId);
        Task<ListDetalheEquipeDto> GetByIdAsync(int id);
        Task<Domain.Entities.Equipe.Equipe?> GetEquipePadraoAsync(int empresaId);
        Task<List<ResponsaveisPorEmpresaDto>> ListarResponsaveisPorEmpresaAsync(int empresaId);
        Task<Domain.Entities.Equipe.Equipe?> GetEquipeByIdAsync(int id);
        Task<List<EquipeSimplesDto>> ListaSimplesPorEmpresaAsync(int empresaId);
        Task<Domain.Entities.Equipe.Equipe?> GetEquipeIntegracaoPorEmpresaIdAsync(int empresaId);
        Task<Domain.Entities.Equipe.Equipe?> ObterEquipeOlxIdAsync(int empresaId);
        /// <summary>Lista equipes não excluídas para uso em ETL (sincronização dimensões).</summary>
        Task<List<Domain.Entities.Equipe.Equipe>> ObterEquipesNaoExcluidasParaETLAsync();
        /// <summary>Obtém equipes da empresa com membros para uso em ETL (sincronização dimensão vendedor).</summary>
        Task<List<Domain.Entities.Equipe.Equipe>> ObterEquipesComMembrosPorEmpresaParaETLAsync(int empresaId);
        Task<List<ListMembroEEquipeDTO>> ListarMembrosEEquipesByResponsavelAsync(int usuarioId);
        Task<List<Domain.Entities.Equipe.Equipe>> GetListEquipesByEmpresasIds(List<int> empresasIds);
        Task<List<EquipeListagemSimplesDto>> ListarSimplesPorEmpresaIdAsync(int empresaId);
    }
}
