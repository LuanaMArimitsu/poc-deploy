using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Comunicacao
{
    public interface ITemplateRepository: IBaseRepository
    {
        Task<Template?> GetTemplateByOrigem(int origem, int canalId);
    }
}
