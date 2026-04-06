using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.ETL;

namespace WebsupplyConnect.Application.Services.ETL;

/// <summary>
/// Serviço de cálculos para métricas OLAP.
/// Implementa cálculos reais baseados em Conversas, Mensagens, Oportunidades e Leads.
/// </summary>
public class ETLCalculosService : IETLCalculosService
{
    private readonly IConversaReaderService _conversaReaderService;
    private readonly IMensagemReaderService _mensagemReaderService;
    private readonly IOportunidadeReaderService _oportunidadeReaderService;
    private readonly ILeadReaderService _leadReaderService;

    public ETLCalculosService(
        IConversaReaderService conversaReaderService,
        IMensagemReaderService mensagemReaderService,
        IOportunidadeReaderService oportunidadeReaderService,
        ILeadReaderService leadReaderService)
    {
        _conversaReaderService = conversaReaderService;
        _mensagemReaderService = mensagemReaderService;
        _oportunidadeReaderService = oportunidadeReaderService;
        _leadReaderService = leadReaderService;
    }

    public async Task<decimal?> CalcularTempoMedioRespostaAsync(
        int leadId,
        HashSet<int> botUserIds,
        Dictionary<int, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>>? horariosVendedores = null,
        CancellationToken cancellationToken = default)
    {
        var conversas = await _conversaReaderService.GetAllConversasByLeadAsync(leadId);
        if (conversas.Count == 0) return null;

        var temposResposta = new List<decimal>();
        foreach (var conversa in conversas)
        {
            if (botUserIds.Contains(conversa.UsuarioId))
                continue;

            var mensagens = await _mensagemReaderService.ObterMensagensPorConversaIdParaETLAsync(conversa.Id);
            mensagens = mensagens.OrderBy(m => m.DataEnvio ?? m.DataCriacao).ToList();

            if (mensagens.Count < 2) continue;

            // Usa a última mensagem do cliente (R) antes de cada resposta humana (E não-bot), não a primeira do bloco.
            Mensagem? ultimaMensagemCliente = null;
            foreach (var msg in mensagens)
            {
                var ehMsgBot = msg.Sentido == 'E' && msg.UsuarioId.HasValue && botUserIds.Contains(msg.UsuarioId.Value);
                if (ehMsgBot) continue;

                if (msg.Sentido == 'R')
                {
                    ultimaMensagemCliente = msg;
                }
                else if (msg.Sentido == 'E' && ultimaMensagemCliente != null)
                {
                    var dataCliente = ultimaMensagemCliente.DataEnvio ?? ultimaMensagemCliente.DataCriacao;
                    var dataVendedor = msg.DataEnvio ?? msg.DataCriacao;

                    var scheduleVendedor = ResolverScheduleVendedor(msg.UsuarioId, horariosVendedores);
                    var minutosUteis = CalcularMinutosUteisEntreDataHoras(dataCliente, dataVendedor, scheduleVendedor);
                    if (minutosUteis >= 0)
                    {
                        temposResposta.Add(minutosUteis);
                    }

                    ultimaMensagemCliente = null;
                }
            }
        }
        return temposResposta.Count > 0 ? Math.Round(temposResposta.Average(), 2) : null;
    }

    /// <summary>
    /// Resolve o schedule de trabalho do vendedor. Retorna null para usar o fallback padrão.
    /// </summary>
    internal static Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>? ResolverScheduleVendedor(
        int? vendedorId,
        Dictionary<int, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>>? horariosVendedores)
    {
        if (vendedorId == null || horariosVendedores == null)
            return null;

        if (!horariosVendedores.TryGetValue(vendedorId.Value, out var schedule))
            return null;

        return schedule.Count > 0 ? schedule : null;
    }

    /// <summary>
    /// Calcula os minutos úteis entre duas datas, considerando o horário de trabalho.
    /// Se <paramref name="schedule"/> for fornecido, usa o horário configurado do vendedor.
    /// Caso contrário, aplica o fallback padrão: Segunda a Sábado, 08:00 às 18:00.
    /// </summary>
    public static decimal CalcularMinutosUteisEntreDataHoras(
        DateTime inicio,
        DateTime fim,
        Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>? schedule = null)
    {
        if (fim <= inicio) return 0;

        var totalMinutosUteis = 0m;
        var atual = inicio;
        const int maxIteracoes = 366;
        var iteracoes = 0;

        while (atual < fim && iteracoes++ < maxIteracoes)
        {
            var (ehDiaUtil, horaInicio, horaFim) = ObterExpedienteDia(atual.DayOfWeek, schedule);

            if (!ehDiaUtil)
            {
                atual = ProximoDiaUtil(atual.Date.AddDays(1), schedule);
                continue;
            }

            if (atual.TimeOfDay < horaInicio)
            {
                atual = atual.Date + horaInicio;
            }

            if (atual.TimeOfDay >= horaFim)
            {
                atual = ProximoDiaUtil(atual.Date.AddDays(1), schedule);
                continue;
            }

            var fimDiaUtil = atual.Date + horaFim;
            var fimEfetivo = fim < fimDiaUtil ? fim : fimDiaUtil;

            if (fimEfetivo > atual)
            {
                totalMinutosUteis += (decimal)(fimEfetivo - atual).TotalMinutes;
            }

            atual = ProximoDiaUtil(atual.Date.AddDays(1), schedule);
        }

        return totalMinutosUteis;
    }

