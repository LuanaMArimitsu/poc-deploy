using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Produto;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Produto
{
    internal class ProdutoRepository : BaseRepository, IProdutoRepository
    {
        public ProdutoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        public async Task AdicionarAsync(WebsupplyConnect.Domain.Entities.Produto.Produto produto)
        {
            await CreateAsync(produto);
        }

        public async Task<WebsupplyConnect.Domain.Entities.Produto.Produto> ObterComEmpresasAsync(int produtoId)
        {
            return await _context.Produto
                .Include(p => p.ProdutoEmpresas)
                .ThenInclude(pe => pe.Empresa)
                .FirstOrDefaultAsync(p => p.Id == produtoId);
        }

        public async Task<WebsupplyConnect.Domain.Entities.Produto.Produto?> ObterPorIdAsync(int id)
        {
            return await _context.Produto.FirstOrDefaultAsync(p => p.Id == id && !p.Excluido);
        }

        public async Task AtualizarAsync(WebsupplyConnect.Domain.Entities.Produto.Produto produto)
        {
            Update(produto);
            await Task.CompletedTask;
        }

        //Remove mais de um vinculo de uma vez
        public async Task RemoverVinculosEmpresasAsync(int produtoId)
        {
            var vinculos = await _context.ProdutoEmpresa
                .Where(pe => pe.ProdutoId == produtoId)
                .ToListAsync();

            _context.ProdutoEmpresa.RemoveRange(vinculos);
        }

        //Remove apenas um vinculo por vez
        public async Task RemoverVinculoEmpresaAsync(int produtoId, int empresaId)
        {
            var vinculo = await _context.ProdutoEmpresa
                .FirstOrDefaultAsync(pe => pe.ProdutoId == produtoId && pe.EmpresaId == empresaId);

            if (vinculo != null)
                _context.ProdutoEmpresa.Remove(vinculo);
        }

        public IQueryable<WebsupplyConnect.Domain.Entities.Produto.Produto> ObterQueryProdutosComEmpresas(int empresaId)
        {
            return _context.Produto
                .Include(p => p.ProdutoEmpresas)
                    .ThenInclude(pe => pe.Empresa)
                .Where(p => !p.Excluido &&
                            p.ProdutoEmpresas.Any(pe => pe.EmpresaId == empresaId));
        }

        public async Task<WebsupplyConnect.Domain.Entities.Produto.Produto?> ObterDetalhePorIdAsync(int id)
        {
                return await _context.Produto
                .AsSplitQuery()
                .Include(p => p.ProdutoEmpresas)
                    .ThenInclude(pe => pe.Empresa)
                .Include(p => p.Historicos)
                    .ThenInclude(h => h.Usuario)
                .Include(p => p.Historicos)
                    .ThenInclude(h => h.TipoOperacao)
                .FirstOrDefaultAsync(p => p.Id == id && !p.Excluido);
        }

        public async Task<ProdutoEmpresa?> ObterVinculoProdutoEmpresaAsync(int produtoId, int empresaId)
        {
            return await _context.ProdutoEmpresa
                .FirstOrDefaultAsync(pe => pe.ProdutoId == produtoId && pe.EmpresaId == empresaId);
        }

        public async Task AtualizarVinculoProdutoEmpresaAsync(ProdutoEmpresa vinculo)
        {
            Update(vinculo);
            await Task.CompletedTask;
        }
    }
}
