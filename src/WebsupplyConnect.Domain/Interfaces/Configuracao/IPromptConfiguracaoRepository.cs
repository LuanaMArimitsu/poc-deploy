using WebsupplyConnect.Domain.Entities.Configuracao;

namespace WebsupplyConnect.Domain.Interfaces.Configuracao;

public interface IPromptConfiguracaoRepository
{
    /// <summary>
    /// Obtém uma configuração de prompt pelo código.
    /// </summary>
    Task<PromptConfiguracao?> ObterPorCodigoAsync(string codigo);

    /// <summary>
    /// Obtém a última versão publicada de uma configuração.
    /// </summary>
    Task<PromptConfiguracaoVersao?> ObterUltimaVersaoPublicadaAsync(int promptConfiguracaoId);

    /// <summary>
    /// Verifica se uma empresa está vinculada a uma configuração.
    /// </summary>
    Task<bool> EmpresaVinculadaAsync(int promptConfiguracaoId, int empresaId);

    /// <summary>
    /// Cria uma nova configuração de prompt.
    /// </summary>
    Task<PromptConfiguracao> CreateAsync(PromptConfiguracao prompt);

    /// <summary>
    /// Cria uma nova versão de um prompt.
    /// </summary>
    Task<PromptConfiguracaoVersao> CreateVersaoAsync(PromptConfiguracaoVersao versao);
}
