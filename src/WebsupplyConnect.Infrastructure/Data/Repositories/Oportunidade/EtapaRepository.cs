using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Oportunidade
{
    public class EtapaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork, ILogger<EtapaRepository> logger) : BaseRepository(dbContext, unitOfWork), IEtapaRepository
    {
        private readonly ILogger<EtapaRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        public async Task<List<EtapaHistorico>> GetListEtapaHistorico(int oportunidadeId)
        {
            try
            {
                return await _context.EtapasHistorico.Where(e => e.OportunidadeId == oportunidadeId)
                    .Include(e => e.EtapaNova)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de etapa histórico pela a oportunidade id {id}.", oportunidadeId);
                throw;
            }
        }

        public async Task<EtapaHistorico> GetEtapaHistoricoById(int etapaHistoricoId)
        {
            try
            {
                return await _context.EtapasHistorico.FirstOrDefaultAsync(e => e.Id == etapaHistoricoId) ?? throw new InfraException("Etapa histórico não encontrado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a etapa histórico pelo id {id}.", etapaHistoricoId);
                throw;
            }
        }
    }
}
