using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Configuracao;

/// <summary>
/// Representa uma versão imutável de um prompt.
/// O runtime sempre usa a última versão com Publicada = true.
/// </summary>
public class PromptConfiguracaoVersao : EntidadeBase
{
    public int PromptConfiguracaoId { get; private set; }
    public int NumeroVersao { get; private set; }
    public bool Publicada { get; private set; }

    public string Provider { get; private set; }
    public string Modelo { get; private set; }
    public string ConteudoPrompt { get; private set; }

    public DateTime? DataPublicacao { get; private set; }
    public int ContadorUso { get; private set; }

    public virtual PromptConfiguracao PromptConfiguracao { get; private set; }

    protected PromptConfiguracaoVersao() { }

    /// <summary>
    /// Construtor de criação de nova versão (não publicada).
    /// </summary>
    public PromptConfiguracaoVersao(
        int promptConfiguracaoId,
        int numeroVersao,
        string provider,
        string modelo,
        string conteudoPrompt) : base()
    {
        PromptConfiguracaoId = promptConfiguracaoId;
        NumeroVersao = numeroVersao;
        Provider = provider;
        Modelo = modelo;
        ConteudoPrompt = conteudoPrompt;
        Publicada = false;
        ContadorUso = 0;
    }

    /// <summary>
    /// Construtor para seed com Id e datas explícitas.
    /// </summary>
    public PromptConfiguracaoVersao(
        int id,
        int promptConfiguracaoId,
        int numeroVersao,
        bool publicada,
        string provider,
        string modelo,
        string conteudoPrompt,
        DateTime dataCriacao,
        DateTime dataModificacao,
        DateTime? dataPublicacao = null)
    {
        Id = id;
        PromptConfiguracaoId = promptConfiguracaoId;
        NumeroVersao = numeroVersao;
        Publicada = publicada;
        Provider = provider;
        Modelo = modelo;
        ConteudoPrompt = conteudoPrompt;
        DataCriacao = dataCriacao;
        DataModificacao = dataModificacao;
        DataPublicacao = dataPublicacao;
        Excluido = false;
        ContadorUso = 0;
    }

    /// <summary>
    /// Publica esta versão.
    /// </summary>
    public void Publicar()
    {
        Publicada = true;
        DataPublicacao = TimeHelper.GetBrasiliaTime();
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Registra o uso deste prompt (incrementa contador).
    /// </summary>
    public void RegistrarUso()
    {
        ContadorUso++;
    }
}
