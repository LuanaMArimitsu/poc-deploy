namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardListagemLeadsDTO
{
    public int LeadId { get; set; }
    /// <summary>ID da empresa (transacional) do lead.</summary>
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string NomeStatus { get; set; } = string.Empty;
    public string NomeOrigem { get; set; } = string.Empty;

    /// <summary>
    /// Nome da equipe responsável pelo lead (baseado no evento de lead mais recente)
    /// </summary>
    public string? NomeEquipe { get; set; }

    /// <summary>
    /// Nome do responsável/vendedor (baseado no evento de lead mais recente:
    /// se o evento tem oportunidade, usa o responsável da oportunidade; senão, usa o responsável do lead)
    /// </summary>
    public string? NomeResponsavel { get; set; }

    /// <summary>
    /// Nome resumido do responsável/vendedor (primeiro nome + sobrenomes abreviados + último sobrenome, máx. 20 caracteres)
    /// </summary>
    public string? NomeResponsavelResumido { get; set; }

    /// <summary>
    /// Nome da campanha associada ao evento de lead mais recente que possui campanha
    /// </summary>
    public string? NomeCampanha { get; set; }

    /// <summary>
    /// Produto de interesse (nome do produto da oportunidade criada pelo evento de lead)
    /// </summary>
    public string? ProdutoInteresse { get; set; }

    /// <summary>
    /// Total de oportunidades vinculadas ao lead
    /// </summary>
    public int TotalOportunidades { get; set; }

    /// <summary>
    /// Quantidade de mensagens do cliente (Sentido='R') com StatusId=25 (não lidas pelo vendedor)
    /// </summary>
    public int MensagensNaoLidas { get; set; }

    /// <summary>
    /// Tempo médio de resposta do vendedor em minutos (calculado somente em horário de trabalho: Seg-Sex 8h-18h)
    /// </summary>
    public decimal? TempoMedioRespostaMinutos { get; set; }
    /// <summary>
    /// Tempo médio de resposta formatado para exibição (ex.: 2h02min).
    /// </summary>
    public string? TempoMedioRespostaLabel { get; set; }

    /// <summary>
    /// Data e hora do evento de lead mais recente
    /// </summary>
    public DateTime? DataUltimoEvento { get; set; }

    public DateTime DataCriacao { get; set; }
    public bool EhConvertido { get; set; }

    /// <summary>
    /// Id da conversa ativa (não encerrada) do lead, se existir
    /// </summary>
    public int? ConversaAtivaId { get; set; }

    /// <summary>
    /// Indica se houve troca de contato pessoal (telefone, celular, WhatsApp) entre cliente e vendedor na conversa
    /// </summary>
    public bool TrocaDeContato { get; set; }

    /// <summary>
    /// Indica se a última mensagem da conversa ativa é do vendedor/atendimento (ex.: template), aguardando resposta do cliente.
    /// </summary>
    public bool PendenteRespostaCliente { get; set; }
}
