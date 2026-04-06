namespace WebsupplyConnect.Application.Helpers;

/// <summary>
/// Helper para abreviar nomes completos de vendedores.
/// Regras: mantém o primeiro nome completo, abrevia sobrenomes intermediários (primeira letra + .),
/// sempre mantém o último sobrenome, retorna no máximo 20 caracteres.
/// </summary>
public static class NomeVendedorHelper
{
    private const int MaxCaracteres = 20;

    /// <summary>
    /// Abrevia o nome completo do vendedor.
    /// Exemplo: "João Pedro da Silva Santos" -> "João P. d. S. Santos" (ou truncado para 20 chars)
    /// </summary>
    /// <param name="nomeCompleto">Nome completo do vendedor. Retorna string vazia se null ou vazio.</param>
    /// <returns>Nome resumido com no máximo 20 caracteres.</returns>
    public static string AbreviarNome(string? nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            return string.Empty;

        var partes = nomeCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return string.Empty;
        if (partes.Length == 1) return Truncar(partes[0], MaxCaracteres);

        var primeiro = partes[0];
        var ultimo = partes[^1];
        var meio = partes.Skip(1).Take(partes.Length - 2).ToList();

        string resultado;
        if (meio.Count == 0)
        {
            resultado = $"{primeiro} {ultimo}";
        }
        else
        {
            var abreviacoes = meio.Select(p => p.Length > 0 ? $"{char.ToUpperInvariant(p[0])}." : "").Where(s => s != "");
            var meioAbreviado = string.Join(" ", abreviacoes);
            resultado = $"{primeiro} {meioAbreviado} {ultimo}";
        }

        return Truncar(resultado, MaxCaracteres);
    }

    private static string Truncar(string valor, int maxLen)
    {
        if (valor.Length <= maxLen) return valor;
        return valor[..maxLen];
    }
}
