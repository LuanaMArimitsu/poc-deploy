namespace WebsupplyConnect.Domain.Entities.Configuracao;

/// <summary>
/// Junção N:N entre PromptConfiguracao e Empresa.
/// Sem FK de versão — o runtime sempre resolve a última versão publicada do PromptConfiguracao.
/// </summary>
public class PromptConfiguracaoEmpresa
{
    public int PromptConfiguracaoId { get; private set; }
    public int EmpresaId { get; private set; }

    public virtual PromptConfiguracao PromptConfiguracao { get; private set; }
    public virtual Empresa.Empresa Empresa { get; private set; }

    protected PromptConfiguracaoEmpresa() { }

    public PromptConfiguracaoEmpresa(int promptConfiguracaoId, int empresaId)
    {
        PromptConfiguracaoId = promptConfiguracaoId;
        EmpresaId = empresaId;
    }
}
