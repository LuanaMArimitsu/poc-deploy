namespace WebsupplyConnect.Application.DTOs.Permissao.Permissao
{
    public class PermissaoPaginadaDTO
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public IReadOnlyList<PermissaoDTO> Itens { get; set; }
    }
}
