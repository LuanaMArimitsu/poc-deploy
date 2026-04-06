using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoEmpresa : EntidadeBase
{
    public int EmpresaOrigemId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }
    public int GrupoEmpresaId { get; private set; }

    protected DimensaoEmpresa() { } // EF Core

    public DimensaoEmpresa(int empresaOrigemId, string nome, bool ativa, int grupoEmpresaId) : base()
    {
        EmpresaOrigemId = empresaOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Ativa = ativa;
        GrupoEmpresaId = grupoEmpresaId;
    }

    public void Atualizar(string nome, bool ativa, int grupoEmpresaId)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Ativa = ativa;
        GrupoEmpresaId = grupoEmpresaId;
        AtualizarDataModificacao();
    }
}
