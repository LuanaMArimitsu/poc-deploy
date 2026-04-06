using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

namespace WebsupplyConnect.Domain.Entities.OLAP.Fatos;

/// <summary>
/// Fato principal com granularidade por Oportunidade.
/// Chave única: OportunidadeId + DataReferencia (granularidade hora)
/// </summary>
public class FatoOportunidadeMetrica : EntidadeBase
{
    // Chaves de relacionamento
    public int OportunidadeId { get; private set; }
    public int LeadId { get; private set; }
    public int? LeadEventoId { get; private set; }    // FK opcional para LeadEvento

    // FKs para dimensões
    public int TempoId { get; private set; }
    public int EmpresaId { get; private set; }
    public int? EquipeId { get; private set; }
    public int? VendedorId { get; private set; }
    public int? StatusLeadId { get; private set; }
    public int OrigemId { get; private set; }
    public int? CampanhaId { get; private set; }      // Via LeadEvento quando disponível
    public int? DimensaoEtapaFunilId { get; private set; }

    // Métricas da Oportunidade
    public decimal ValorEstimado { get; private set; }
    public decimal? ValorFinal { get; private set; }
    public int? Probabilidade { get; private set; }
    public bool EhGanha { get; private set; }         // Convertida == true
    public bool EhPerdida { get; private set; }       // Convertida == false
    public DateTime? DataFechamento { get; private set; }

    // Métricas de Ciclo de Vendas
    public int? DuracaoCicloVendasDias { get; private set; }
    public int? TempoEmEtapaAtualDias { get; private set; }
    public int? DiasDesdeUltimaInteracao { get; private set; }
    public bool EhEstagnada { get; private set; }
    public decimal? ValorEsperadoPipeline { get; private set; }

    // Métricas de Taxa de Conversão
    public decimal? TaxaConversaoEtapa { get; private set; }
    public decimal? WinRateEtapa { get; private set; }

    // Métricas agregadas do Lead
    public decimal? TempoMedioRespostaMinutos { get; private set; }
    public decimal? TempoMedioPrimeiroAtendimentoMinutos { get; private set; }
    public int TotalConversas { get; private set; }
    public int ConversasNaoLidas { get; private set; }

    /// <summary>
    /// Data do último evento do lead vinculado à oportunidade. Usado para filtros de período
    /// nos indicadores de campanha (considerar leads ativos no período).
    /// </summary>
    public DateTime? DataUltimoEvento { get; private set; }

    // Granularidade
    public DateTime DataReferencia { get; private set; }

    // Navegação para dimensões
    public virtual DimensaoTempo Tempo { get; private set; } = null!;
    public virtual DimensaoEmpresa Empresa { get; private set; } = null!;
    public virtual DimensaoEquipe? Equipe { get; private set; }
    public virtual DimensaoVendedor? Vendedor { get; private set; }
    public virtual DimensaoStatusLead? StatusLead { get; private set; }
    public virtual DimensaoOrigem Origem { get; private set; } = null!;
    public virtual DimensaoCampanha? Campanha { get; private set; }
    public virtual DimensaoEtapaFunil? EtapaFunil { get; private set; }

    protected FatoOportunidadeMetrica() { } // EF Core

    public FatoOportunidadeMetrica(
        int oportunidadeId,
        int leadId,
        int? leadEventoId,
        int tempoId,
        int empresaId,
        int? equipeId,
        int? vendedorId,
        int? statusLeadId,
        int origemId,
        int? campanhaId,
        int? dimensaoEtapaFunilId,
        decimal valorEstimado,
        decimal? valorFinal,
        int? probabilidade,
        bool ehGanha,
        bool ehPerdida,
        DateTime? dataFechamento,
        DateTime dataReferencia) : base()
    {
        OportunidadeId = oportunidadeId;
        LeadId = leadId;
        LeadEventoId = leadEventoId;
        TempoId = tempoId;
        EmpresaId = empresaId;
        EquipeId = equipeId;
        VendedorId = vendedorId;
        StatusLeadId = statusLeadId;
        OrigemId = origemId;
        CampanhaId = campanhaId;
        DimensaoEtapaFunilId = dimensaoEtapaFunilId;
        ValorEstimado = valorEstimado;
        ValorFinal = valorFinal;
        Probabilidade = probabilidade;
        EhGanha = ehGanha;
        EhPerdida = ehPerdida;
        DataFechamento = dataFechamento;
        DataReferencia = TruncarParaHora(dataReferencia);
    }

    public void AtualizarMetricas(
        decimal valorEstimado,
        decimal? valorFinal,
        int? probabilidade,
        bool ehGanha,
        bool ehPerdida,
        DateTime? dataFechamento)
    {
        ValorEstimado = valorEstimado;
        ValorFinal = valorFinal;
        Probabilidade = probabilidade;
        EhGanha = ehGanha;
        EhPerdida = ehPerdida;
        DataFechamento = dataFechamento;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasCicloVendas(
        int? duracaoCicloVendasDias,
        int? tempoEmEtapaAtualDias,
        int? diasDesdeUltimaInteracao,
        bool ehEstagnada,
        decimal? valorEsperadoPipeline)
    {
        DuracaoCicloVendasDias = duracaoCicloVendasDias;
        TempoEmEtapaAtualDias = tempoEmEtapaAtualDias;
        DiasDesdeUltimaInteracao = diasDesdeUltimaInteracao;
        EhEstagnada = ehEstagnada;
        ValorEsperadoPipeline = valorEsperadoPipeline;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasTaxaConversao(
        decimal? taxaConversaoEtapa,
        decimal? winRateEtapa)
    {
        TaxaConversaoEtapa = taxaConversaoEtapa;
        WinRateEtapa = winRateEtapa;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasLead(
        decimal? tempoMedioRespostaMinutos,
        decimal? tempoMedioPrimeiroAtendimentoMinutos,
        int totalConversas,
        int conversasNaoLidas)
    {
        TempoMedioRespostaMinutos = tempoMedioRespostaMinutos;
        TempoMedioPrimeiroAtendimentoMinutos = tempoMedioPrimeiroAtendimentoMinutos;
        TotalConversas = totalConversas;
        ConversasNaoLidas = conversasNaoLidas;
        AtualizarDataModificacao();
    }

    public void AtualizarDimensoes(int? equipeId, int? vendedorId, int? campanhaId, int? dimensaoEtapaFunilId)
    {
        EquipeId = equipeId;
        VendedorId = vendedorId;
        CampanhaId = campanhaId;
        DimensaoEtapaFunilId = dimensaoEtapaFunilId;
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
