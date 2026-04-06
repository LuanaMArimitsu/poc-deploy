using WebsupplyConnect.Domain.Entities.Configuracao;

namespace WebsupplyConnect.Application.Interfaces.Configuracao;

/// <summary>
/// Serviço de aplicação para resolver e cachear prompts de IA.
/// Utiliza estratégia Cache-Aside com fallback para arquivo .txt.
/// </summary>
public interface IPromptConfiguracaoService
{
    /// <summary>
    /// Obtém o conteúdo de um prompt pelo código.
    /// Resolve: banco (última versão publicada) → fallback arquivo .txt.
    /// Cache Redis: chave "prompt-config:{codigo}" (ou com empresaId se informado)
    /// </summary>
    /// <param name="codigo">Código da configuração (ex: "CLASSIFICACAO_CONVERSA")</param>
    /// <param name="empresaId">ID da empresa (opcional, para override futuro)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Conteúdo do prompt ou null se não encontrado</returns>
    Task<string?> ObterConteudoAsync(
        string codigo,
        int? empresaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a versão executável completa (conteúdo + provider + modelo).
    /// Sempre retorna a última versão publicada.
    /// </summary>
    /// <param name="codigo">Código da configuração</param>
    /// <param name="empresaId">ID da empresa (opcional)</param>
    /// <returns>Versão completa ou null</returns>
    Task<PromptConfiguracaoVersao?> ObterVersaoExecutavelAsync(
        string codigo,
        int? empresaId = null);

    /// <summary>
    /// Invalida o cache Redis para um prompt específico.
    /// Chamado quando há atualização/publicação de nova versão.
    /// </summary>
    /// <param name="codigo">Código do prompt</param>
    /// <param name="empresaId">ID da empresa (opcional)</param>
    /// <returns></returns>
    Task InvalidarCacheAsync(string codigo, int? empresaId = null);
}
