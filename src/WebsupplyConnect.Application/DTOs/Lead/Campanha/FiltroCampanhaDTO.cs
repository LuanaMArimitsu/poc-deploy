namespace WebsupplyConnect.Application.DTOs.Lead.Campanha
{
    public class FiltroCampanhaDTO
    {
        public string? Busca { get; set; }
        public int? EmpresaId { get; set; }
        public string? Codigo { get; set; }
        public bool? Ativa { get; set; }
        public bool? Temporaria { get; set; }
        public int? EquipeId { get; set; }
        public DateTime? DataCadastro { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? Pagina { get; set; }
        public int? TamanhoPagina { get; set; }
    }
}
