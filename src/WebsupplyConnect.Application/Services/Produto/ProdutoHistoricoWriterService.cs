using WebsupplyConnect.Application.Interfaces.Produto;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Produto;

namespace WebsupplyConnect.Application.Services.Produto
{
    public class ProdutoHistoricoWriterService(
        IUnitOfWork unitOfWork,
        IProdutoHistoricoRepository produtoHistoricoRepository
    ) : IProdutoHistoricoWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IProdutoHistoricoRepository _produtoHistoricoRepository = produtoHistoricoRepository;

        public async Task RegistrarAsync(int produtoId, int usuarioId, int tipoOperacaoId, string descricao, object? detalhes = null)
        {
            var historico = new ProdutoHistorico(produtoId, usuarioId, tipoOperacaoId, descricao, detalhes);
            await _produtoHistoricoRepository.AdicionarAsync(historico);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
