using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Domain.Entities.Oportunidade;

namespace WebsupplyConnect.Application.Interfaces.Oportunidade
{
    public interface IEtapaReaderService
    {
        Task<List<Etapa>> GetListEtapaByFunil(int funilId);
        Task<Etapa?> GetEtapaById(int etapaId);
        Task<List<EtapaHistoricoListDTO>> GetListEtapaHistorico(int oportunidadeId);
        Task<List<Etapa>> ListarEtapasParaETLAsync(CancellationToken cancellationToken = default);
    }
}
