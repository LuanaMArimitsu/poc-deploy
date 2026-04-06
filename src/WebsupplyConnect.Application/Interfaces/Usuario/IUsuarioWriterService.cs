using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Usuario
{
    public interface IUsuarioWriterService
    {
        Task<WebsupplyConnect.Domain.Entities.Usuario.Usuario> IncluirUsuarioDoAzureAsync(string azureUserId, int? usuarioSuperiorId, int empresaId, int canalPadraoId, int equipePadraoId, string cargo, string departamento, string? metadata = null);
        Task<bool> AtualizarUsuarioAsync(int id, AtualizarUsuarioRequestDTO request, int usuarioLogadoId);
        Task AlterarStatusUsuarioAsync(int usuarioId, bool novoStatus, int usuarioLogadoId);
        Task<bool> AssociarEmpresaAoUsuarioAsync(int usuarioId, int empresaId, int canalPadraoId, int equipePadraoId, string? metadata = null);      
        Task AtualizarVinculosEmpresasDoUsuario(int usuarioId, List<EmpresaVinculoDTO> empresasVinculos);
        Task AssociarMultiplasEmpresasAoUsuarioAsync(int usuarioId, AtualizarVinculosRequestDTO request);
        Task<bool?> DesassociarEmpresaAsync(int usuarioId, int empresaId);
        Task<bool> DefinirEmpresaPrincipalAsync(int usuarioId, int empresaId);
        Task<object> AlternarAssociacaoEmpresaAsync(int usuarioId, int empresaId, bool? definirComoPrincipal, int? canalPadraoId = null, int? equipePadraoId = null, string? metadata = null);
        Task<List<UsuarioHorarioDTO>> ConfigurarHorariosAsync(int usuarioId, List<HorarioTrabalhoDTO> horarios);
        Task AtualizarHorarioDiaAsync(int usuarioId, int diaSemanaId, AtualizarHorarioTrabalhoDTO horario);
        Task<bool> CopiarHorariosDeUsuarioAsync(int usuarioDestinoId, int usuarioOrigemId);
        Task RemoverHorarioDiaAsync(int usuarioId, int diaSemanaId);
        Task AplicarHorarioPadraoAsync(int usuarioId, string tipoPadrao);
        Task<ToleranciaResponseDTO> DefinirToleranciaAsync(int userId, bool ativo);
    }
}
