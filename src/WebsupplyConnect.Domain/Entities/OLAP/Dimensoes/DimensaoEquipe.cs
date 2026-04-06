using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoEquipe : EntidadeBase
{
    public int EquipeOrigemId { get; private set; }   // ID da tabela Equipe transacional
    public string Nome { get; private set; } = string.Empty;
    public int? TipoEquipeId { get; private set; }
    public int EmpresaId { get; private set; }
    public bool Ativa { get; private set; }

    protected DimensaoEquipe() { } // EF Core

    public DimensaoEquipe(int equipeOrigemId, string nome, int? tipoEquipeId,
        int empresaId, bool ativa) : base()
    {
        EquipeOrigemId = equipeOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        TipoEquipeId = tipoEquipeId;
        EmpresaId = empresaId;
        Ativa = ativa;
    }

    public void Atualizar(string nome, int? tipoEquipeId, int empresaId, bool ativa)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        TipoEquipeId = tipoEquipeId;
        EmpresaId = empresaId;
        Ativa = ativa;
        AtualizarDataModificacao();
    }
}
