using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;

namespace WebsupplyConnect.Application.Interfaces.ControleSistemasExternos
{
    public interface IEventoIntegracaoWriterService
    {
        Task RegistrarAsync(EventoIntegracao evento, bool commit = true);
    }
}