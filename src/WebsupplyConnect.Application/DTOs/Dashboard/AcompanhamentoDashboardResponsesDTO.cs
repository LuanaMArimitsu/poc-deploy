namespace WebsupplyConnect.Application.DTOs.Dashboard;

public static class AcompanhamentoDashboardTipoPendenciaConstantes
{
    public const string PrimeiroContato = "PRIMEIRO_CONTATO";
    public const string AguardandoResposta = "AGUARDANDO_RESPOSTA";
}

public class AcompanhamentoDashboardKpisDTO
{
    public int LeadsRecebidosHoje { get; set; }
    public int LeadsSemana { get; set; }
    public int ConversasAtivas { get; set; }
    public int LeadsEmNegociacao { get; set; }
    public int LeadsConvertidos { get; set; }
    public int LeadsPendentesAtendimento { get; set; }
}

public class AcompanhamentoDashboardAgregadoResponseDTO
{
    public AcompanhamentoDashboardKpisDTO Kpis { get; set; } = new();
    public DateTime? UltimaAtualizacao { get; set; }
}

/// <summary>Último evento associado ao lead, com campanha quando existir.</summary>
public class AcompanhamentoDashboardUltimoEventoDTO
{
    public int Id { get; set; }
    public DateTime DataEvento { get; set; }
    /// <summary>Nome da campanha do evento, quando existir.</summary>
    public string? NomeCampanha { get; set; }
}

public class AcompanhamentoDashboardLeadPendenteItemDTO
{
    public int LeadId { get; set; }
    public string NomeLead { get; set; } = string.Empty;
    public DateTime? DataUltimoEvento { get; set; }
    public string TipoPendencia { get; set; } = string.Empty;
    public string TipoPendenciaLabel { get; set; } = string.Empty;
    public string? TipoPendenciaCor { get; set; }
    public string NomeOrigem { get; set; } = string.Empty;
    public string? NomeCampanha { get; set; }
    /// <summary>Último evento associado ao lead, com campanha quando existir.</summary>
    public AcompanhamentoDashboardUltimoEventoDTO? UltimoEvento { get; set; }
    public string? UltimaMensagemCliente { get; set; }
    public int MensagensNaoLidas { get; set; }
    public int TempoSemAcaoMinutos { get; set; }
    /// <summary>Tempo sem ação formatado para exibição (ex.: 2h02min).</summary>
    public string TempoSemAcaoLabel { get; set; } = "0min";
    /// <summary>Minutos entre a data da última mensagem do cliente e a data/hora atual (tempo aguardando resposta do vendedor).</summary>
    public int TempoAguardandoRespostaMinutos { get; set; }
    /// <summary>Tempo aguardando resposta formatado para exibição (ex.: 2h02min).</summary>
    public string TempoAguardandoRespostaLabel { get; set; } = "0min";
    public int? ConversaAtivaId { get; set; }
    /// <summary>Indica se a última mensagem da conversa ativa é do vendedor/atendimento (ex.: template), aguardando resposta do cliente.</summary>
    public bool PendenteRespostaCliente { get; set; }
}

public class AcompanhamentoDashboardConversaAtivaItemDTO
{
    public int ConversaAtivaId { get; set; }
    public int LeadId { get; set; }
    public string NomeLead { get; set; } = string.Empty;
    public string? ProdutoInteresse { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public string? StatusCor { get; set; }
    public bool TrocaDeContato { get; set; }
    public string? UltimaMensagemCliente { get; set; }
    public string UltimaMensagemEnviadaPor { get; set; } = string.Empty;
    public DateTime? DataUltimaMensagem { get; set; }
    public DateTime? DataHoraUltimaMensagem { get; set; }
    public int TempoMedioAtendimentoMinutos { get; set; }
    /// <summary>Tempo médio de atendimento formatado para exibição (ex.: 2h02min).</summary>
    public string TempoMedioAtendimentoLabel { get; set; } = "0min";
    public int MensagensNaoLidas { get; set; }
}

public class AcompanhamentoDashboardConversaClassificacaoResponseDTO
{
    public int ConversaId { get; set; }
    public string? Contexto { get; set; }
    public string? CategoriaIA { get; set; }
    public DateTime? DataAtualizacaoContexto { get; set; }
}

public class AcompanhamentoDashboardConversaContextoDTO
{
    public string ContextoResumo { get; set; } = string.Empty;
    public string Pendencia { get; set; } = string.Empty;
    public string AcaoVendedor { get; set; } = string.Empty;
}
