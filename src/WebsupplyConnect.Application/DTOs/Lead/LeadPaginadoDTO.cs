namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadPaginadoDTO
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int? TamanhoPagina { get; set; }
        public int TotalPaginas { get; set; }
        public IEnumerable<LeadRetornoDTO> Itens { get; set; }

        public LeadFiltrosAplicadosDto? FiltrosAplicados { get; set; }
    }

    public class LeadFiltrosAplicadosDto
    {
        public bool MeusLeads { get; set; }
        public List<string>? Status { get; set; }
        public List<string>? Origens { get; set; }
        public List<string>? Responsaveis { get; set; }
        public string? Periodo { get; set; } // Ex: "Hoje", "Últimos 7 dias", "01/01/2024 - 31/01/2024"
        public bool? ComOportunidades { get; set; }
        public bool? ComConversasAtivas { get; set; }
        public bool? ComMensagensNaoLidas { get; set; }
    }
}
