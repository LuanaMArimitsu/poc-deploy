namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// Distribuição de leads criados por hora do dia (0–23).
/// Baseado exclusivamente na data de criação do lead (DataReferencia),
/// independente de campanhas, eventos ou qualquer outro vínculo.
/// </summary>
public class DashboardLeadsCriadosPorHorarioDTO
{
    public int Hora { get; set; }
    public int Quantidade { get; set; }
    public decimal Percentual { get; set; }
}
