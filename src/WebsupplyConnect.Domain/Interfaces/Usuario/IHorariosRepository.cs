using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Usuario
{
    public interface IHorariosRepository : IBaseRepository
    {
        Task RemoverHorariosPorUsuarioAsync(int usuarioId);
        Task AdicionarAsync(UsuarioHorario horario);
        Task<UsuarioHorario?> ObterUsuarioEDiaAsync(int usuarioId, int diaSemanaId);
        Task RemoverAsync(UsuarioHorario horario);
        Task<List<UsuarioHorario>> ObterPorUsuarioAsync(int usuarioId);
    }
}
