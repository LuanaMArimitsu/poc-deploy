using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Produto;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Produto
{
    internal class ProdutoHistoricoRepository : BaseRepository, IProdutoHistoricoRepository
    {
        public ProdutoHistoricoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        public async Task AdicionarAsync(ProdutoHistorico historico)
        {
            await CreateAsync(historico);
        }
    }
}
