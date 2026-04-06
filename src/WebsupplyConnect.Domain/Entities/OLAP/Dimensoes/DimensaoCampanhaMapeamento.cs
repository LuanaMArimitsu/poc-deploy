using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoCampanhaMapeamento : EntidadeBase
{
    public int CampanhaOrigemId { get; private set; }
    public int DimensaoCampanhaId { get; private set; }

    protected DimensaoCampanhaMapeamento() { } // EF Core

    public DimensaoCampanhaMapeamento(int campanhaOrigemId, int dimensaoCampanhaId) : base()
    {
        CampanhaOrigemId = campanhaOrigemId;
        DimensaoCampanhaId = dimensaoCampanhaId;
    }
}
