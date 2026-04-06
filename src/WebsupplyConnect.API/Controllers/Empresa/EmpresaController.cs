using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.Interfaces.Empresa;

namespace WebsupplyConnect.API.Controllers.Empresa
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class EmpresaController(IEmpresaReaderService empresaReaderService, ILogger<EmpresaController> logger) : ControllerBase
    {
       private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
       private readonly ILogger<EmpresaController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("{id}/canal")]
        public async Task<ActionResult<ApiResponse<EmpresaComCanaisResponseDTO>>> ObterCanaisPorEmpresa(int id)
        {
            try
            {
                var resultado = await _empresaReaderService.ObterEmpresaComCanaisAsync(id);

                if (resultado == null)
                {
                    _logger.LogWarning("Empresa {EmpresaId} não encontrada ou inativa.", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Empresa não encontrada ou inativa."));
                }

                return Ok(ApiResponse<EmpresaComCanaisResponseDTO>.SuccessResponse(resultado, "Empresa e canais obtidos com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter canais da empresa {EmpresaId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar canais da empresa."));
            }
        }

        [HttpGet("canais")]
        public async Task<ActionResult<ApiResponse<List<EmpresaComCanaisResponseDTO>>>> ObterTodasEmpresasComCanais()
        {
            try
            {
                var resultado = await _empresaReaderService.ObterEmpresasComCanaisAsync();

                if (resultado == null || resultado.Count == 0)
                    return NotFound(ApiResponse<object>.ErrorResponse("Nenhuma empresa encontrada."));

                return Ok(ApiResponse<List<EmpresaComCanaisResponseDTO>>.SuccessResponse(resultado, "Empresas e canais obtidos com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar empresas e canais.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar empresas e canais."));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmpresaListagemDTO>>>> ObterTodasEmpresas()
        {
            try
            {
                var resultado = await _empresaReaderService.ObterTodasEmpresasAsync();

                if (resultado == null || resultado.Count == 0)
                    return NotFound(ApiResponse<object>.ErrorResponse("Nenhuma empresa encontrada."));

                return Ok(ApiResponse<List<EmpresaListagemDTO>>.SuccessResponse(resultado, "Empresas obtidas com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar empresas.");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao buscar empresas."));
            }
        }
    }
}
