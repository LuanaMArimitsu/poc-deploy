using WebsupplyConnect.Application.DTOs.Produto;

namespace WebsupplyConnect.Application.Interfaces.Produto
{
    public interface IProdutoWriterService
    {
        Task<WebsupplyConnect.Domain.Entities.Produto.Produto> AdicionarProdutoAsync(AdicionarProdutoRequestDTO dto, int UsuarioId);
        Task VincularEmpresaAsync(VincularEmpresaProdutoRequestDTO dto, int usuarioId);
        Task ExcluirProdutoAsync(int produtoId, int usuarioId);
        Task RemoverEmpresaDoProdutoAsync(int produtoId, int empresaId, int usuarioId);
        Task AlterarStatusProdutoAsync(int produtoId, int usuarioId);     
        Task AtualizarInformacoesAsync(AtualizarProdutoRequestDTO dto, int usuarioId, int produtoId);
        Task AlterarValorProdutoEmpresaAsync(int produtoId, int empresaId, AlterarValorProdutoEmpresaRequestDTO dto, int usuarioId);
    }
}
