using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Equipe;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Exceptions;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "HorarioTrabalho")]
public class EquipeController(IEquipeReaderService equipeReader, IEquipeWriterService equipeWriter, ITipoEquipeReadService tipoRead, IValidator<CriarEquipeDto> validator, ILogger<EquipeController> logger, IRoleReaderService roleReaderService) : ControllerBase
{
    private readonly IEquipeReaderService _equipeReader = equipeReader;
    private readonly IEquipeWriterService _equipeWriter = equipeWriter;
    private readonly ITipoEquipeReadService _tipoRead = tipoRead;
    private readonly IValidator<CriarEquipeDto> _validator = validator;
    private readonly ILogger<EquipeController> _logger = logger;
    private readonly IRoleReaderService _roleReaderService = roleReaderService;

    /// <summary>Cria equipe.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> CreateEquipe(CriarEquipeDto dto)
    {
        var usuarioId = _roleReaderService.ObterUsuarioId(User);

        var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "EQUIPE_CRIAR");

        if (!temPermissao)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Você não possui permissão para criar equipes nesta empresa.",
                "PERMISSAO_NEGADA"
            ));
        }
        try
        {
            var id = await _equipeWriter.CreateEquipe(dto);
            return StatusCode(201,
                ApiResponse<object>.SuccessResponse(new { id }, "Equipe criada com sucesso."));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Erro ao criar equipe");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (DomainException ex)
        {
            var status = ex.Message.Contains("já existe", StringComparison.OrdinalIgnoreCase) ? 409 : 400;
            return StatusCode(status, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar equipe");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao criar equipe."));
        }
    }

    /// <summary>Lista os tipos de equipe</summary>
    [HttpGet("tipos")]
    [ProducesResponseType(typeof(ApiResponse<List<TipoEquipeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<TipoEquipeDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<TipoEquipeDto>>>> ListarTiposFixos()
    {
        try
        {
            var tipos = await _tipoRead.GetTiposFixosAsync();
            return Ok(ApiResponse<List<TipoEquipeDto>>
                .SuccessResponse(tipos, "Tipos de equipe retornados com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tipos de equipe fixos.");
            return StatusCode(500, ApiResponse<List<TipoEquipeDto>>
                .ErrorResponse("Erro interno ao listar tipos de equipe.", ex.ToString()));
        }
    }

    /// <summary>Lista equipes de uma empresa com filtros e paginação.</summary>
    [HttpPost("listar")]
    [ProducesResponseType(typeof(ApiResponse<EquipePaginadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipePaginadoDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EquipePaginadoDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EquipePaginadoDto>>> ListarPorEmpresa([FromBody] EquipeFiltroRequestDto filtro)
    {
        try
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, filtro.EmpresaId, "EQUIPE_VISUALIZAR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para visualizar equipes nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }

            var resultado = await _equipeReader.ListarPorEmpresaAsync(filtro);
            return Ok(ApiResponse<EquipePaginadoDto>.SuccessResponse(resultado));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<EquipePaginadoDto>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar equipes");
            return StatusCode(500, ApiResponse<EquipePaginadoDto>.ErrorResponse("Erro interno ao listar equipes.", ex.ToString()));
        }
    }

    /// <summary>Lista equipes a partir do ID delas.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ListDetalheEquipeDto>>> GetById(int id)
    {
        try
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var dto = await _equipeReader.GetByIdAsync(id);

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "EQUIPE_VISUALIZAR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para visualizar esta equipe.",
                    "PERMISSAO_NEGADA"
                ));
            }

            return Ok(ApiResponse<ListDetalheEquipeDto>.SuccessResponse(dto));
        }
        catch (NotFoundAppException ex)
        {
            return NotFound(ApiResponse<ListDetalheEquipeDto>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<ListDetalheEquipeDto>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter detalhes da equipe {EquipeId}", id);
            return StatusCode(500, ApiResponse<ListDetalheEquipeDto>.ErrorResponse("Erro interno ao obter equipe.", ex.ToString()));
        }

    }

    /// <summary>Atualiza parcialmente uma equipe (nome, status e configurações de notificações).</summary>
    [HttpPatch("editar/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEquipe(int id, [FromBody] AtualizarEquipeDto dto)
    {
        try
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);
            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, dto.EmpresaId, "EQUIPE_EDITAR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para editar equipes nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }

            await _equipeWriter.UpdateEquipeAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Equipe atualizada com sucesso."));
        }
        catch (DomainException ex)
        {
            var status =
                ex.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase) ? 404 :
                ex.Message.Contains("já existe", StringComparison.OrdinalIgnoreCase) ? 409 : 400;

            return StatusCode(status, ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar equipe {EquipeId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao atualizar equipe.", ex.ToString()));
        }
    }

    /// <summary>Exclusão lógica da equipe (somente se não houver membros ativos).</summary>
    [HttpDelete("excluir/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEquipe(int id, int empresaId)
    {
        try
        {
            var usuarioId = _roleReaderService.ObterUsuarioId(User);

            var temPermissao = await _roleReaderService.UsuarioTemPermissaoAsync(usuarioId, empresaId, "EQUIPE_EXCLUIR");

            if (!temPermissao)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Você não possui permissão para excluir equipes nesta empresa.",
                    "PERMISSAO_NEGADA"
                ));
            }

            await _equipeWriter.DeleteEquipeAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Equipe excluída com sucesso."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir equipe {EquipeId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao excluir equipe.", ex.ToString()));
        }
    }

    /// <summary>Lista os responsáveis das equipes de uma empresa.</summary>
    [HttpGet("responsaveis/{empresaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<ResponsaveisPorEmpresaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<ResponsaveisPorEmpresaDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<List<ResponsaveisPorEmpresaDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<List<ResponsaveisPorEmpresaDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ResponsaveisPorEmpresaDto>>>> ListarResponsaveisPorEmpresa(int empresaId)
    {
        try
        {
            if (empresaId <= 0)
                return BadRequest(ApiResponse<List<ResponsaveisPorEmpresaDto>>
                    .ErrorResponse("O parâmetro EmpresaId deve ser maior que zero."));

            var result = await _equipeReader.ListarResponsaveisPorEmpresaAsync(empresaId);

            if (result == null || result.Count == 0)
                return NotFound(ApiResponse<List<ResponsaveisPorEmpresaDto>>
                    .ErrorResponse("Nenhuma equipe com responsável encontrada para a empresa informada."));

            return Ok(ApiResponse<List<ResponsaveisPorEmpresaDto>>
                .SuccessResponse(result, "Responsáveis retornados com sucesso."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<List<ResponsaveisPorEmpresaDto>>
                .ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar responsáveis da empresa {EmpresaId}", empresaId);
            return StatusCode(500, ApiResponse<List<ResponsaveisPorEmpresaDto>>
                .ErrorResponse("Erro interno ao listar responsáveis da empresa.", ex.ToString()));
        }
    }

    /// <summary>Lista simples de equipes com membros de uma empresa, necessaria para criação/transferencia de lead.</summary>
    [HttpPost("listaSimples")]
    public async Task<ActionResult<ApiResponse<List<EquipeSimplesDto>>>> ListaSimplesPorEmpresa(int empresaId)
    {
        try
        {
            var resultado = await _equipeReader.ListaSimplesPorEmpresaAsync(empresaId);
            return Ok(ApiResponse<List<EquipeSimplesDto>>.SuccessResponse(resultado));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<EquipePaginadoDto>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar equipes");
            return StatusCode(500, ApiResponse<EquipePaginadoDto>.ErrorResponse("Erro interno ao listar equipes.", ex.ToString()));
        }
    }

    [HttpGet("responsavel/{usuarioId}/membros")]
    public async Task<ActionResult<ApiResponse<List<ListMembroEEquipeDTO>>>> ListarMembrosEEquipesByResponsavel(int usuarioId)
    {
        try
        {
            var resultado = await _equipeReader.ListarMembrosEEquipesByResponsavelAsync(usuarioId);
            return Ok(ApiResponse<List<ListMembroEEquipeDTO>>.SuccessResponse(resultado));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<List<ListMembroEEquipeDTO>>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar membros e equipes do responsável");
            return StatusCode(500, ApiResponse<List<ListMembroEEquipeDTO>>.ErrorResponse("Erro interno ao listar membros e equipes do responsável.", ex.ToString()));
        }
    }

    /// <summary>Lista simples de equipes de uma empresa, para seleção de equipe padrão.</summary>
    [HttpGet("empresa/{empresaId:int}")]
    public async Task<ActionResult<ApiResponse<List<EquipeListagemSimplesDto>>>> ListarPorEmpresaId(int empresaId)
    {
        try
        {
            var resultado = await _equipeReader.ListarSimplesPorEmpresaIdAsync(empresaId);
            return Ok(ApiResponse<List<EquipeListagemSimplesDto>>.SuccessResponse(resultado));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<List<EquipeListagemSimplesDto>>.ErrorResponse(ex.Message, ex.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar equipes da empresa {EmpresaId}", empresaId);
            return StatusCode(500, ApiResponse<List<EquipeListagemSimplesDto>>.ErrorResponse("Erro interno ao listar equipes.", ex.ToString()));
        }
    }
}
