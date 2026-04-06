using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Usuario
{
    public interface IUsuarioRepository : IBaseRepository
    {
        /// <summary>
        /// Busca um usuario pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="includeDeleted">Se deve incluir usuários excluídos</param>
        /// <returns>Usuário encontrado ou null</returns>
        Task<int> GetUsuarioResponsavelByIdAsync(int id, bool includeDeleted = false);
        Task RemoverUsuarioEmpresaAsync(int usuarioId, int empresaId);
        Task<bool> ExisteUsuarioComAzureIdAsync(string azureId);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> BuscarUsuarioPorObjectIdAsync(string objectId);
        Task<Domain.Entities.Usuario.Usuario?> GetEmpresaByUsuarioId(int usuarioId);
        Task AdicionarAsync(Entities.Usuario.Usuario usuario);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> BuscarUsuarioPorIdAsync(int id);
        Task AtualizarAsync(WebsupplyConnect.Domain.Entities.Usuario.Usuario? usuario);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> GetUsuarioByIdWithConversasAsync(int id);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> GetUsuarioByIdWithLeadsAsync(int id);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterNovoResponsavelAsync(int idUsuarioDesativado);
        Task<List<UsuarioEmpresa>?> ObterEmpresasPorUsuarioIdAsync(int usuarioId);
        Task AdicionarUsuarioEmpresaAsync(UsuarioEmpresa associacao);
        Task RemoverAssociacoesPorUsuarioIdAsync(int usuarioId);
        void RemoverVinculoEmpresa(UsuarioEmpresa vinculo);
        IQueryable<WebsupplyConnect.Domain.Entities.Usuario.Usuario> ObterQueryUsuariosComEmpresa();
        Task<(List<WebsupplyConnect.Domain.Entities.Usuario.Usuario> itens, int totalItens)> ObterUsuariosComFiltros(int? empresaId = null, string? nome = null, bool? ativo = null, int? tamanhoPagina = null, int? pagina = null);
        Task<List<WebsupplyConnect.Domain.Entities.Usuario.Usuario>> ObterUsuariosComSubordinadosAsync();
        Task<List<WebsupplyConnect.Domain.Entities.Usuario.Usuario>> ObterUsuariosPorEmpresaAsync(int empresaId);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterUsuarioPorIdAsync(int id);
        Task<List<WebsupplyConnect.Domain.Entities.Usuario.Usuario>> ObterVendedoresAtivosPorEmpresaAsync(int empresaId);
        Task<List<WebsupplyConnect.Domain.Entities.Usuario.Usuario>> ObterVendedoresDisponiveisNoHorarioAsync(int empresaId, int diaSemana, TimeSpan horaAtual);
        Task AtualizarUsuarioEmpresaAsync(UsuarioEmpresa usuarioEmpresa);
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario?> ObterUsuarioPorMembroId(int membroId);
    }
}
