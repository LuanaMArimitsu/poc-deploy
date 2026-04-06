using WebsupplyConnect.Domain.Entities.Produto;

namespace WebsupplyConnect.Domain.Interfaces.Produto
{
    public interface IProdutoHistoricoRepository
    {
        Task AdicionarAsync(ProdutoHistorico historico);
    }
}
