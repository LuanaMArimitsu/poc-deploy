namespace WebsupplyConnect.Application.DTOs.Dashboard;

public class DashboardPerformanceEquipeDTO
{
    public int EquipeId { get; set; }
    /// <summary>ID da empresa (transacional) à qual a equipe pertence.</summary>
    public int EmpresaId { get; set; }
    public string NomeEquipe { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int LeadsConvertidos { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal ValorTotal { get; set; }
    public int TotalConversas { get; set; }
    public int TotalMensagens { get; set; }
    public int MensagensNaoLidas { get; set; }
}
