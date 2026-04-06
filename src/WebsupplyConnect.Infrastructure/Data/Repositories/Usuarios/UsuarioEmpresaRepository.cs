using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Usuario;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Usuarios
{
    public class UsuarioEmpresaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IUsuarioEmpresaRepository
    {
        public async Task<UsuarioEmpresa?> GetVinculoUsuarioEmpresaAsync(int usuarioId, int empresaId)
        {
            return await _context.UsuarioEmpresa
                .Include(ue => ue.CanalPadrao)
                .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);
        }

        public async Task<List<UsuarioEmpresa>> GetVinculosPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.UsuarioEmpresa
                .Where(ue => ue.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<UsuarioEmpresa?> GetBotVinculoByEmpresaAsync(int empresaId)
        {
            return await _context.UsuarioEmpresa
                .Include(ue => ue.Usuario)
                .FirstOrDefaultAsync(ue => ue.EmpresaId == empresaId && ue.Usuario.IsBot);
        }

        public async Task<UsuarioEmpresa?> GetUsuarioEmpresaAsync(int empresaId, int usuarioLogado)
        {
            return await _context.UsuarioEmpresa.Where(x => x.UsuarioId == usuarioLogado && x.EmpresaId == empresaId).FirstOrDefaultAsync();
        }

        public async Task<UsuarioEmpresa> GetEquipePadraoVinculoAsync(int usuarioId, int empresaId)
        {
            return await _context.UsuarioEmpresa
                .Include(ue => ue.EquipePadrao)
                .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);
        }
    }
}
