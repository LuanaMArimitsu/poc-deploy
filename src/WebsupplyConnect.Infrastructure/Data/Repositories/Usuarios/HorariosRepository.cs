using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Usuario;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Usuarios
{
    internal class HorariosRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IHorariosRepository
    {
        public async Task RemoverHorariosPorUsuarioAsync(int usuarioId)
        {
            var horarios = await _context.UsuarioHorario
                .Where(h => h.UsuarioId == usuarioId)
                .ToListAsync();

            if (horarios.Any())
                _context.UsuarioHorario.RemoveRange(horarios);
        }

        public async Task AdicionarAsync(UsuarioHorario horario)
        {
            await CreateAsync(horario);
        }

        public async Task<UsuarioHorario?> ObterUsuarioEDiaAsync(int usuarioId, int diaSemanaId)
        {
            return await _context.UsuarioHorario
                .FirstOrDefaultAsync(h => h.UsuarioId == usuarioId && h.DiaSemanaId == diaSemanaId);
        }

        public Task RemoverAsync(UsuarioHorario horario)
        {
            _context.UsuarioHorario.Remove(horario);
            return Task.CompletedTask;
        }

        public async Task<List<UsuarioHorario>> ObterPorUsuarioAsync(int usuarioId)
        {
            return await _context.UsuarioHorario
                .Where(h => h.UsuarioId == usuarioId)
                .ToListAsync();
        }
    }
}
