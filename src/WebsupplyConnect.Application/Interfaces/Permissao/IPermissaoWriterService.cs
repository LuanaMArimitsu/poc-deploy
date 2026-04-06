using WebsupplyConnect.Application.DTOs.Permissao.Permissao;

namespace WebsupplyConnect.Application.Interfaces.Permissao
{
    public interface IPermissaoWriterService
    {
        Task CriarPermissaoAsync(CreatePermissaoDTO dto);

        Task ExcluirPermissaoAsync(int permissaoId);

        Task AtualizarPermissaoAsync(int permissaoId, string nome, string descricao, bool isCritica);

        Task DesativarPermissaoAsync(int permissaoId);

        Task ReativarPermissaoAsync(int permissaoId);
    }
}
