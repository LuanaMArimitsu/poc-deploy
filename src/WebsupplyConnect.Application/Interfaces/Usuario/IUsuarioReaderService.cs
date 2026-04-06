using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;

namespace WebsupplyConnect.Application.Interfaces.Usuario
{
    public interface IUsuarioReaderService
    {
        Task<Domain.Entities.Usuario.Usuario?> GetUsuarioByEmail(string email);
        Task<List<AzureUserDTO>> BuscarUsuariosAzureAdPorNome(string? startsWith);
        Task<Domain.Entities.Usuario.Usuario?> GetUsuarioBot();
        Task<UsuarioDetalheSimplesDTO?> ObterUsuarioDetalhadoSimplesAsync(int id);
        Task<UsuarioDetalheDTO?> ObterUsuarioDetalhadoAsync(int id);
        Task<List<UsuarioSuperiorDTO>> ObterUsuariosSuperioresAsync();
        Task<List<UsuarioEmpresaDTO>?> ObterEmpresasUsuarioAsync(int usuarioId);
        Task<bool> UserExistsAsync(int usuarioId);
        Task<PagedResponseDTO<UsuarioListagemDTO>> ListarUsuariosAsync(UsuarioFiltroRequestDTO filtro);
        Task<List<UsuarioHorarioDTO>> ObterHorariosUsuarioAsync(int usuarioId);
        Task<Dictionary<int, List<UsuarioHorarioDTO>>> ObterHorariosMultiplosUsuariosAsync(IEnumerable<int> usuarioIds);
        Task<List<UsuarioSimplesDTO>> UsuariosEmpresa(int empresaId);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterUsuarioPorIdAsync(int id);
        Task<(List<WebsupplyConnect.Domain.Entities.Usuario.Usuario> Vendedores, bool FallbackAplicado, string? DetalhesFallback)> ObterVendedoresDisponiveisAsync(int empresaId, ConfiguracaoDistribuicao configuracao);
        /// <summary>Lista usuários ativos (não excluídos, não bot) para uso em ETL (sincronização dimensão vendedor).</summary>
        Task<List<WebsupplyConnect.Domain.Entities.Usuario.Usuario>> ObterUsuariosAtivosNaoBotParaETLAsync();
        /// <summary>Retorna o conjunto de IDs de usuários que são bot (IsBot=true).</summary>
        Task<HashSet<int>> ObterBotUserIdsAsync();
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterVendedorPorMembroId(int usuarioId);
    }
}
