using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Configuracao;

public class PromptConfiguracao : EntidadeBase
{
    public string Codigo { get; private set; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }

    public virtual ICollection<PromptConfiguracaoVersao> Versoes { get; private set; }
    public virtual ICollection<PromptConfiguracaoEmpresa> Empresas { get; private set; }

    protected PromptConfiguracao() { }

    /// <summary>
    /// Construtor de criação de nova configuração.
    /// </summary>
    public PromptConfiguracao(string codigo, string nome, string? descricao = null) : base()
    {
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        Versoes = new List<PromptConfiguracaoVersao>();
        Empresas = new List<PromptConfiguracaoEmpresa>();
    }

    /// <summary>
    /// Construtor para seed com Id e datas explícitas.
    /// </summary>
    public PromptConfiguracao(int id, string codigo, string nome, string? descricao,
        DateTime dataCriacao, DateTime dataModificacao)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        DataCriacao = dataCriacao;
        DataModificacao = dataModificacao;
        Excluido = false;
        Versoes = new List<PromptConfiguracaoVersao>();
        Empresas = new List<PromptConfiguracaoEmpresa>();
    }
}
