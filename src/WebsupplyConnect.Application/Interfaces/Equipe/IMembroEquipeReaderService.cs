using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Equipe
{
    public interface IMembroEquipeReaderService
    {
        Task<MembrosEquipePaginadoDto> ListarMembrosAsync(MembrosEquipeFiltroRequestDto filtro);
        Task<List<MembroEquipe>> ObterMembrosPorEquipe(int empresaId, string? statusCodigo = null, int? statusId = null);
        Task<MembroEquipe> ObterLiderDaEquipeAsync(int equipeId);
        Task<List<MembroEquipe>> ObterMembrosPorUsuarioAsync(int usuarioId, int? equipeId = null);
        Task<MembroEquipe?> GetByIdAsync(int membroId);
        Task<(List<MembroEquipe> Vendedores, bool FallbackAplicado, string? DetalhesFallback)> ObterVendedoresDisponiveisPorEquipeAsync(int equipeId, ConfiguracaoDistribuicao configuracao);
        Task<bool> VerificarAssociacaoUsuarioEmpresaAsync(int usuarioId, int empresaId);
        Task<MembroEquipe> GetBotMembroEquipeAsync(int usuarioId, int empresaId);
        Task<Dictionary<int, MembroEquipe>> ObterLideresDaEquipeAsync(List<int> equipeIds);
        Task<List<MembroEquipe>> ObterMembrosPorUsuarioIsLiderAsync(int usuarioId);

        /// <summary>
        /// Vendedores ativos nas equipes informadas (mesmos critérios de <see cref="ObterMembrosPorEquipe"/> com status ATIVO).
        /// O mesmo usuário pode aparecer mais de uma vez (uma por equipe).
        /// </summary>
        Task<List<MembroEquipe>> ObterVendedoresAtivosPorEquipeIdsAsync(IReadOnlyList<int> equipeIds);
    }
}
