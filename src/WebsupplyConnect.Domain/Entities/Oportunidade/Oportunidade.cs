using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Oportunidade;

public class Oportunidade : EntidadeBase
{

    /// <summary>
    /// ID do lead associado a esta oportunidade
    /// </summary>
    public int LeadId { get; private set; }

    /// <summary>
    /// Lead associado a esta oportunidade
    /// </summary>
    public virtual Lead.Lead Lead { get; private set; }

    /// <summary>
    /// ID do produto de interesse desta oportunidade
    /// </summary>
    public int ProdutoId { get; private set; }

    /// <summary>
    /// Produto de interesse desta oportunidade
    /// </summary>
    public virtual Produto.Produto Produto { get; private set; }

    /// <summary>
    /// ID da etapa atual no funil de vendas
    /// </summary>
    public int EtapaId { get; private set; }

    /// <summary>
    /// Etapa atual no funil de vendas
    /// </summary>
    public virtual Etapa Etapa { get; private set; }

    /// <summary>
    /// Valor estimado da oportunidade em reais
    /// </summary>
    public decimal? Valor { get; private set; }

    /// <summary>
    /// Probabilidade de fechamento da oportunidade (0-100%)
    /// </summary>
    public int? Probabilidade { get; private set; }

    /// <summary>
    /// Data prevista para fechamento da oportunidade
    /// </summary>
    public DateTime? DataPrevisaoFechamento { get; private set; }

    /// <summary>
    /// ID do usuário responsável por esta oportunidade
    /// </summary>
    public int ResponsavelId { get; private set; }

    /// <summary>
    /// Usuário responsável por esta oportunidade
    /// </summary>
    public virtual Usuario.Usuario Responsavel { get; private set; }

    /// <summary>
    /// ID da origem desta oportunidade (mesmo do lead)
    /// </summary>
    public int OrigemId { get; private set; }

    /// <summary>
    /// Origem desta oportunidade
    /// </summary>
    public virtual Origem Origem { get; private set; }

    /// <summary>
    /// ID da empresa proprietária desta oportunidade
    /// </summary>
    public int EmpresaId { get; private set; }

    /// <summary>
    /// Empresa proprietária desta oportunidade
    /// </summary>
    public virtual Empresa.Empresa Empresa { get; private set; }

    /// <summary>
    /// Observações gerais sobre a oportunidade
    /// </summary>
    public string? Observacoes { get; private set; }

    /// <summary>
    /// Data de fechamento da oportunidade (quando finalizada)
    /// </summary>
    public DateTime? DataFechamento { get; private set; }

    /// <summary>
    /// Valor final da oportunidade (quando fechada)
    /// </summary>
    public decimal? ValorFinal { get; private set; }

    /// <summary>
    /// Data da última interação com o cliente
    /// </summary>
    public DateTime? DataUltimaInteracao { get; private set; }

    /// <summary>
    /// se a oportunidade foi convertida em venda no Gold
    /// </summary>
    public bool? Convertida { get; private set; } = null;

    public string? CodEvento { get; private set; }

    public int? TipoInteresseId { get; private set; }
    public virtual TipoInteresse TipoInteresse { get; private set; }
    public DateTime? DataConversao { get; private set; }


    public int? LeadEventoId { get; private set; }
    public virtual LeadEvento LeadEvento { get; private set; }

    /// <summary>
    /// Lista de históricos de mudança de etapa
    /// </summary>
    public virtual ICollection<EtapaHistorico> HistoricoEtapas { get; private set; }

    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected Oportunidade()
    {
        HistoricoEtapas = new List<EtapaHistorico>();
    }

