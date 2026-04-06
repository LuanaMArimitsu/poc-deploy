using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Fatos;

/// <summary>
/// Fato agregado com granularidade por LeadEvento.
/// Chave única: LeadEventoId + DataReferencia (granularidade hora)
/// </summary>
public class FatoEventoAgregado : EntidadeBase
{
    // Chave de relacionamento
    public int LeadEventoId { get; private set; }
    public int LeadId { get; private set; }

    // FKs para dimensões
    public int TempoId { get; private set; }
    public int EmpresaId { get; private set; }
    public int? EquipeId { get; private set; }
    public int? VendedorId { get; private set; }
    public int? StatusAtualId { get; private set; }
    public int OrigemId { get; private set; }
    public int? CampanhaId { get; private set; }

    // Métricas (regra via Oportunidade.Convertida)
    public int TotalOportunidadesGeradas { get; private set; }
    public int OportunidadesGanhas { get; private set; }
    public int OportunidadesPerdidas { get; private set; }
    public decimal ValorTotalOportunidadesGanhas { get; private set; }

    // Métricas de Conversão do Lead
    public bool EhConvertido { get; private set; }
    public DateTime? DataConversao { get; private set; }

    // Métricas de Ciclo de Vendas
    public int? DuracaoCicloCompletoDias { get; private set; }
    public int? TempoAtePrimeiraOportunidadeDias { get; private set; }

    // Métricas de Atendimento
    public decimal? TempoMedioRespostaMinutos { get; private set; }
    public decimal? TempoMedioPrimeiroAtendimentoMinutos { get; private set; }
    public int TotalConversas { get; private set; }
    public int TotalMensagens { get; private set; }
    public int ConversasNaoLidas { get; private set; }

    public string? ProdutoInteresse { get; private set; }

    /// <summary>
    /// Data do último evento do lead ao qual este evento pertence. Usado para filtros de período
    /// nos indicadores de campanha (considerar leads ativos no período).
    /// </summary>
    public DateTime? DataUltimoEvento { get; private set; }

    // Granularidade
    public DateTime DataReferencia { get; private set; }

    protected FatoEventoAgregado() { } // EF Core

    public FatoEventoAgregado(
        int leadEventoId,
        int leadId,
        int tempoId,
        int empresaId,
        int? equipeId,
        int? vendedorId,
        int? statusAtualId,
        int origemId,
        int? campanhaId,
        DateTime dataReferencia) : base()
    {
        LeadEventoId = leadEventoId;
        LeadId = leadId;
        TempoId = tempoId;
        EmpresaId = empresaId;
        EquipeId = equipeId;
        VendedorId = vendedorId;
        StatusAtualId = statusAtualId;
        OrigemId = origemId;
        CampanhaId = campanhaId;
        DataReferencia = TruncarParaHora(dataReferencia);
    }

    public void AtualizarMetricas(
        int totalOportunidadesGeradas,
        int oportunidadesGanhas,
        int oportunidadesPerdidas,
        decimal valorTotalOportunidadesGanhas)
    {
        TotalOportunidadesGeradas = totalOportunidadesGeradas;
        OportunidadesGanhas = oportunidadesGanhas;
        OportunidadesPerdidas = oportunidadesPerdidas;
        ValorTotalOportunidadesGanhas = valorTotalOportunidadesGanhas;
        AtualizarDataModificacao();
    }

    public void AtualizarDimensoes(int? equipeId, int? vendedorId, int? campanhaId)
    {
        EquipeId = equipeId;
        VendedorId = vendedorId;
        CampanhaId = campanhaId;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasConversao(bool ehConvertido, DateTime? dataConversao)
    {
        EhConvertido = ehConvertido;
        DataConversao = dataConversao;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasCicloVendas(int? duracaoCicloCompletoDias, int? tempoAtePrimeiraOportunidadeDias)
    {
        DuracaoCicloCompletoDias = duracaoCicloCompletoDias;
        TempoAtePrimeiraOportunidadeDias = tempoAtePrimeiraOportunidadeDias;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasAtendimento(
        decimal? tempoMedioRespostaMinutos,
        decimal? tempoMedioPrimeiroAtendimentoMinutos,
        int totalConversas,
        int totalMensagens,
        int conversasNaoLidas)
    {
        TempoMedioRespostaMinutos = tempoMedioRespostaMinutos;
        TempoMedioPrimeiroAtendimentoMinutos = tempoMedioPrimeiroAtendimentoMinutos;
        TotalConversas = totalConversas;
        TotalMensagens = totalMensagens;
        ConversasNaoLidas = conversasNaoLidas;
        AtualizarDataModificacao();
    }

    public void AtualizarProdutoInteresse(string? produtoInteresse)
    {
        ProdutoInteresse = produtoInteresse;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a data do último evento do lead (para filtros de período nos indicadores de campanha).
    /// </summary>
    public void AtualizarDataUltimoEvento(DateTime? dataUltimoEvento)
    {
        DataUltimoEvento = dataUltimoEvento;
        AtualizarDataModificacao();
    }

    private static DateTime TruncarParaHora(DateTime data) =>
        new(data.Year, data.Month, data.Day, data.Hour, 0, 0);
}
