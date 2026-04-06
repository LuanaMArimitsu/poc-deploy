using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    public interface IFilaDistribuicaoReaderService
    {
        Task<FilaDistribuicao> GetMembroEquipeFilaDistribuicaoById(int membroEquipeId);
    }
}