    /// <summary>
    /// Construtor para criação de nova oportunidade
    /// </summary>
    /// <param name="leadId">ID do lead associado</param>
    /// <param name="produtoId">ID do produto de interesse</param>
    /// <param name="etapaId">ID da etapa inicial</param>
    /// <param name="valor">Valor estimado inicial</param>
    /// <param name="responsavelId">ID do usuário responsável</param>
    /// <param name="origemId">ID da origem</param>
    /// <param name="empresaId">ID da empresa</param>
    /// <param name="probabilidade">Probabilidade inicial (opcional)</param>
    /// <param name="leadEventoId">ID do evento do lead (opcional)</param>
    public Oportunidade(
        int leadId,
        int produtoId,
        int etapaId,
        int responsavelId,
        int origemId,
        int empresaId,
        decimal? valor,
        int? probabilidade,
        DateTime? dataPrevisaoFechamento = null,
        string? observacoes = null,
        int? tipoInteresse = null,
        int? leadEventoId = null) : this()
    {
        ValidarParametros(leadId, produtoId, etapaId, responsavelId, origemId, empresaId);

        LeadId = leadId;
        ProdutoId = produtoId;
        EtapaId = etapaId;
        Valor = valor ?? 0;
        Probabilidade = probabilidade ?? 0;
        ResponsavelId = responsavelId;
        OrigemId = origemId;
        EmpresaId = empresaId;
        DataPrevisaoFechamento = dataPrevisaoFechamento;
        Observacoes = observacoes;
        DataCriacao = TimeHelper.GetBrasiliaTime();
        DataModificacao = TimeHelper.GetBrasiliaTime();
        TipoInteresseId = tipoInteresse;
        LeadEventoId = leadEventoId;
    }

    public void AtualizaOportunidade(
        int? produtoId,
        decimal? valor,
        decimal? valorFinal,
        int? probabilidade,
        DateTime? dataPrevisaoFechamento = null,
        DateTime? dataUltimaInteracao = null,
        DateTime? dataFechamento = null,
        string? observacoes = null,
        int? tipoInteresse = null,
        int? leadEventoId = null,
        int? origemId = null)
    {
        ProdutoId = produtoId.HasValue && produtoId > 0 && produtoId != ProdutoId ? produtoId.Value : ProdutoId;
        Valor = valor.HasValue && valor > 0 && valor != Valor ? valor.Value : Valor;
        Probabilidade = probabilidade.HasValue && probabilidade > 0 && probabilidade != Probabilidade ? probabilidade.Value : Probabilidade;
        DataPrevisaoFechamento = dataPrevisaoFechamento.HasValue && dataPrevisaoFechamento != null && dataPrevisaoFechamento != DataPrevisaoFechamento ? dataPrevisaoFechamento.Value : DataPrevisaoFechamento;
        Observacoes = observacoes != null && observacoes != Observacoes ? observacoes : Observacoes;
        ValorFinal = valorFinal != null && valorFinal > 0 && valorFinal != ValorFinal ? valorFinal : ValorFinal;
        DataFechamento = dataFechamento.HasValue && dataFechamento != null && dataFechamento != DataFechamento ? dataFechamento.Value : DataFechamento;
        DataUltimaInteracao = dataUltimaInteracao.HasValue && dataUltimaInteracao != null && dataUltimaInteracao != DataUltimaInteracao ? dataUltimaInteracao.Value : DataUltimaInteracao;
        TipoInteresseId = tipoInteresse.HasValue && tipoInteresse > 0 && tipoInteresse != TipoInteresseId ? tipoInteresse.Value : TipoInteresseId;
        LeadEventoId = leadEventoId.HasValue && leadEventoId != LeadEventoId ? leadEventoId : null;
        OrigemId = origemId.HasValue && origemId > 0 && origemId != OrigemId ? origemId.Value : OrigemId;

        AtualizarDataModificacao();
    }



