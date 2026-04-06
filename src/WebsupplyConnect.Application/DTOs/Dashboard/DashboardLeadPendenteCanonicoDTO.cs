namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardLeadPendenteCanonicoDTO
{
    public int LeadId { get; set; }
    public string NomeLead { get; set; } = string.Empty;
    public DateTime? DataUltimoEvento { get; set; }
    public string TipoPendencia { get; set; } = string.Empty;
    public string NomeOrigem { get; set; } = string.Empty;
    public string? NomeCampanha { get; set; }
    public int MensagensNaoLidas { get; set; }
    public int? ConversaAtivaId { get; set; }
}
