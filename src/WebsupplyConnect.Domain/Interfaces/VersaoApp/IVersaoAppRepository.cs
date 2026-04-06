using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.VersaoApp
{
    public interface IVersaoAppRepository : IBaseRepository
    {
        Task<Domain.Entities.VersaoApp.VersaoApp> GetUltimaVersaoAppAsync(string? plataformaApp);
    }
}
