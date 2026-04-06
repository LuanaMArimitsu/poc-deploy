using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

/// <summary>
/// Dimensão de etapa do funil (grain: uma linha por etapa transacional).
/// </summary>
public class DimensaoEtapaFunil : EntidadeBase
{
    public int EtapaOrigemId { get; private set; }
    public int FunilDimensaoId { get; private set; }
    /// <summary>ID do funil transacional (denormalizado para filtros e validação).</summary>
    public int FunilOrigemId { get; private set; }

    public string Nome { get; private set; } = string.Empty;
    public int Ordem { get; private set; }
    public string Cor { get; private set; } = string.Empty;
    public int ProbabilidadePadrao { get; private set; }
    public bool EhAtiva { get; private set; }
    public bool EhFinal { get; private set; }
    public bool EhVitoria { get; private set; }
    public bool EhPerdida { get; private set; }
    public bool EhExibida { get; private set; }
    public bool Ativo { get; private set; }

    public virtual DimensaoFunil Funil { get; private set; } = null!;

    protected DimensaoEtapaFunil() { }

    public DimensaoEtapaFunil(
        int etapaOrigemId,
        int funilDimensaoId,
        int funilOrigemId,
        string nome,
        int ordem,
        string cor,
        int probabilidadePadrao,
        bool ehAtiva,
        bool ehFinal,
        bool ehVitoria,
        bool ehPerdida,
        bool ehExibida,
        bool ativo) : base()
    {
        EtapaOrigemId = etapaOrigemId;
        FunilDimensaoId = funilDimensaoId;
        FunilOrigemId = funilOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Ordem = ordem;
        Cor = cor ?? string.Empty;
        ProbabilidadePadrao = probabilidadePadrao;
        EhAtiva = ehAtiva;
        EhFinal = ehFinal;
        EhVitoria = ehVitoria;
        EhPerdida = ehPerdida;
        EhExibida = ehExibida;
        Ativo = ativo;
    }

    public void Atualizar(
        int funilDimensaoId,
        int funilOrigemId,
        string nome,
        int ordem,
        string cor,
        int probabilidadePadrao,
        bool ehAtiva,
        bool ehFinal,
        bool ehVitoria,
        bool ehPerdida,
        bool ehExibida,
        bool ativo)
    {
        FunilDimensaoId = funilDimensaoId;
        FunilOrigemId = funilOrigemId;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Ordem = ordem;
        Cor = cor ?? string.Empty;
        ProbabilidadePadrao = probabilidadePadrao;
        EhAtiva = ehAtiva;
        EhFinal = ehFinal;
        EhVitoria = ehVitoria;
        EhPerdida = ehPerdida;
        EhExibida = ehExibida;
        Ativo = ativo;
        AtualizarDataModificacao();
    }
}