    public void Reabrir(int etapaDestivoId, int usuarioId, string observacaoObrigatoria)
    {
        if (string.IsNullOrWhiteSpace(observacaoObrigatoria))
            throw new DomainException("Observação é obrigatória para reabertura.");

        var etapaAnterior = EtapaId;

        DataFechamento = null;
        ValorFinal = null;
        EtapaId = etapaDestivoId;

        observacaoObrigatoria = $"Motivo da Reabertura: {observacaoObrigatoria}.";

        HistoricoEtapas.Add(new EtapaHistorico(
            oportunidadeId: Id,
            etapaAnteriorId: etapaAnterior,
            etapaNovaId: etapaDestivoId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: usuarioId,
            observacao: observacaoObrigatoria
        ));

        AtualizarDataModificacao();
    }

    public void AvancarPara(int etapaDestivoId, int usuarioId, string? observacao = null)
    {
        var etapaAnterior = EtapaId;

        EtapaId = etapaDestivoId;

        HistoricoEtapas.Add(new EtapaHistorico(
            oportunidadeId: Id,
            etapaAnteriorId: etapaAnterior,
            etapaNovaId: etapaDestivoId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: usuarioId,
            observacao: observacao
        ));

        AtualizarDataModificacao();
    }

    public void RegredirPara(int etapaDestivoId, int usuarioId, string observacaoObrigatoria)
    {
        if (string.IsNullOrWhiteSpace(observacaoObrigatoria))
            throw new DomainException("Observação é obrigatória para regressão.");

        var etapaAnterior = EtapaId;

        EtapaId = etapaDestivoId;

        observacaoObrigatoria = $"Motivo da Regressão: {observacaoObrigatoria}.";

        HistoricoEtapas.Add(new EtapaHistorico(
            oportunidadeId: Id,
            etapaAnteriorId: etapaAnterior,
            etapaNovaId: etapaDestivoId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: usuarioId,
            observacao: observacaoObrigatoria
        ));

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Marca a oportunidade como perdida (move para etapa "Perdida")
    /// </summary>
    /// <param name="etapaPerdidaId">ID da etapa "Perdida"</param>
    /// <param name="usuarioId">ID do usuário que está fazendo a mudança</param>
    /// <param name="observacao">Observação sobre a perda</param>
    /// <returns>Objeto de histórico da mudança de etapa</returns>
    public EtapaHistorico MarcarComoPerdida(int etapaPerdidaId, int usuarioId, string observacaoObrigatoria)
    {
        if (string.IsNullOrWhiteSpace(observacaoObrigatoria))
            throw new DomainException("Observação é obrigatória para regressão.");

        var etapaAnterior = EtapaId;
        EtapaId = etapaPerdidaId;
        DataFechamento = TimeHelper.GetBrasiliaTime();
        Probabilidade = 0;

        observacaoObrigatoria = $"Motivo da Perda: {observacaoObrigatoria}.";

        AtualizarDataModificacao();

        // Criar histórico de mudança de etapa
        var historico = new EtapaHistorico(
            oportunidadeId: Id,
            etapaAnteriorId: etapaAnterior,
            etapaNovaId: etapaPerdidaId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: usuarioId,
            observacao: observacaoObrigatoria
        );

        HistoricoEtapas.Add(historico);

        return historico;
    }

    public EtapaHistorico MarcarComoArquivada(int etapaArquivadaId, int usuarioId, string observacaoObrigatoria)
    {
        if (string.IsNullOrWhiteSpace(observacaoObrigatoria))
            throw new DomainException("Observação é obrigatória para arquivar oportunidade.");


        var etapaAnterior = EtapaId;
        EtapaId = etapaArquivadaId;
        DataFechamento = TimeHelper.GetBrasiliaTime();
        Probabilidade = 0;

        observacaoObrigatoria = $"Motivo de arquivamento: {observacaoObrigatoria}.";

        AtualizarDataModificacao();

        // Criar histórico de mudança de etapa
        var historico = new EtapaHistorico(
            oportunidadeId: Id,
            etapaAnteriorId: etapaAnterior,
            etapaNovaId: etapaArquivadaId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: usuarioId,
            observacao: observacaoObrigatoria
        );

        HistoricoEtapas.Add(historico);

        return historico;
    }


    /// <summary>
    /// Marca a oportunidade como ganha (move para etapa "Ganha")
    /// </summary>
    /// <param name="etapaGanhaId">ID da etapa "Ganha"</param>
    /// <param name="valorFinal">Valor final da venda</param>
    /// <param name="usuarioId">ID do usuário que está fazendo a mudança</param>
    /// <param name="observacao">Observação sobre o fechamento</param>
    /// <returns>Objeto de histórico da mudança de etapa</returns>
    public EtapaHistorico MarcarComoGanha(int etapaGanhaId, decimal valorFinal, int usuarioId, string? observacao = null)
    {
        if (valorFinal <= 0)
            throw new DomainException("O valor final deve ser maior que zero");

        var etapaAnterior = EtapaId;
        EtapaId = etapaGanhaId;
        ValorFinal = valorFinal;
        Valor = valorFinal; 
        DataFechamento = TimeHelper.GetBrasiliaTime();
        Probabilidade = 100;

        AtualizarDataModificacao();

        // Criar histórico de mudança de etapa
        var historico = new EtapaHistorico(
            oportunidadeId: Id,
            etapaAnteriorId: etapaAnterior,
            etapaNovaId: etapaGanhaId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: usuarioId,
            observacao: observacao
        );

        HistoricoEtapas.Add(historico);

        return historico;
    }

    public EtapaHistorico AdicionarEtapaHistorico(
        int oportunidadeId,
        int? etapaAnteriorId,
        int etapaNovaId,
        int responsavelId,
        string? observacao = null)
    {
        var historico = new EtapaHistorico(
            oportunidadeId: oportunidadeId,
            etapaAnteriorId: etapaAnteriorId,
            etapaNovaId: etapaNovaId,
            dataMudanca: TimeHelper.GetBrasiliaTime(),
            responsavelId: responsavelId,
            observacao: observacao
        );

        HistoricoEtapas.Add(historico);
        AtualizarDataModificacao();
        return historico;
    }

    /// <summary>
    /// Atualiza o valor da oportunidade
    /// </summary>
    /// <param name="novoValor">Novo valor da oportunidade</param>
    public void AtualizarValor(decimal novoValor)
    {
        if (novoValor < 0)
            throw new DomainException("O valor da oportunidade não pode ser negativo");

        if (EstaFinalizada())
            throw new DomainException("Não é possível alterar o valor de uma oportunidade finalizada");

        Valor = novoValor;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza o valor final da oportunidade
    /// </summary>
    /// <param name="valorFinal">Novo valor da oportunidade</param>
    public void AtualizarValorFinal(decimal valorFinal)
    {
        if (valorFinal < 0)
            throw new DomainException("O valor da oportunidade não pode ser negativo");

        ValorFinal = valorFinal;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a data de previsão de fechamento
    /// </summary>
    /// <param name="novaDataPrevisao">Nova data de previsão</param>
    public void AtualizarDataPrevisaoFechamento(DateTime? novaDataPrevisao)
    {
        if (novaDataPrevisao.HasValue && novaDataPrevisao.Value.Date < DateTime.UtcNow.Date)
            throw new DomainException("A data de previsão não pode ser no passado");

        DataPrevisaoFechamento = novaDataPrevisao;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Transfere a responsabilidade da oportunidade para outro usuário
    /// </summary>
    /// <param name="novoResponsavelId">ID do novo responsável</param>
    public void TransferirResponsabilidade(int novoResponsavelId, int empresaId)
    {
        if (novoResponsavelId <= 0)
            throw new DomainException("O ID do novo responsável deve ser maior que zero");
        ResponsavelId = novoResponsavelId;
        EmpresaId = empresaId;

        AtualizarDataModificacao();

        // O registro da transferência seria feito pelo Application Service
    }

    /// <summary>
    /// Atualiza a probabilidade de fechamento
    /// </summary>
    /// <param name="novaProbabilidade">Nova probabilidade (0-100)</param>
    public void AtualizarProbabilidade(int novaProbabilidade)
    {
        if (novaProbabilidade < 0 || novaProbabilidade > 100)
            throw new DomainException("A probabilidade deve estar entre 0 e 100");

        if (EstaFinalizada())
            throw new DomainException("Não é possível alterar a probabilidade de uma oportunidade finalizada");

        Probabilidade = novaProbabilidade;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza as observações da oportunidade
    /// </summary>
    /// <param name="novasObservacoes">Novas observações</param>
    public void AtualizarObservacoes(string? novasObservacoes)
    {
        Observacoes = novasObservacoes;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a data da última interação
    /// </summary>
    /// <param name="dataInteracao">Data da interação</param>
    public void AtualizarUltimaInteracao(DateTime dataInteracao)
    {
        if (dataInteracao > DateTime.UtcNow)
            throw new DomainException("A data da interação não pode ser no futuro");

        DataUltimaInteracao = dataInteracao;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Verifica se a oportunidade está finalizada (ganha ou perdida)
    /// </summary>
    /// <returns>True se estiver finalizada</returns>
    public bool EstaFinalizada()
    {
        // Esta lógica será implementada com base na propriedade da Etapa atual
        // Para o MVP, vamos simplificar verificando pelo DataFechamento
        return DataFechamento.HasValue;
    }

    /// <summary>
    /// Verifica se a oportunidade foi ganha
    /// </summary>
    /// <returns>True se foi ganha</returns>
    public bool FoiGanha()
    {
        // Esta lógica será implementada com base na propriedade da Etapa atual
        // Para o MVP, vamos simplificar verificando se tem valor final
        return DataFechamento.HasValue && ValorFinal.HasValue;
    }

    /// <summary>
    /// Verifica se a oportunidade foi perdida
    /// </summary>
    /// <returns>True se foi perdida</returns>
    public bool FoiPerdida()
    {
        // Esta lógica será implementada com base na propriedade da Etapa atual
        // Para o MVP, vamos simplificar verificando se foi finalizada sem valor final
        return DataFechamento.HasValue && !ValorFinal.HasValue;
    }

    /// <summary>
    /// Valida os parâmetros do construtor
    /// </summary>
    private static void ValidarParametros(int leadId, int produtoId, int etapaId,
        int responsavelId, int origemId, int empresaId)
    {
        if (leadId <= 0)
            throw new DomainException("ID do lead é obrigatório");

        if (produtoId <= 0)
            throw new DomainException("ID do produto é obrigatório");

        if (etapaId <= 0)
            throw new DomainException("ID da etapa é obrigatório");

        if (responsavelId <= 0)
            throw new DomainException("ID do responsável é obrigatório");

        if (origemId <= 0)
            throw new DomainException("ID da origem é obrigatório");

        if (empresaId <= 0)
            throw new DomainException("ID da empresa é obrigatório");
    }

    /// <summary>
    /// Tipos de movimentação possíveis para a oportunidade
    /// </summary>
    public enum TipoMovimentacao
    {
        Progresso,
        Regressao,
        Reabertura,
        Reativacao,
        Arquivamento,
        Finalização
    }

    /// <summary>
    /// Atribui o código do evento à oportunidade
    /// </summary>
    /// <param name="codEvento">Código do evento (máx. 30 caracteres)</param>
    public void AtribuirCodEvento(string? codEvento)
    {
        if (codEvento != null && codEvento.Length > 30)
            throw new DomainException("O código do evento deve ter no máximo 30 caracteres.");

        CodEvento = codEvento;
        AtualizarDataModificacao();
    }

    public void AtualizarConversao(bool convertida, DateTime date)
    {
        Convertida = convertida;
        DataConversao = date;

        AtualizarDataModificacao();
    }
}
