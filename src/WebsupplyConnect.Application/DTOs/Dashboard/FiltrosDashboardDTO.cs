using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Application.DTOs.Dashboard;

/// <summary>
/// DTO para filtros do dashboard - contempla todos os filtros das 4 abas
/// </summary>
public class FiltrosDashboardDTO
{
    private static readonly DateTime DataInicioHistoricoPadrao = new(2000, 1, 1);

    // ===== FILTROS DE PERÍODO (todos os períodos pré-definidos + customizado) =====

    /// <summary>
    /// Tipo de período: Hoje, EstaSemana, Ultimos7Dias, Ultimos30Dias, EsteMes, EsteTrimestre, EsteAno, Customizado
    /// </summary>
    public TipoPeriodoEnum? TipoPeriodo { get; set; }

    /// <summary>
    /// Data inicial do período (obrigatório quando TipoPeriodo = Customizado)
    /// </summary>
    public DateTime? DataInicio { get; set; }

    /// <summary>
    /// Data final do período (obrigatório quando TipoPeriodo = Customizado)
    /// </summary>
    public DateTime? DataFim { get; set; }

    /// <summary>
    /// Ano de referência para períodos mensais.
    /// Obrigatório para MesReferenciaCompleto.
    /// </summary>
    public int? AnoReferencia { get; set; }

    /// <summary>
    /// Mês de referência para períodos mensais (1 a 12).
    /// Obrigatório para MesReferenciaCompleto.
    /// </summary>
    public int? MesReferencia { get; set; }

    /// <summary>
    /// Ano inicial para o período "MesesAnteriores" (opcional).
    /// </summary>
    public int? AnoInicioHistorico { get; set; }

    /// <summary>
    /// Mês inicial para o período "MesesAnteriores" (opcional; 1 a 12).
    /// </summary>
    public int? MesInicioHistorico { get; set; }

    /// <summary>
    /// Quando true, não aplica filtro de período/datas (usado em leads-aguardando-atendimento e leads-aguardando-resposta).
    /// </summary>
    public bool IgnorarFiltroPeriodo { get; set; }

    // ===== FILTROS AVANÇADOS =====

    /// <summary>
    /// Filtro por empresa (compatibilidade legado: valor único).
    /// </summary>
    public int? EmpresaId { get; set; }

    /// <summary>
    /// Filtro por empresas (multi-seleção).
    /// </summary>
    public List<int>? EmpresaIds { get; set; }

    /// <summary>
    /// Filtro por equipe (compatibilidade legado: valor único).
    /// </summary>
    public int? EquipeId { get; set; }

    /// <summary>
    /// Filtro por equipes (multi-seleção).
    /// </summary>
    public List<int>? EquipeIds { get; set; }

    /// <summary>
    /// Filtro por vendedor/responsável (compatibilidade legado: valor único).
    /// </summary>
    public int? VendedorId { get; set; }

    /// <summary>
    /// Filtro por vendedores/responsáveis (multi-seleção).
    /// </summary>
    public List<int>? VendedorIds { get; set; }

    /// <summary>
    /// Filtro por origem (WhatsApp, Instagram, Website, etc.) - compatibilidade legado.
    /// </summary>
    public int? OrigemId { get; set; }

    /// <summary>
    /// Filtro por origens (multi-seleção).
    /// </summary>
    public List<int>? OrigemIds { get; set; }

    /// <summary>
    /// Filtro por nome(s) de campanha (atributo Nome na dimensão OLAP / entidade transacional).
    /// Comparado sem diferenciar maiúsculas/minúsculas; inclui todas as campanhas (todas as empresas) cujo nome coincide, salvo restrição por <see cref="EmpresaIds"/>.
    /// </summary>
    public string? CampanhaNome { get; set; }

    /// <summary>
    /// Filtro por vários nomes de campanha (multi-seleção).
    /// </summary>
    public List<string>? CampanhaNomes { get; set; }

    /// <summary>
    /// Filtro por status do lead
    /// </summary>
    public int? StatusLeadId { get; set; }

    /// <summary>
    /// Filtro por status do lead (multi-seleção).
    /// </summary>
    public List<int>? StatusLeadIds { get; set; }

    /// <summary>Filtro por funil (IDs transacionais de Funil).</summary>
    public int? FunilId { get; set; }
    public List<int>? FunilIds { get; set; }

    /// <summary>Filtro por etapa (IDs transacionais de Etapa).</summary>
    public int? EtapaId { get; set; }
    public List<int>? EtapaIds { get; set; }

    /// <summary>
    /// Campo para ordenação da listagem de leads (ex: dataUltimoEvento, nome, nomeOrigem).
    /// </summary>
    public string? OrdenarPor { get; set; }

    /// <summary>
    /// Direção da ordenação: "asc" ou "desc".
    /// </summary>
    public string? DirecaoOrdenacao { get; set; }

    // ===== MÉTODOS AUXILIARES =====

