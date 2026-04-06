using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.OLAP.Controle;

/// <summary>
/// Controla o processamento delta do ETL.
/// Chave única: TipoProcessamento
/// </summary>
public class ETLControleProcessamento : EntidadeBase
{
    public string TipoProcessamento { get; private set; } = string.Empty;  // "Dimensoes", "FatoOportunidade", "FatoLead", "FatoEvento"
    public DateTime UltimaDataProcessada { get; private set; }
    public DateTime DataUltimaExecucao { get; private set; }
    public string StatusUltimaExecucao { get; private set; } = string.Empty;  // "Sucesso", "Erro", "EmProcessamento"
    public int RegistrosProcessados { get; private set; }
    public int TempoExecucaoSegundos { get; private set; }
    public string? MensagemErro { get; private set; }

    protected ETLControleProcessamento() { } // EF Core

    public ETLControleProcessamento(string tipoProcessamento) : base()
    {
        TipoProcessamento = tipoProcessamento ?? throw new ArgumentNullException(nameof(tipoProcessamento));
        StatusUltimaExecucao = "Pendente";
        UltimaDataProcessada = TimeHelper.GetBrasiliaTime();
        DataUltimaExecucao = TimeHelper.GetBrasiliaTime();
    }

    public void IniciarProcessamento()
    {
        StatusUltimaExecucao = "EmProcessamento";
        DataUltimaExecucao = TimeHelper.GetBrasiliaTime();
        MensagemErro = null;
        AtualizarDataModificacao();
    }

    public void FinalizarComSucesso(DateTime ultimaDataProcessada, int registrosProcessados, int tempoExecucaoSegundos)
    {
        UltimaDataProcessada = ultimaDataProcessada;
        StatusUltimaExecucao = "Sucesso";
        RegistrosProcessados = registrosProcessados;
        TempoExecucaoSegundos = tempoExecucaoSegundos;
        MensagemErro = null;
        AtualizarDataModificacao();
    }

    public void FinalizarComErro(string mensagemErro, int tempoExecucaoSegundos)
    {
        StatusUltimaExecucao = "Erro";
        MensagemErro = mensagemErro;
        TempoExecucaoSegundos = tempoExecucaoSegundos;
        AtualizarDataModificacao();
    }
}
