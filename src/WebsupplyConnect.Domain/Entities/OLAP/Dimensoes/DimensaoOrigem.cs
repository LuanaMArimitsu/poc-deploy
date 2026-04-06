using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoOrigem : EntidadeBase
{
    public int OrigemOrigemId { get; private set; }   // ID da tabela Origem transacional
    public string Nome { get; private set; } = string.Empty;
    public int? OrigemTipoId { get; private set; }
    public string? Descricao { get; private set; }

    protected DimensaoOrigem() { } // EF Core

    public DimensaoOrigem(int origemOrigemId, string nome, int? origemTipoId,
        string? descricao) : base()
    {
        OrigemOrigemId = origemOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        OrigemTipoId = origemTipoId;
        Descricao = descricao;
    }

    public void Atualizar(string nome, int? origemTipoId, string? descricao)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        OrigemTipoId = origemTipoId;
        Descricao = descricao;
        AtualizarDataModificacao();
    }
}
