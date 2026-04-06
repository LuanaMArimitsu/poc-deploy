using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Oportunidade
{
    public interface IEtapaRepository : IBaseRepository
    {
        Task<List<EtapaHistorico>> GetListEtapaHistorico(int oportunidadeId);
        Task<EtapaHistorico> GetEtapaHistoricoById(int etapaHistoricoId);
    }
}
