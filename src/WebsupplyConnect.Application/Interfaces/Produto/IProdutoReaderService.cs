using WebsupplyConnect.Application.DTOs.Produto;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Produto
{
    public interface IProdutoReaderService
    {      
        Task<PagedResponseDTO<ProdutoListagemDTO>> ListarProdutosPaginadoAsync(ProdutoFiltroRequestDTO filtro);
        Task<ProdutoDetalhadoDTO> ObterDetalhadoAsync(int id);
    }
}
