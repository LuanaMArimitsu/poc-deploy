namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class EquipeFiltroRequestDto
    {
        public int EmpresaId { get; set; }
        public int? TipoEquipeId { get; set; }
        public bool? Ativa { get; set; }
        public int? ResponsavelMembroId { get; set; }
        public string? Busca { get; set; }

        public int? Pagina { get; set; } 
        public int? TamanhoPagina { get; set; } 
    }
}
