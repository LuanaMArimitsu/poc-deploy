using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Comunicacao
{
    public interface IMidiaRepository : IBaseRepository
    {
        Task<MidiaStatusProcessamento> GetMidiaStatusProcessamentoAsync(string codigo);

        Task<Midia> GetMidiaByMensagemId(int mensagemId);
    }
}
