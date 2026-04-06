using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoStatusLead : EntidadeBase
{
    public int StatusOrigemId { get; private set; }   // ID da tabela LeadStatus transacional
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string? Cor { get; private set; }
    public int Ordem { get; private set; }

    protected DimensaoStatusLead() { } // EF Core

    public DimensaoStatusLead(int statusOrigemId, string codigo, string nome,
        string? cor, int ordem) : base()
    {
        StatusOrigemId = statusOrigemId;
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Cor = cor;
        Ordem = ordem;
    }

    public void Atualizar(string codigo, string nome, string? cor, int ordem)
    {
        Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Cor = cor;
        Ordem = ordem;
        AtualizarDataModificacao();
    }
}
