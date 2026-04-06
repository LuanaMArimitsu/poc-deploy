using WebsupplyConnect.Application.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.ControleSistemasExternos;

namespace WebsupplyConnect.Application.Services.ControleSistemasExternos
{
    public class EventoIntegracaoWriterService(IEventoIntegracaoRepository eventoIntegracaoRepository, IUnitOfWork unitOfWork) : IEventoIntegracaoWriterService
    {
        private readonly IEventoIntegracaoRepository _eventoIntegracaoRepository = eventoIntegracaoRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task RegistrarAsync(EventoIntegracao evento, bool commit = true)
        {
            try
            {
                await _eventoIntegracaoRepository.CreateAsync(evento);

                await _unitOfWork.SaveChangesAsync();

                if (commit)
                {
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}