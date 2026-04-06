using WebsupplyConnect.Application.DTOs.Permissao.Permissao;

namespace WebsupplyConnect.Application.Interfaces.Perfil
{
    public interface IPermissaoReaderService
    {
        Task<IReadOnlyList<PermissaoDTO>> GetPermissoes();
        Task<PermissaoPaginadaDTO> GetPermissoes(PermissaoFiltroDTO filtro);
        Task<Domain.Entities.Permissao.Permissao?> GetPermissaoPorIdAsync(int permissaoId);
    }
}
