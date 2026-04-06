using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Equipe
{
    public interface IMembroEquipeRepository : IBaseRepository
    {
        Task<bool> ExistsAtivoAsync(int equipeId, int usuarioId);
        Task<bool> ExistsLiderAtivoAsync(int equipeId);
        Task<bool> EhUltimoAtivoAsync(int equipeId, int membroId);
        Task<MembroEquipe?> GetLiderDaEquipeAsync(int equipeId);
        Task<(IReadOnlyList<MembroEquipe> Itens, int Total)> ListarMembrosAsync(int equipeId, bool apenasAtivos, IEnumerable<int>? statusIds, string? buscaNome, int? pagina = null, int? tamanho = null);
        Task<MembroEquipe?> GetLiderAtivoByEquipeAsync(int equipeId, int? ignoreMembroId = null);
        Task<int> CountAtivosByEquipeAsync(int equipeId);
        Task<(int quantidadeRemovidos, List<MembroEquipe> membrosRemovidos)> SoftDeleteAllByEquipeAsync(int equipeId);
        Task SoftDeleteAsync(MembroEquipe membro);
        Task RestaurarMembro(MembroEquipe membro);
        Task<List<MembroEquipe>> ObterMembrosPorEquipeAsync(int equipeId, string? statusCodigo = null, int? statusId = null);

        /// <summary>
        /// Vendedores ativos (status ATIVO, não bot) nas equipes informadas — uma consulta.
        /// </summary>
        Task<List<MembroEquipe>> ObterVendedoresAtivosPorEquipeIdsAsync(IReadOnlyList<int> equipeIds);
        Task<MembroEquipe?> GetByIdComStatusAsync(int membroId);
        Task<List<MembroEquipe>> ObterVendedoresPorEquipeDisponiveisNoHorarioAsync(int diaSemana, TimeSpan horaAtual, List<int> membrosEquipeIds);
        Task<bool> VerificarAssociacaoEquipeAsync(int usuarioId, int empresaId);
        Task<MembroEquipe?> GetBotMembroEquipeAsync(int usuarioId, int empresaId);
        Task<MembroEquipe?> ObterMembroPorEmail(string email, int empresaId, string? statusCodigo = null);
        Task<MembroEquipe?> GetMembroPorUsuario(int usuarioId, int equipeId);
        Task<Dictionary<int, MembroEquipe>> GetLideresPorEquipesAsync(List<int> equipeIds);
    }
}