    /// <summary>
    /// Retorna se o dia é útil e os horários de expediente.
    /// Quando não há schedule configurado, usa o fallback (Seg-Sáb, 08-18h).
    /// </summary>
    private static (bool EhDiaUtil, TimeSpan HoraInicio, TimeSpan HoraFim) ObterExpedienteDia(
        DayOfWeek dia,
        Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>? schedule)
    {
        if (schedule != null)
        {
            if (schedule.TryGetValue(dia, out var horario))
                return (true, horario.Inicio, horario.Fim);
            return (false, TimeSpan.Zero, TimeSpan.Zero);
        }

        if (dia == DayOfWeek.Sunday)
            return (false, TimeSpan.Zero, TimeSpan.Zero);

        return (true, new TimeSpan(8, 0, 0), new TimeSpan(18, 0, 0));
    }

    /// <summary>
    /// Avança para o próximo dia útil conforme o schedule (ou fallback padrão).
    /// </summary>
    private static DateTime ProximoDiaUtil(DateTime data, Dictionary<DayOfWeek, (TimeSpan Inicio, TimeSpan Fim)>? schedule)
    {
        const int maxIteracoes = 8;
        var iteracoes = 0;

        while (iteracoes++ < maxIteracoes)
        {
            var (ehDiaUtil, horaInicio, _) = ObterExpedienteDia(data.DayOfWeek, schedule);
            if (ehDiaUtil)
                return data + horaInicio;
            data = data.AddDays(1);
        }

        return data + new TimeSpan(8, 0, 0);
    }

    public async Task<decimal?> CalcularTempoMedioPrimeiroAtendimentoAsync(int leadId, HashSet<int> botUserIds, CancellationToken cancellationToken = default)
    {
        var conversas = await _conversaReaderService.GetAllConversasByLeadAsync(leadId);
        var primeiraConversa = conversas
            .Where(c => !botUserIds.Contains(c.UsuarioId))
            .OrderBy(c => c.DataInicio)
            .FirstOrDefault();
        if (primeiraConversa == null) return null;

        var mensagens = await _mensagemReaderService.ObterMensagensPorConversaIdParaETLAsync(primeiraConversa.Id, 'E');
        mensagens = mensagens
            .Where(m => !m.UsuarioId.HasValue || !botUserIds.Contains(m.UsuarioId.Value))
            .OrderBy(m => m.DataEnvio ?? m.DataCriacao)
            .ToList();

        var primeiraResposta = mensagens.FirstOrDefault();
        if (primeiraResposta == null) return null;

        var dataResposta = primeiraResposta.DataEnvio ?? primeiraResposta.DataCriacao;
        return (decimal)(dataResposta - primeiraConversa.DataInicio).TotalMinutes;
    }

    public async Task<int?> CalcularDuracaoCicloVendasAsync(int oportunidadeId)
    {
        var oportunidade = await _oportunidadeReaderService.ObterOportunidadePorIdParaETLAsync(oportunidadeId);
        if (oportunidade == null) return null;
        if (!oportunidade.DataFechamento.HasValue) return null;
        return (int)(oportunidade.DataFechamento.Value - oportunidade.DataCriacao).TotalDays;
    }

    public async Task<int?> CalcularTempoEmEtapaAtualAsync(int oportunidadeId)
    {
        var historico = await _oportunidadeReaderService.ObterHistoricoEtapasPorOportunidadeIdAsync(oportunidadeId);
        var ultimaMudanca = historico.FirstOrDefault();
        if (ultimaMudanca == null) return null;
        return (int)(DateTime.UtcNow - ultimaMudanca.DataMudanca).TotalDays;
    }

    public async Task<int?> CalcularDuracaoCicloCompletoAsync(int leadId)
    {
        var lead = await _leadReaderService.ObterLeadComResponsavelParaETLAsync(leadId, includeDeleted: true);
        if (lead?.DataConversaoCliente == null) return null;
        var primeiraOportunidade = await _oportunidadeReaderService.GetPrimeiraOportunidadeAsync(leadId);
        if (primeiraOportunidade == null) return null;
        return (int)(lead.DataConversaoCliente.Value - primeiraOportunidade.DataCriacao).TotalDays;
    }

    public async Task<int?> CalcularTempoAtePrimeiraOportunidadeAsync(int leadId)
    {
        var lead = await _leadReaderService.ObterLeadComResponsavelParaETLAsync(leadId, includeDeleted: true);
        if (lead == null) return null;
        var primeiraOportunidade = await _oportunidadeReaderService.GetPrimeiraOportunidadeAsync(leadId);
        if (primeiraOportunidade == null) return null;
        return (int)(primeiraOportunidade.DataCriacao - lead.DataCriacao).TotalDays;
    }

    public async Task<decimal?> CalcularValorEsperadoPipelineAsync(int oportunidadeId)
    {
        var oportunidade = await _oportunidadeReaderService.ObterOportunidadePorIdParaETLAsync(oportunidadeId);
        if (oportunidade?.Valor == null || oportunidade.Probabilidade == null) return null;
        return (oportunidade.Valor.Value * oportunidade.Probabilidade.Value) / 100m;
    }

}
