namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardConversaAtivaCanonicaDTO
{
    public int ConversaAtivaId { get; set; }
    public int LeadId { get; set; }
    public string NomeLead { get; set; } = string.Empty;
    public string? ProdutoInteresse { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public int MensagensNaoLidas { get; set; }
    public int TempoMedioAtendimentoMinutos { get; set; }
    public DateTime? DataUltimaMensagem { get; set; }
}
