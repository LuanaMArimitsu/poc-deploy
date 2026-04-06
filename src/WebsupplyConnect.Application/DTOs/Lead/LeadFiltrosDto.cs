public class LeadFiltrosDto
{
    public int? LeadId { get; set; } /////caso lead id seja recebido todo o resto sera ignorado

    public int? EmpresaId { get; set; }

    public int? EquipeId { get; set; }

    // Busca textual global
    public string? TextoBusca { get; set; }

    // Filtros de responsabilidade
    public List<int>? ResponsavelIds { get; set; }
    public bool? MeusLeads { get; set; } // Se true, ignora ResponsavelIds e usa usuário atual

    // Filtros de relacionamento
    public bool? ComOportunidades { get; set; } // true = com, false = sem, null = todos

    // Filtros de status e origem
    public List<int>? StatusIds { get; set; }
    public List<int>? OrigemIds { get; set; }

    // Filtro por período (sempre Data de Criação)
    public PeriodoFiltroDto? PeriodoFiltro { get; set; }

    // Filtros de conversas
    public ConversasFiltroDto? ConversasFiltro { get; set; }

    // Filtros por identificadores
    public IdentificadoresFiltroDto? IdentificadoresFiltro { get; set; }

    // Paginação e ordenação
    public PaginacaoDto Paginacao { get; set; } = new();

    // Para aplicar filtro salvo
    public int? FiltroSalvoId { get; set; }
}

public class PeriodoFiltroDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}

public class ConversasFiltroDto
{
    public bool? ComConversasAtivas { get; set; }
    public bool? ComMensagensNaoLidas { get; set; }
    public bool? AguardandoResposta { get; set; }
}

public class IdentificadoresFiltroDto
{
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? CPF { get; set; }
}

public class PaginacaoDto
{
    public int Pagina { get; set; } = 1;
    public int? TamanhoPagina { get; set; } = null;
    public string OrderBy { get; set; } = "dataCriacao_desc"; // dataCriacao_desc, nome_asc, etc
}
