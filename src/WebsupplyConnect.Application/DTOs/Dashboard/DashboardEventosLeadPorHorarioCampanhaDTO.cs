namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// DTO para análise de eventos de lead por horário e campanha.
/// Agrega quantidade de eventos independente do dia, agrupando apenas por hora (0-23) e campanha.
/// Permite identificar quais horários geram mais engajamento por campanha.
/// </summary>
public class DashboardEventosLeadPorHorarioCampanhaDTO
{
    /// <summary>
    /// Lista de campanhas com seus dados de eventos por horário
    /// </summary>
    public List<CampanhaHorarioDTO> Campanhas { get; set; } = new();

    /// <summary>
    /// Resumo geral (todas campanhas) por horário
    /// </summary>
    public List<HorarioResumoDTO> ResumoGeral { get; set; } = new();

    /// <summary>
    /// Hora com maior número de eventos (pico de engajamento geral)
    /// </summary>
    public int? HoraPicoGeral { get; set; }

    /// <summary>
    /// Total geral de eventos no período
    /// </summary>
    public int TotalEventos { get; set; }
}

/// <summary>
/// Dados de uma campanha com distribuição de eventos por hora
/// </summary>
public class CampanhaHorarioDTO
{
    public string NomeCampanha { get; set; } = string.Empty;

    /// <summary>
    /// Total de eventos da campanha no período
    /// </summary>
    public int TotalEventos { get; set; }

    /// <summary>
    /// Hora com maior número de eventos para esta campanha
    /// </summary>
    public int? HoraPico { get; set; }

    /// <summary>
    /// Distribuição de eventos por hora (0-23)
    /// </summary>
    public List<HorarioResumoDTO> EventosPorHora { get; set; } = new();
}

/// <summary>
/// Quantidade de eventos em um determinado horário
/// </summary>
public class HorarioResumoDTO
{
    /// <summary>
    /// Hora do dia (0-23)
    /// </summary>
    public int Hora { get; set; }

    /// <summary>
    /// Total de eventos neste horário
    /// </summary>
    public int TotalEventos { get; set; }

    /// <summary>
    /// Percentual em relação ao total
    /// </summary>
    public decimal Percentual { get; set; }
}
