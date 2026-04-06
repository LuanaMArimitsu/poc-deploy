using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.API.Controllers.Usuario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly IUsuarioWriterService _usuarioWriterService;
        private readonly IDispositivosWriterService _dispositivoWriterService;
        private readonly IDispositivosReaderService _dispositivoReaderService;
        private readonly IVendedorEstatisticasService _estatisticasService;
        private readonly IFilaDistribuicaoService _filaService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(
            IUsuarioReaderService usuarioReaderService,
            IUsuarioWriterService usuarioWriterService,
            IDispositivosWriterService dispositivosWriterService,
            IDispositivosReaderService dispositivoReaderService,
            IVendedorEstatisticasService estatisticasService,
            IFilaDistribuicaoService filaService,
            ILogger<UsuarioController> logger)
        {
            _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
            _usuarioWriterService = usuarioWriterService ?? throw new ArgumentNullException(nameof(usuarioWriterService));
            _dispositivoWriterService = dispositivosWriterService ?? throw new ArgumentNullException(nameof(dispositivosWriterService));
            _dispositivoReaderService = dispositivoReaderService ?? throw new ArgumentNullException(nameof(dispositivoReaderService));
            _estatisticasService = estatisticasService ?? throw new ArgumentNullException(nameof(estatisticasService));
            _filaService = filaService ?? throw new ArgumentNullException(nameof(filaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("search-azure-ad")]
        public async Task<ActionResult<ApiResponse<List<AzureUserDTO>>>> GetUsuariosAzure([FromQuery] string? nome = null)
        {
            try
            {
                var usuarios = await _usuarioReaderService.BuscarUsuariosAzureAdPorNome(nome);

                if (usuarios == null || usuarios.Count == 0)
                {
                    _logger.LogWarning("Nenhum usuário encontrado no Azure AD com o nome informado.");
                    return NotFound(ApiResponse<object>.ErrorResponse("Nenhum usuário encontrado com o nome informado."));
                }

                return Ok(ApiResponse<List<AzureUserDTO>>.SuccessResponse(usuarios, "Usuários encontrados com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuários no Azure AD.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar usuários. Tente novamente mais tarde."));
            }
        }


        [HttpPost("azure-add-user")]
        public async Task<ActionResult<ApiResponse<int>>> IncluirUsuarioAzure([FromBody] AzureAddUserRequest request)
        {
            try
            {
                var usuario = await _usuarioWriterService.IncluirUsuarioDoAzureAsync(
                    request.AzureUserId,
                    request.UsuarioSuperiorId,
                    request.EmpresaId,
                    request.CanalPadraoId,
                    request.EquipePadraoId,
                    request.Cargo,
                    request.Departamento
                );

                return Ok(ApiResponse<int>.SuccessResponse(usuario.Id, "Usuário incluído com sucesso."));
            }
            catch (DomainException dex)
            {
                _logger.LogWarning(dex, "Validação de domínio falhou ao incluir usuário do Azure.");
                return BadRequest(ApiResponse<int>.ErrorResponse(dex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao incluir usuário do Azure. AzureUserId: {AzureUserId}", request.AzureUserId);
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Erro ao incluir usuário. Tente novamente mais tarde."));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UsuarioDetalheDTO>>> ObterUsuarioPorId(int id)
        {
            try
            {
                var usuario = await _usuarioReaderService.ObterUsuarioDetalhadoAsync(id);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuário com ID {UsuarioId} não encontrado.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));
                }

                return Ok(ApiResponse<UsuarioDetalheDTO>.SuccessResponse(usuario, "Usuário retornado com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do usuário com ID {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar o usuário. Tente novamente mais tarde."));
            }
        }

        [HttpGet("obterWeb/{id:int}")]
        public async Task<ActionResult<ApiResponse<UsuarioDetalheSimplesDTO>>> ObterUsuarioPorIdWeb(int id)
        {
            try
            {
                var usuario = await _usuarioReaderService.ObterUsuarioDetalhadoSimplesAsync(id);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuário com ID {UsuarioId} não encontrado.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));
                }

                return Ok(ApiResponse<UsuarioDetalheSimplesDTO>.SuccessResponse(usuario, "Usuário retornado com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do usuário com ID {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar o usuário. Tente novamente mais tarde."));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponseDTO<UsuarioListagemDTO>>>> ListarUsuarios([FromQuery] UsuarioFiltroRequestDTO filtro)
        {
            try
            {
                if (filtro == null)
                    filtro = new UsuarioFiltroRequestDTO();

                var usuariosPaged = await _usuarioReaderService.ListarUsuariosAsync(filtro);

                var response = new ApiResponse<PagedResponseDTO<UsuarioListagemDTO>>
                {
                    Success = true,
                    Data = usuariosPaged,
                    Message = "Usuários listados com sucesso."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar usuários.");
                return StatusCode(500, new ApiResponse<PagedResponseDTO<UsuarioListagemDTO>>
                {
                    Success = false,
                    Error = ex.Message,
                    Message = "Erro ao listar usuários."
                });
            }
        }

        [HttpGet("superiores")]
        public async Task<ActionResult<ApiResponse<List<UsuarioSuperiorDTO>>>> ObterUsuariosSuperioresAsync()
        {
            var superiores = await _usuarioReaderService.ObterUsuariosSuperioresAsync();

            if (superiores == null || !superiores.Any())
            {
                return Ok(ApiResponse<List<UsuarioSuperiorDTO>>.SuccessResponse(
                    new List<UsuarioSuperiorDTO>(), "Não há usuários superiores cadastrados."));
            }

            return Ok(ApiResponse<List<UsuarioSuperiorDTO>>.SuccessResponse(
                superiores, "Usuários superiores obtidos com sucesso."));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> AtualizarUsuario(int id, AtualizarUsuarioRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var usuarioLogadoId))
            {
                _logger.LogWarning("Usuário não autenticado ou claim inválida.");
                return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado."));
            }

            try
            {
                var atualizado = await _usuarioWriterService.AtualizarUsuarioAsync(id, request, usuarioLogadoId);

                if (!atualizado)
                {
                    _logger.LogWarning("Tentativa de atualizar usuário com ID {UsuarioId}, mas ele não foi encontrado.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));
                }

                return Ok(ApiResponse<string>.SuccessResponse("Usuário atualizado com sucesso."));
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Erro de negócio ao atualizar usuário com ID {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar usuário com ID {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao atualizar usuário. Tente novamente mais tarde."));
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<string>>> AlterarStatusUsuario(int id, [FromBody] AlterarStatusRequestDTO dto)
        {
            if (!ModelState.IsValid || dto == null)
            {
                _logger.LogWarning("Requisição inválida para alteração de status do usuário com ID {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Dados inválidos."));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var usuarioLogadoId))
            {
                _logger.LogWarning("Usuário não autenticado ao tentar alterar status do usuário com ID {UsuarioId}.", id);
                return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado."));
            }

            try
            {
                await _usuarioWriterService.AlterarStatusUsuarioAsync(id, dto.Ativo, usuarioLogadoId);

                return Ok(ApiResponse<string>.SuccessResponse("Status do usuário atualizado com sucesso."));
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Erro de negócio ao alterar status do usuário com ID {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao alterar status do usuário com ID {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao alterar status do usuário. Tente novamente mais tarde."));
            }
        }

        [HttpGet("{id}/empresas")]
        public async Task<ActionResult<ApiResponse<List<UsuarioEmpresaDTO>>>> ObterEmpresasUsuario(int id)
        {
            try
            {
                var empresas = await _usuarioReaderService.ObterEmpresasUsuarioAsync(id);

                if (empresas == null || !empresas.Any())
                {
                    _logger.LogWarning("Usuário com ID {UsuarioId} não encontrado ou sem empresas associadas.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado ou sem empresas associadas."));
                }

                return Ok(ApiResponse<List<UsuarioEmpresaDTO>>.SuccessResponse(empresas, "Empresas associadas ao usuário retornadas com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter empresas associadas ao usuário com ID {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar empresas do usuário. Tente novamente mais tarde."));
            }
        }

        [HttpPost("{id}/empresas/{empresaId}/canal/{canalPadraoId}")]
        public async Task<ActionResult<ApiResponse<string>>> AssociarEmpresaAoUsuario(int id, int empresaId, int canalPadraoId, int equipePadraoId)
        {
            try
            {
                var resultado = await _usuarioWriterService.AssociarEmpresaAoUsuarioAsync(id, empresaId, canalPadraoId, equipePadraoId);

                if (resultado)
                {
                    return Ok(ApiResponse<string>.SuccessResponse("Empresa associada com sucesso."));
                }

                _logger.LogWarning("Tentativa de associar empresa já vinculada. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}", id, empresaId);
                return Conflict(ApiResponse<object>.ErrorResponse("Empresa já está associada a este usuário."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao associar empresa {EmpresaId} ao usuário {UsuarioId}.", empresaId, id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao associar empresa ao usuário. Tente novamente mais tarde."));
            }
        }

        [HttpPut("{id}/empresas/vinculos")]
        public async Task<ActionResult<ApiResponse<string>>> AtualizarVinculosEmpresas(int id, [FromBody] AtualizarVinculosRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("ModelState inválido ao atualizar vínculos de empresas do usuário {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Dados inválidos."));
            }

            if (request?.EmpresasVinculos == null || !request.EmpresasVinculos.Any())
            {
                _logger.LogError("Requisição com lista de vínculos vazia ou nula ao atualizar vínculos do usuário {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("A lista de vínculos está vazia ou não foi informada."));
            }

            try
            {
                await _usuarioWriterService.AtualizarVinculosEmpresasDoUsuario(id, request.EmpresasVinculos);
                return Ok(ApiResponse<string>.SuccessResponse("Vínculos atualizados com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar vínculos de empresas do usuário {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}/empresas")]
        public async Task<ActionResult<ApiResponse<string>>> AssociarMultiplasEmpresasUsuario(int id, [FromBody] AtualizarVinculosRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido ao substituir empresas do usuário {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Dados inválidos."));
            }

            try
            {
                await _usuarioWriterService.AssociarMultiplasEmpresasAoUsuarioAsync(id, request);
                return Ok(ApiResponse<string>.SuccessResponse("Empresas substituídas com sucesso."));
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Erro de negócio ao substituir empresas do usuário {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao substituir empresas do usuário {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao substituir empresas. Tente novamente mais tarde."));
            }
        }

        [HttpDelete("{id}/empresas/{empresaId}")]
        public async Task<ActionResult<ApiResponse<object>>> DesassociarEmpresa(int id, int empresaId)
        {
            try
            {

                var resultado = await _usuarioWriterService.DesassociarEmpresaAsync(id, empresaId);

                if (resultado is null)
                {
                    _logger.LogWarning("Usuário com ID {UsuarioId} não encontrado ao tentar desassociar empresa {EmpresaId}.", id, empresaId);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));
                }

                if (resultado == false)
                {
                    _logger.LogWarning("Falha ao desassociar empresa {EmpresaId} do usuário {UsuarioId}. Empresa pode não estar associada ou é a principal.", empresaId, id);
                    return BadRequest(ApiResponse<object>.ErrorResponse("Não foi possível remover a empresa. Ela pode não estar associada ou ser a empresa principal."));
                }

                return Ok(ApiResponse<object>.SuccessResponse("Empresa desassociada com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao desassociar empresa {EmpresaId} do usuário {UsuarioId}.", empresaId, id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao processar requisição. Tente novamente mais tarde."));
            }
        }

        [HttpPatch("{id}/empresas/{empresaId}/principal")]
        public async Task<ActionResult<ApiResponse<string>>> DefinirEmpresaPrincipal(int id, int empresaId)
        {
            try
            {
                var sucesso = await _usuarioWriterService.DefinirEmpresaPrincipalAsync(id, empresaId);

                if (!sucesso)
                {
                    _logger.LogWarning("Tentativa de definir empresa {EmpresaId} como principal para usuário {UsuarioId}, mas a empresa não está associada.", empresaId, id);
                    return BadRequest(ApiResponse<object>.ErrorResponse("Empresa não está associada a este usuário."));
                }

                return Ok(ApiResponse<string>.SuccessResponse("Empresa definida como principal."));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuário ou empresa não encontrado ao definir empresa principal. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}", id, empresaId);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao definir empresa principal para usuário {UsuarioId} e empresa {EmpresaId}.", id, empresaId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao definir empresa principal. Tente novamente mais tarde."));
            }
        }

        [HttpPut("{id}/empresas/{empresaId}/toggle")]
        public async Task<ActionResult<ApiResponse<object>>> AlternarAssociacaoEmpresa(int id, int empresaId, [FromQuery] bool? definirComoPrincipal = null, [FromQuery] int? canalPadraoId = null)
        {
            try
            {
                var resultado = await _usuarioWriterService.AlternarAssociacaoEmpresaAsync(
                    usuarioId: id,
                    empresaId: empresaId,
                    definirComoPrincipal: definirComoPrincipal,
                    canalPadraoId: canalPadraoId
                );

                return Ok(ApiResponse<object>.SuccessResponse(resultado, "Operação realizada com sucesso."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao alternar associação da empresa {EmpresaId} para usuário {UsuarioId}.", empresaId, id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entidade não encontrada ao alternar associação da empresa {EmpresaId} para usuário {UsuarioId}.", empresaId, id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao alternar associação da empresa {EmpresaId} para usuário {UsuarioId}.", empresaId, id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao alternar associação de empresa. Tente novamente mais tarde."));
            }
        }

        [HttpGet("{id}/dispositivos")]
        public async Task<ActionResult<ApiResponse<List<Dispositivo>>>> Dispositivos(int id)
        {
            try
            {
                var dispositivos = await _dispositivoReaderService.GetDispositivosByUserAsync(id);

                if (dispositivos == null || !dispositivos.Any())
                {
                    _logger.LogWarning("Nenhum dispositivo encontrado para o usuário {UsuarioId}.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Nenhum dispositivo encontrado para este usuário."));
                }

                return Ok(ApiResponse<List<Dispositivo>>.SuccessResponse(dispositivos, "Dispositivos retornados com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pesquisar dispositivos do usuário {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao pesquisar dispositivos do usuário. Tente novamente mais tarde."));
            }
        }

        [HttpPost("add-dispositivo")]
        public async Task<ActionResult<ApiResponse<string>>> AdicionarDispositivo([FromBody] AdicionarDispositivoDTO request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Dados inválidos ao tentar incluir dispositivo.");
                return BadRequest(ApiResponse<object>.ErrorResponse("Dados inválidos."));
            }

            try
            {
                await _dispositivoWriterService.Create(request);
                return Ok(ApiResponse<string>.SuccessResponse("Dispositivo incluído com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incluir dispositivo.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao incluir dispositivo. Tente novamente mais tarde."));
            }
        }

        [HttpGet("{id}/horarios")]
        public async Task<ActionResult<ApiResponse<List<UsuarioHorarioDTO>>>> ObterHorariosUsuario(int id)
        {
            try
            {
                var horarios = await _usuarioReaderService.ObterHorariosUsuarioAsync(id);

                if (horarios == null)
                {
                    _logger.LogWarning("Horários não encontrados para usuário {UsuarioId}.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));
                }

                return Ok(ApiResponse<List<UsuarioHorarioDTO>>.SuccessResponse(horarios, "Horários obtidos com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter horários do usuário {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao obter horários. Tente novamente mais tarde."));
            }
        }

        [HttpPut("{id}/horarios")]
        public async Task<ActionResult<ApiResponse<List<UsuarioHorarioDTO>>>> ConfigurarHorariosAsync(int id, [FromBody] ConfigurarHorariosRequestDTO request)
        {
            try
            {
                var horariosAtualizados = await _usuarioWriterService.ConfigurarHorariosAsync(id, request.Horarios);
                return Ok(ApiResponse<List<UsuarioHorarioDTO>>.SuccessResponse(horariosAtualizados, "Horários configurados com sucesso."));
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro de domínio ao configurar horários do usuário {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro de domínio. " + ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao configurar horários do usuário {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao configurar horários. Tente novamente mais tarde."));
            }
        }

        [HttpPut("{id}/horarios/{diaSemanaId}")]
        public async Task<ActionResult<ApiResponse<string>>> AtualizarHorarioDia(int id, int diaSemanaId, [FromBody] AtualizarHorarioTrabalhoDTO request)
        {
            try
            {
                await _usuarioWriterService.AtualizarHorarioDiaAsync(id, diaSemanaId, request);
                return Ok(ApiResponse<string>.SuccessResponse("Horário atualizado com sucesso."));
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro ao atualizar horário do dia {DiaSemanaId} para usuário {UsuarioId}.", diaSemanaId, id);

                if (ex.Message.Contains("Usuário não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));

                if (ex.Message.Contains("Dia da semana inválido", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Dia da semana inválido."));

                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao atualizar horário."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar horário do dia {DiaSemanaId} para usuário {UsuarioId}.", diaSemanaId, id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno no servidor. Tente novamente mais tarde."));
            }
        }

        [HttpPost("{id}/horarios/copiar-de/{usuarioOrigemId}")]
        public async Task<ActionResult<ApiResponse<object>>> CopiarHorariosDeUsuario(int id, int usuarioOrigemId)
        {
            if (id == usuarioOrigemId)
            {
                _logger.LogWarning("Tentativa de copiar horários para o mesmo usuário {UsuarioId}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse("Não é possível copiar horários para o mesmo usuário."));
            }

            try
            {
                var copiado = await _usuarioWriterService.CopiarHorariosDeUsuarioAsync(id, usuarioOrigemId);

                if (!copiado)
                {
                    _logger.LogWarning("Usuário origem {UsuarioOrigemId} ou destino {UsuarioDestinoId} não encontrado ao copiar horários.", usuarioOrigemId, id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário origem ou destino não encontrado."));
                }

                return Ok(ApiResponse<object>.SuccessResponse("Horários copiados com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao copiar horários do usuário {UsuarioOrigemId} para usuário {UsuarioDestinoId}.", usuarioOrigemId, id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao copiar horários. Tente novamente mais tarde."));
            }
        }

        [HttpDelete("{id}/horarios/{diaSemanaId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoverHorarioDia(int id, int diaSemanaId)
        {
            try
            {
                await _usuarioWriterService.RemoverHorarioDiaAsync(id, diaSemanaId);
                return Ok(ApiResponse<object>.SuccessResponse("Horário removido."));
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro ao remover horário do dia {DiaSemanaId} para usuário {UsuarioId}.", diaSemanaId, id);

                if (ex.Message.Contains("usuário", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));

                if (ex.Message.Contains("dia", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Dia da semana inválido."));

                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao remover horário do dia."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao remover horário do dia {DiaSemanaId} para usuário {UsuarioId}.", diaSemanaId, id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao remover horário. Tente novamente mais tarde."));
            }
        }

        [HttpPost("{id}/horarios/aplicar-padrao")]
        public async Task<ActionResult<ApiResponse<object>>> AplicarHorarioPadrao(int id, [FromQuery] string tipoPadrao = "comercial")
        {
            try
            {
                await _usuarioWriterService.AplicarHorarioPadraoAsync(id, tipoPadrao.ToLowerInvariant());
                return Ok(ApiResponse<object>.SuccessResponse("Horário padrão adicionado com sucesso."));
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro ao aplicar horário padrão '{TipoPadrao}' para usuário {UsuarioId}.", tipoPadrao, id);

                if (ex.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado."));

                if (ex.Message.Contains("inválido", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Tipo de horário padrão inválido."));

                return BadRequest(ApiResponse<object>.ErrorResponse("Erro ao aplicar horário padrão."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao aplicar horário padrão para usuário {UsuarioId}.", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao aplicar horário padrão. Tente novamente mais tarde."));
            }
        }

        [HttpGet("{empresaId}/usuarios")]
        public async Task<ActionResult<ApiResponse<List<UsuarioSimplesDTO>>>> UsuarioByEmpresa(int empresaId)
        {
            try
            {
                var filtro = new UsuarioFiltroRequestDTO
                {
                    EmpresaId = empresaId
                };

                var resultado = await _usuarioReaderService.UsuariosEmpresa(empresaId);

                if (resultado.Count == 0)
                {
                    _logger.LogWarning("Nenhum usuário encontrado para a empresa.");
                    return NotFound(ApiResponse<object>.ErrorResponse("Nenhum usuário encontrado para esta empresa."));
                }

                return Ok(ApiResponse<List<UsuarioSimplesDTO>>.SuccessResponse(resultado, "Usuários relacionados à empresa retornados com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuários relacionados à empresa {EmpresaId}.", empresaId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar usuários da empresa. Tente novamente mais tarde."));
            }
        }

        /// <summary>
        /// Lista vendedores de uma empresa para distribuição
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Lista de vendedores disponíveis para distribuição</returns>
        [HttpGet("{empresaId}/vendedores-distribuicao")]
        public async Task<ActionResult<ApiResponse<List<VendedorDistribuicaoDTO>>>> ListarVendedoresDistribuicao(int empresaId)
        {
            try
            {
                _logger.LogInformation("Listando vendedores para distribuição da empresa {EmpresaId}", empresaId);

                // Obter vendedores da empresa
                var vendedores = await _usuarioReaderService.UsuariosEmpresa(empresaId);

                // Obter dados de distribuição para cada vendedor
                var vendedoresDistribuicao = new List<VendedorDistribuicaoDTO>();

                foreach (var vendedor in vendedores)
                {
                    var posicaoFila = await _filaService.ObterPosicaoVendedorAsync(empresaId, vendedor.Id);
                    var taxaConversao = await _estatisticasService.CalcularTaxaConversaoAsync(vendedor.Id, empresaId, 30);
                    var velocidadeMedia = await _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedor.Id, empresaId, 30);

                    vendedoresDistribuicao.Add(new VendedorDistribuicaoDTO
                    {
                        VendedorId = vendedor.Id,
                        NomeVendedor = vendedor.Nome,
                        EmailVendedor = string.Empty, // TODO: Obter email do vendedor
                        AtivoDistribuicao = posicaoFila != null,
                        PosicaoFila = posicaoFila?.PosicaoFila,
                        LeadsAtivos = 0, // TODO: Implementar contagem de leads ativos
                        TaxaConversao = taxaConversao,
                        VelocidadeMediaAtendimento = velocidadeMedia,
                        ScoreAtual = 0, // TODO: Implementar cálculo de score atual
                        DataUltimaAtribuicao = null, // TODO: Implementar busca da última atribuição
                        Disponivel = posicaoFila != null && posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento == true,
                        MotivoIndisponibilidade = posicaoFila?.StatusFilaDistribuicao?.PermiteRecebimento != true ? "Indisponível" : null
                    });
                }

                return Ok(ApiResponse<List<VendedorDistribuicaoDTO>>.SuccessResponse(
                    vendedoresDistribuicao,
                    $"Vendedores listados com sucesso. Total: {vendedoresDistribuicao.Count}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar vendedores para distribuição da empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<List<VendedorDistribuicaoDTO>>.ErrorResponse(
                    "Erro ao listar vendedores para distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Configura um vendedor para distribuição
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="configuracao">Configuração do vendedor</param>
        /// <returns>Resultado da configuração</returns>
        [HttpPut("{vendedorId}/configuracao-distribuicao/{empresaId}")]
        public async Task<ActionResult<ApiResponse<VendedorDistribuicaoDTO>>> ConfigurarVendedorDistribuicao(
            int vendedorId,
            int empresaId,
            [FromBody] ConfigurarVendedorDistribuicaoDTO configuracao)
        {
            try
            {
                _logger.LogInformation("Configurando vendedor {VendedorId} para distribuição na empresa {EmpresaId}",
                    vendedorId, empresaId);

                // Verificar se o vendedor existe
                var vendedor = await _usuarioReaderService.ObterUsuarioDetalhadoAsync(vendedorId);
                if (vendedor == null)
                {
                    return NotFound(ApiResponse<VendedorDistribuicaoDTO>.ErrorResponse(
                        "Vendedor não encontrado",
                        $"Não foi encontrado nenhum vendedor com ID {vendedorId}"));
                }

                // Verificar se o vendedor pertence à empresa
                //if (!vendedor.Empresas.Any(e => e.EmpresaId == empresaId))
                //{
                //    return BadRequest(ApiResponse<VendedorDistribuicaoDTO>.ErrorResponse(
                //        "Vendedor não pertence à empresa",
                //        $"O vendedor {vendedorId} não está associado à empresa {empresaId}"));
                //}

                // Configurar vendedor na distribuição
                if (configuracao.AtivoDistribuicao)
                {
                    // Adicionar vendedor à fila se não estiver
                    await _filaService.InicializarPosicaoFilaVendedorAsync(vendedorId, empresaId);

                    // Atualizar posição se especificada
                    if (configuracao.PosicaoFila.HasValue)
                    {
                        // TODO: Implementar atualização de posição específica
                        _logger.LogInformation("Atualizando posição do vendedor {VendedorId} para {Posicao}",
                            vendedorId, configuracao.PosicaoFila.Value);
                    }
                }
                else
                {
                    // Remover vendedor da fila
                    // TODO: Implementar remoção da fila
                    _logger.LogInformation("Removendo vendedor {VendedorId} da fila de distribuição", vendedorId);
                }

                // Obter dados atualizados do vendedor
                var posicaoFila = await _filaService.ObterPosicaoVendedorAsync(empresaId, vendedorId);
                var taxaConversao = await _estatisticasService.CalcularTaxaConversaoAsync(vendedorId, empresaId, 30);
                var velocidadeMedia = await _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedorId, empresaId, 30);

                var resultado = new VendedorDistribuicaoDTO
                {
                    VendedorId = vendedorId,
                    NomeVendedor = vendedor.Nome,
                    EmailVendedor = vendedor.Email,
                    AtivoDistribuicao = configuracao.AtivoDistribuicao,
                    PosicaoFila = posicaoFila?.PosicaoFila,
                    LeadsAtivos = 0,
                    TaxaConversao = taxaConversao,
                    VelocidadeMediaAtendimento = velocidadeMedia,
                    ScoreAtual = 0,
                    DataUltimaAtribuicao = null,
                    Disponivel = posicaoFila != null && posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento == true,
                    MotivoIndisponibilidade = posicaoFila?.StatusFilaDistribuicao?.PermiteRecebimento != true ? "Indisponível" : null
                };

                return Ok(ApiResponse<VendedorDistribuicaoDTO>.SuccessResponse(
                    resultado,
                    $"Vendedor {vendedorId} configurado com sucesso para distribuição"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar vendedor {VendedorId} para distribuição na empresa {EmpresaId}",
                    vendedorId, empresaId);
                return StatusCode(500, ApiResponse<VendedorDistribuicaoDTO>.ErrorResponse(
                    "Erro ao configurar vendedor para distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Obtém a configuração de distribuição de um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Configuração de distribuição do vendedor</returns>
        [HttpGet("{vendedorId}/configuracao-distribuicao/{empresaId}")]
        public async Task<ActionResult<ApiResponse<VendedorDistribuicaoDTO>>> ObterConfiguracaoDistribuicao(
            int vendedorId,
            int empresaId)
        {
            try
            {
                _logger.LogInformation("Obtendo configuração de distribuição do vendedor {VendedorId} na empresa {EmpresaId}",
                    vendedorId, empresaId);

                // Verificar se o vendedor existe
                var vendedor = await _usuarioReaderService.ObterUsuarioDetalhadoAsync(vendedorId);
                if (vendedor == null)
                {
                    return NotFound(ApiResponse<VendedorDistribuicaoDTO>.ErrorResponse(
                        "Vendedor não encontrado",
                        $"Não foi encontrado nenhum vendedor com ID {vendedorId}"));
                }

                // Obter dados de distribuição do vendedor
                var posicaoFila = await _filaService.ObterPosicaoVendedorAsync(empresaId, vendedorId);
                var taxaConversao = await _estatisticasService.CalcularTaxaConversaoAsync(vendedorId, empresaId, 30);
                var velocidadeMedia = await _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedorId, empresaId, 30);

                var resultado = new VendedorDistribuicaoDTO
                {
                    VendedorId = vendedorId,
                    NomeVendedor = vendedor.Nome,
                    EmailVendedor = vendedor.Email,
                    AtivoDistribuicao = posicaoFila != null,
                    PosicaoFila = posicaoFila?.PosicaoFila,
                    LeadsAtivos = 0,
                    TaxaConversao = taxaConversao,
                    VelocidadeMediaAtendimento = velocidadeMedia,
                    ScoreAtual = 0,
                    DataUltimaAtribuicao = null,
                    Disponivel = posicaoFila != null && posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento == true,
                    MotivoIndisponibilidade = posicaoFila?.StatusFilaDistribuicao?.PermiteRecebimento != true ? "Indisponível" : null
                };

                return Ok(ApiResponse<VendedorDistribuicaoDTO>.SuccessResponse(
                    resultado,
                    $"Configuração de distribuição do vendedor {vendedorId} obtida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração de distribuição do vendedor {VendedorId} na empresa {EmpresaId}",
                    vendedorId, empresaId);
                return StatusCode(500, ApiResponse<VendedorDistribuicaoDTO>.ErrorResponse(
                    "Erro ao obter configuração de distribuição", ex.Message));
            }
        }

        [HttpPost("{userId:int}/horarios/tolerancia")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ToleranciaResponseDTO>>> AtualizarTolerancia(int userId, [FromBody] ToleranciaRequestDTO request)
        {
            try
            {
                var resultado = await _usuarioWriterService.DefinirToleranciaAsync(userId, request.Tolerancia);
                return Ok(ApiResponse<ToleranciaResponseDTO>.SuccessResponse(
                    resultado, "Tolerância atualizada com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tolerância para usuário {UserId}", userId);
                return StatusCode(500, ApiResponse<ToleranciaResponseDTO>.ErrorResponse("Erro ao atualizar tolerância", ex.Message));
            }
        }
    }
}
