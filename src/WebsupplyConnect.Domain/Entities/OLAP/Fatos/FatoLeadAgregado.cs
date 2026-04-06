using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Fatos;

/// <summary>
/// Fato agregado com granularidade por Lead.
/// Chave única: LeadId + DataReferencia (granularidade hora)
/// </summary>
public class FatoLeadAgregado : EntidadeBase
{
    // Chave de relacionamento
    public int LeadId { get; private set; }

    // FKs para dimensões
    public int TempoId { get; private set; }
    public int EmpresaId { get; private set; }
    public int? EquipeId { get; private set; }
    public int? VendedorId { get; private set; }
    public int? StatusAtualId { get; private set; }
    public int OrigemId { get; private set; }
    public int? CampanhaId { get; private set; }

    // Métricas agregadas
    public int TotalEventos { get; private set; }
    public int TotalOportunidades { get; private set; }
    public int OportunidadesGanhas { get; private set; }
    public int OportunidadesPerdidas { get; private set; }
    public decimal ValorTotalOportunidadesGanhas { get; private set; }

    // Métricas de Conversão do Lead
    public bool EhConvertido { get; private set; }
    public bool EhConvertidoPorOportunidade { get; private set; }
    public DateTime? DataConversao { get; private set; }

    // Métricas de Ciclo de Vendas
    public int? DuracaoCicloCompletoDias { get; private set; }
    public int? TempoAtePrimeiraOportunidadeDias { get; private set; }
    public int? TempoAtePrimeiraConversaoDias { get; private set; }

    // Métricas de Taxa de Conversão
    public decimal? TaxaConversaoLeadParaOportunidade { get; private set; }
    public decimal? TaxaQualificacaoLead { get; private set; }

    // Métricas de Atendimento
    public decimal? TempoMedioRespostaMinutos { get; private set; }
    public decimal? TempoMedioPrimeiroAtendimentoMinutos { get; private set; }
    public int TotalConversas { get; private set; }
    public int TotalMensagens { get; private set; }
    public int ConversasNaoLidas { get; private set; }
    public bool AguardandoRespostaVendedor { get; private set; }
    public bool AguardandoRespostaAtendimento { get; private set; }

    /// <summary>
    /// Nome do produto de interesse (da oportunidade mais recente vinculada ao lead)
    /// </summary>
    public string? ProdutoInteresse { get; private set; }

    /// <summary>
    /// Data e hora do evento de lead mais recente (DataEvento do LeadEvento)
    /// </summary>
    public DateTime? DataUltimoEvento { get; private set; }

    // Granularidade
    public DateTime DataReferencia { get; private set; }

    protected FatoLeadAgregado() { } // EF Core

    public FatoLeadAgregado(
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

    public void AtualizarMetricasAgregadas(
        int totalEventos,
        int totalOportunidades,
        int oportunidadesGanhas,
        int oportunidadesPerdidas,
        decimal valorTotalOportunidadesGanhas)
    {
        TotalEventos = totalEventos;
        TotalOportunidades = totalOportunidades;
        OportunidadesGanhas = oportunidadesGanhas;
        OportunidadesPerdidas = oportunidadesPerdidas;
        ValorTotalOportunidadesGanhas = valorTotalOportunidadesGanhas;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasConversao(
        bool ehConvertido,
        bool ehConvertidoPorOportunidade,
        DateTime? dataConversao)
    {
        EhConvertido = ehConvertido;
        EhConvertidoPorOportunidade = ehConvertidoPorOportunidade;
        DataConversao = dataConversao;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasCicloVendas(
        int? duracaoCicloCompletoDias,
        int? tempoAtePrimeiraOportunidadeDias,
        int? tempoAtePrimeiraConversaoDias)
    {
        DuracaoCicloCompletoDias = duracaoCicloCompletoDias;
        TempoAtePrimeiraOportunidadeDias = tempoAtePrimeiraOportunidadeDias;
        TempoAtePrimeiraConversaoDias = tempoAtePrimeiraConversaoDias;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasTaxaConversao(
        decimal? taxaConversaoLeadParaOportunidade,
        decimal? taxaQualificacaoLead)
    {
        TaxaConversaoLeadParaOportunidade = taxaConversaoLeadParaOportunidade;
        TaxaQualificacaoLead = taxaQualificacaoLead;
        AtualizarDataModificacao();
    }

    public void AtualizarMetricasAtendimento(
        decimal? tempoMedioRespostaMinutos,
        decimal? tempoMedioPrimeiroAtendimentoMinutos,
        int totalConversas,
        int totalMensagens,
        int conversasNaoLidas,
        bool aguardandoRespostaVendedor,
        bool aguardandoRespostaAtendimento)
    {
        TempoMedioRespostaMinutos = tempoMedioRespostaMinutos;
        TempoMedioPrimeiroAtendimentoMinutos = tempoMedioPrimeiroAtendimentoMinutos;
        TotalConversas = totalConversas;
        TotalMensagens = totalMensagens;
        ConversasNaoLidas = conversasNaoLidas;
        AguardandoRespostaVendedor = aguardandoRespostaVendedor;
        AguardandoRespostaAtendimento = aguardandoRespostaAtendimento;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza o produto de interesse (da oportunidade mais recente)
    /// </summary>
    public void AtualizarProdutoInteresse(string? produtoInteresse)
    {
        ProdutoInteresse = produtoInteresse;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a data/hora do evento de lead mais recente
    /// </summary>
    public void AtualizarDataUltimoEvento(DateTime? dataUltimoEvento)
    {
        DataUltimoEvento = dataUltimoEvento;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza todas as dimensões do fato (para manter o registro único por lead sempre atualizado)
    /// </summary>
    public void AtualizarDimensoesCompletas(
        int tempoId, int empresaId, int? equipeId, int? vendedorId,
        int? statusAtualId, int origemId, int? campanhaId, DateTime dataReferencia)
    {
        TempoId = tempoId;
        EmpresaId = empresaId;
        EquipeId = equipeId;
        VendedorId = vendedorId;
        StatusAtualId = statusAtualId;
        OrigemId = origemId;
        CampanhaId = campanhaId;
        DataReferencia = TruncarParaHora(dataReferencia);
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza as dimensões de equipe, vendedor e campanha (podem ter mudado no lead)
    /// </summary>
    public void AtualizarDimensoes(int? equipeId, int? vendedorId, int? campanhaId)
    {
        EquipeId = equipeId;
        VendedorId = vendedorId;
        CampanhaId = campanhaId;
        AtualizarDataModificacao();
    }

    private static DateTime TruncarParaHora(DateTime data) =>
        new(data.Year, data.Month, data.Day, data.Hour, 0, 0);
}
