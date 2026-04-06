namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadFiltroRequestDTO
    {
        public int? OrigemId { get; set; }
        public int? StatusId { get; set; }
        public int? UsuarioId { get; set; }
        public int? EmpresaId { get; set; }
        public string? NumeroWhatsapp { get; set; }
        public DateTime? DataCadastroInicio { get; set; }
        public DateTime? DataCadastroFim { get; set; }
        public string? NivelInteresse { get; set; }
        public int? Pagina { get; set; }
        public int? TamanhoPagina { get; set; } 
        public string? Busca { get; set; }
    }
}