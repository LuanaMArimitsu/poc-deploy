using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Oportunidade
{
    public class FunilRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork, ILogger<FunilRepository> logger) : BaseRepository(dbContext, unitOfWork), IFunilRepository
    {
        private readonly ILogger<FunilRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
