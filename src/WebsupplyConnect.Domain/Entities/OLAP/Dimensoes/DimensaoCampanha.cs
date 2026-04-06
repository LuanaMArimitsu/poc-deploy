using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoCampanha : EntidadeBase
{
    public int CampanhaOrigemId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Codigo { get; private set; }
    public bool Ativo { get; private set; }
    public bool Temporaria { get; private set; }
    public int EmpresaId { get; private set; }
    public int GrupoEmpresaId { get; private set; }

    protected DimensaoCampanha() { } // EF Core

    public DimensaoCampanha(int campanhaOrigemId, string nome, string? codigo,
        bool ativo, bool temporaria, int empresaId, int grupoEmpresaId) : base()
    {
        CampanhaOrigemId = campanhaOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Codigo = codigo;
        Ativo = ativo;
        Temporaria = temporaria;
        EmpresaId = empresaId;
        GrupoEmpresaId = grupoEmpresaId;
    }

    public void Atualizar(string nome, string? codigo, bool ativo,
        bool temporaria, int empresaId, int grupoEmpresaId)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Codigo = codigo;
        Ativo = ativo;
        Temporaria = temporaria;
        EmpresaId = empresaId;
        GrupoEmpresaId = grupoEmpresaId;
        AtualizarDataModificacao();
    }
}
