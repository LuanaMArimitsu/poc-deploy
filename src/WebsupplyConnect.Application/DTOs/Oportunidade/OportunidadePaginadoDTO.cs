namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class OportunidadePaginadoDTO
    {
        public List<GetOportunidadeDTO> Oportunidades { get; set; } = [];
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
