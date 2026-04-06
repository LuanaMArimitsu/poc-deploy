using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Permissao
{
    public interface IPermissaoRepository : IBaseRepository
    {
        Task<(IReadOnlyList<Domain.Entities.Permissao.Permissao> Itens, int TotalItens)> GetPermissoesAsync(string nome,
        string modulo,
        bool criticas,
        string categoria,
        int pagina,
        int tamanhoPagina);
    }
}