    /// <summary>
    /// Resolve as datas de início e fim baseado no tipo de período
    /// </summary>
    public (DateTime DataInicio, DateTime DataFim) ResolverPeriodo()
    {
        // Usa Brasília para consistência com dados salvos (EntidadeBase usa TimeHelper.GetBrasiliaTime())
        var agora = TimeHelper.GetBrasiliaTime();
        var inicioMesReferencia = ResolverInicioMesReferencia(agora);
        var inicioMesAnterior = inicioMesReferencia.AddMonths(-1);
        var fimMesAnterior = inicioMesReferencia.AddTicks(-1);

        return TipoPeriodo switch
        {
            TipoPeriodoEnum.Hoje => (agora.Date, agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.EstaSemana => (ObterInicioSemana(agora), agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.Ultimos7Dias => (agora.AddDays(-7).Date, agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.Ultimos30Dias => (agora.AddDays(-30).Date, agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.EsteMes => (new DateTime(agora.Year, agora.Month, 1), agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.EsteTrimestre => (ObterInicioTrimestre(agora), agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.EsteAno => (new DateTime(agora.Year, 1, 1), agora.Date.AddDays(1).AddTicks(-1)),
            TipoPeriodoEnum.MesAnterior => (inicioMesAnterior, fimMesAnterior),
            TipoPeriodoEnum.MesesAnteriores => (ResolverInicioHistorico(), fimMesAnterior),
            TipoPeriodoEnum.MesReferenciaCompleto => (inicioMesReferencia, ObterFimMes(inicioMesReferencia)),
            TipoPeriodoEnum.Customizado when DataInicio.HasValue && DataFim.HasValue =>
                (DataInicio.Value.Date, DataFim.Value.Date.AddDays(1).AddTicks(-1)),
            _ => (agora.AddDays(-365).Date, agora.Date.AddDays(1).AddTicks(-1)) // Default: últimos 365 dias (evita vazio)
        };
    }

    public List<int> ObterEmpresaIds() => NormalizarIds(EmpresaIds, EmpresaId);
    public List<int> ObterEquipeIds() => NormalizarIds(EquipeIds, EquipeId);
    public List<int> ObterVendedorIds() => NormalizarIds(VendedorIds, VendedorId);
    public List<int> ObterOrigemIds() => NormalizarIds(OrigemIds, OrigemId);
    public List<int> ObterStatusLeadIds() => NormalizarIds(StatusLeadIds, StatusLeadId);
    public List<int> ObterFunilIds() => NormalizarIds(FunilIds, FunilId);
    public List<int> ObterEtapaIds() => NormalizarIds(EtapaIds, EtapaId);

    /// <summary>
    /// Nomes de campanha não vazios, sem duplicata (comparação ordinal, ignorando maiúsculas/minúsculas).
    /// </summary>
    public List<string> ObterCampanhaNomes()
    {
        var brutos = new List<string>();
        foreach (var n in CampanhaNomes ?? [])
        {
            var t = n?.Trim();
            if (!string.IsNullOrEmpty(t))
                brutos.Add(t);
        }

        if (!string.IsNullOrWhiteSpace(CampanhaNome))
            brutos.Add(CampanhaNome.Trim());

        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resultado = new List<string>();
        foreach (var n in brutos)
        {
            if (vistos.Add(n))
                resultado.Add(n);
        }

        return resultado;
    }

    private static List<int> NormalizarIds(List<int>? ids, int? legado)
    {
        var normalizados = (ids ?? [])
            .Where(i => i > 0)
            .Distinct()
            .ToList();

        if (normalizados.Count == 0 && legado.HasValue && legado.Value > 0)
            normalizados.Add(legado.Value);

        return normalizados;
    }

    private static DateTime ObterInicioSemana(DateTime data)
    {
        // Segunda-feira como início da semana (padrão brasileiro/comercial)
        var diff = (7 + (data.DayOfWeek - DayOfWeek.Monday)) % 7;
        return data.AddDays(-diff).Date;
    }

    private static DateTime ObterInicioTrimestre(DateTime data)
    {
        var trimestre = (data.Month - 1) / 3;
        return new DateTime(data.Year, trimestre * 3 + 1, 1);
    }

    private static DateTime ObterFimMes(DateTime inicioMes)
        => inicioMes.AddMonths(1).AddTicks(-1);

    private DateTime ResolverInicioHistorico()
    {
        if (AnoInicioHistorico.HasValue && MesInicioHistorico.HasValue &&
            EhAnoMesValido(AnoInicioHistorico.Value, MesInicioHistorico.Value))
        {
            return new DateTime(AnoInicioHistorico.Value, MesInicioHistorico.Value, 1);
        }

        if (DataInicio.HasValue)
            return new DateTime(DataInicio.Value.Year, DataInicio.Value.Month, 1);

        return DataInicioHistoricoPadrao;
    }

    private DateTime ResolverInicioMesReferencia(DateTime agora)
    {
        if (AnoReferencia.HasValue && MesReferencia.HasValue &&
            EhAnoMesValido(AnoReferencia.Value, MesReferencia.Value))
        {
            return new DateTime(AnoReferencia.Value, MesReferencia.Value, 1);
        }

        return new DateTime(agora.Year, agora.Month, 1);
    }

    private static bool EhAnoMesValido(int ano, int mes)
        => ano is >= 1900 and <= 9999 && mes is >= 1 and <= 12;
}

public enum TipoPeriodoEnum
{
    Ultimos7Dias = 1,
    Ultimos30Dias = 2,
    EsteMes = 3,
    EsteTrimestre = 4,
    EsteAno = 5,
    Customizado = 6,
    Hoje = 7,
    EstaSemana = 8,
    MesAnterior = 9,
    MesesAnteriores = 10,
    MesReferenciaCompleto = 11
}
