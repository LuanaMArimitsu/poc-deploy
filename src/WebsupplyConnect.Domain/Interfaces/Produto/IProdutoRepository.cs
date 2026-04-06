using WebsupplyConnect.Domain.Entities.Produto;

namespace WebsupplyConnect.Domain.Interfaces.Produto
{
    public interface IProdutoRepository
    {
        Task AdicionarAsync(WebsupplyConnect.Domain.Entities.Produto.Produto produto);
        Task<WebsupplyConnect.Domain.Entities.Produto.Produto> ObterComEmpresasAsync(int produtoId);
        Task<WebsupplyConnect.Domain.Entities.Produto.Produto?> ObterPorIdAsync(int id);
        Task AtualizarAsync(WebsupplyConnect.Domain.Entities.Produto.Produto produto);
        Task RemoverVinculosEmpresasAsync(int produtoId);
        Task RemoverVinculoEmpresaAsync(int produtoId, int empresaId);
        IQueryable<WebsupplyConnect.Domain.Entities.Produto.Produto> ObterQueryProdutosComEmpresas(int empresaId);
        Task<WebsupplyConnect.Domain.Entities.Produto.Produto?> ObterDetalhePorIdAsync(int id);
        Task<ProdutoEmpresa?> ObterVinculoProdutoEmpresaAsync(int produtoId, int empresaId);
        Task AtualizarVinculoProdutoEmpresaAsync(ProdutoEmpresa vinculo);
    }
}
