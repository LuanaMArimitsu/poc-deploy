using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Domain.Entities.Oportunidade;

namespace WebsupplyConnect.Application.Interfaces.Oportunidade
{
    public interface IFunilReaderService
    {
        Task<List<GetEtapasDTO>> GetFunilByEmpresa(int empresaId);
        Task<List<Funil>> ListarFunisParaETLAsync(CancellationToken cancellationToken = default);
    }
}
