namespace WebsupplyConnect.Application.Interfaces.Produto
{
    public interface IProdutoHistoricoWriterService
    {
        Task RegistrarAsync(int produtoId, int usuarioId, int tipoOperacaoId, string descricao, object? detalhes = null);
    }
}
