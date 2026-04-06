using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

/// <summary>
/// Dimensão de funil de vendas (um registro por funil transacional).
/// </summary>
public class DimensaoFunil : EntidadeBase
{
    public int FunilOrigemId { get; private set; }
    public int EmpresaOrigemId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public bool EhPadrao { get; private set; }
    public string? Cor { get; private set; }

    protected DimensaoFunil() { }

    public DimensaoFunil(int funilOrigemId, int empresaOrigemId, string nome, bool ativo, bool ehPadrao, string? cor) : base()
    {
        FunilOrigemId = funilOrigemId;
        EmpresaOrigemId = empresaOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Ativo = ativo;
        EhPadrao = ehPadrao;
        Cor = cor;
    }

    public void Atualizar(string nome, bool ativo, bool ehPadrao, string? cor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Ativo = ativo;
        EhPadrao = ehPadrao;
        Cor = cor;
        AtualizarDataModificacao();
    }
}
