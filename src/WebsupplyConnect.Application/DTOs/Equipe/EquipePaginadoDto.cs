namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class EquipePaginadoDto
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public List<ListEquipeDto> Itens { get; set; } = new();
    }
}
