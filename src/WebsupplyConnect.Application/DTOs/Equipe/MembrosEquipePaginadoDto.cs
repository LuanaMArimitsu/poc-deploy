namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class MembrosEquipePaginadoDto
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public List<ListMembroEquipeDto> Itens { get; set; } = new();
    }
}
