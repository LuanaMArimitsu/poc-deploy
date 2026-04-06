namespace WebsupplyConnect.Application.DTOs.Lead.Campanha
{
    public class CampanhaPaginadaDTO
    {
        public int TotalItens { get; set; }
        public int? PaginaAtual { get; set; }
        public int? TotalPaginas { get; set; }
        public IEnumerable<CampanhaDTO> Itens { get; set; }
    }
}
