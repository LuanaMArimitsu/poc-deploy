using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Usuario;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Usuarios
{
    internal class DispositivosRepository : BaseRepository, IDispositivosRepository
    {
        public DispositivosRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Lista dispositivos com filtros de usuario e status ativo
        /// </summary>
        /// <param name="usuarioId">ID do Usuario</param>
        /// <param name="ativo">Status ativo do dispositivo para filtrar (opcional - true=apenas ativos, false=apenas inativos, null=todos os status)</param>
        /// <returns>Lista de dispositivos ordenada por data</returns>
        public async Task<List<Dispositivo>> DispositivosUserAsync(int usuarioId, bool? ativo = null)
        {
            var query = _context.Set<Dispositivo>().Where(x => x.UsuarioId == usuarioId && x.Excluido == false).AsQueryable();

            if (ativo.HasValue)
            {
                query = query.Where(x => x.Ativo == ativo.Value);
            }

            return await query
                .OrderBy(x => x.DataCriacao)
                .ToListAsync();
        }

        public async Task<Dispositivo?> ObterPorDeviceIdAsync(int usuarioId, string deviceId)
        {
            return await _context.Dispositivo
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId && d.DeviceId == deviceId && !d.Excluido);
        }

        public async Task<Dispositivo?> ObterPorIdAsync(int id)
        {
            return await _context.Dispositivo
                .Include(d => d.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && !d.Excluido);
        }

        public IQueryable<Dispositivo> ObterQueryComUsuario()
        {
            return _context.Dispositivo
                .Include(d => d.Usuario)
                .Where(d => !d.Excluido)
                .AsQueryable();
        }

        public async Task<Dispositivo?> ObterDetalhadoPorDeviceIdAsync(string deviceId)
        {
            return await _context.Dispositivo
                .Include(d => d.Usuario)
                    .ThenInclude(u => u.UsuarioEmpresas)
                        .ThenInclude(ue => ue.Empresa)
                .Where(d => !d.Excluido && d.DeviceId == deviceId)
                .FirstOrDefaultAsync();
        }

        public async Task AtualizarAsync(Dispositivo dispositivo)
        {
            Update(dispositivo);
            await Task.CompletedTask;
        }
    }
}
