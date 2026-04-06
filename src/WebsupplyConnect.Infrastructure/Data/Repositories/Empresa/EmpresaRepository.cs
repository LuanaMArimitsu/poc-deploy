using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Empresa;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Empresa;

internal class EmpresaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IEmpresaRepository
{
    private readonly List<string> filiaisSemAtendimento =
    [
        "67375899000191",
        "67375899001170",
        "03647692000233",
    ];

    public async Task<bool> EmpresaExisteAsync(int empresaId)
    {
        return await _context.Empresa.AnyAsync(e => e.Id == empresaId);
    }

    public async Task<bool> ExistemEmpresasAtivasAsync(List<int> empresasIds)
    {
        var totalAtivas = await _context.Empresa
            .CountAsync(e => empresasIds.Contains(e.Id) && !e.Excluido && e.Ativo);

        return totalAtivas == empresasIds.Distinct().Count();
    }

    public IQueryable<WebsupplyConnect.Domain.Entities.Empresa.Empresa> Query()
    {
        return _context.Empresa.AsQueryable();
    }

    public async Task<Domain.Entities.Empresa.Empresa?> ObterPorIdAsync(int empresaId)
    {
        return await _context.Empresa
            .FirstOrDefaultAsync(e => e.Id == empresaId && !e.Excluido);
    }

    public async Task<List<Domain.Entities.Empresa.Empresa>> ListarEmpresasAtivasAsync()
    {
        return await _context.Empresa
            .Where(e => !e.Excluido)
            .ToListAsync();
    }

    public async Task<Domain.Entities.Empresa.Empresa> GetGrupoEmpresaByEmpresaId(int empresaId)
    {
        var empresa = await _context.Empresa
            .Include(e => e.GrupoEmpresa)
            .FirstOrDefaultAsync(e => e.Id == empresaId && e.Ativo && !e.Excluido);
        return empresa!;
    }   

    public Task<List<Domain.Entities.Empresa.Empresa>> GetFiliais(int grupoEmpresaId)
    {
        return _context.Empresa
            .Where(e => !filiaisSemAtendimento.Contains(e.Cnpj) && e.GrupoEmpresaId == grupoEmpresaId && e.Ativo && !e.Excluido)
            .ToListAsync();
    }

    public async Task<Domain.Entities.Empresa.Empresa> GetEmpresaPorCnpjAsync(string cnpjEmpresa)
    {
        return await _context.Empresa
            .FirstOrDefaultAsync(e => e.Cnpj == cnpjEmpresa && e.Ativo && !e.Excluido);
    }
}
