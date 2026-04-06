namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class MembrosEquipeFiltroRequestDto
    {
        public int EquipeId { get; set; }
        public bool? ApenasAtivos { get; set; } = true;
        public List<int>? StatusIds { get; set; }  
        public string? Busca { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; } 
        public int EmpresaId { get; set; }
    }
}
