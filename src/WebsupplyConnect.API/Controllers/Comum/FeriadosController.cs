using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Comum;
using WebsupplyConnect.Application.Interfaces.Comum;

namespace WebsupplyConnect.API.Controllers.Comum
{
    /// <summary>
    /// Controller para gerenciamento de feriados
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FeriadosController(IFeriadoReaderService feriadoReaderService, IFeriadoWriterService feriadoWriterService, ILogger<FeriadosController> logger) : ControllerBase
    {
        private readonly IFeriadoReaderService _feriadoReaderService = feriadoReaderService;
        private readonly IFeriadoWriterService _feriadoWriterService = feriadoWriterService;
        private readonly ILogger<FeriadosController> _logger = logger;

        /// <summary>
        /// Obtém todos os feriados
        /// </summary>
        /// <returns>Lista de feriados</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("List")]
        public async Task<ActionResult<ApiResponse<List<FeriadoDTO>>>> List()
        {
            try
            {
                _logger.LogInformation("Obtendo todos os feriados");
                var feriados = await _feriadoReaderService.ObterTodosAsync();
                return Ok(ApiResponse<List<FeriadoDTO>>.SuccessResponse(feriados, "Feriados obtidos com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os feriados");
                return StatusCode(500, ApiResponse<List<FeriadoDTO>>.ErrorResponse("Erro ao obter todos os feriados", ex.Message));
            }
        }

        /// <summary>
        /// Obtém um feriado pelo seu ID
        /// </summary>
        /// <param name="id">ID do feriado</param>
        /// <returns>O feriado encontrado</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ApiResponse<FeriadoDTO>>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Obtendo feriado por ID: {Id}", id);
                var feriado = await _feriadoReaderService.ObterPorIdAsync(id);
                return Ok(ApiResponse<FeriadoDTO>.SuccessResponse(feriado, "Feriado obtido com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao obter feriado por ID: {Id}", id);
                return NotFound(ApiResponse<FeriadoDTO>.ErrorResponse("Feriado não encontrado", ex.Message));
            }
        }

        /// <summary>
        /// Adiciona um novo feriado
        /// </summary>
        /// <param name="feriadoDTO">Dados do feriado a ser criado</param>
        /// <returns>O feriado criado</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AddFeriado")]
        public async Task<ActionResult<ApiResponse<FeriadoDTO>>> AddFeriado([FromBody] FeriadoCriarDTO feriadoDTO)
        {
            try
            {
                _logger.LogInformation("Adicionando novo feriado: {Nome}", feriadoDTO.Nome);
                var feriadoCriado = await _feriadoWriterService.AdicionarAsync(feriadoDTO);
                
                return Created($"/api/feriados/GetById/{feriadoCriado.Id}", 
                    ApiResponse<FeriadoDTO>.SuccessResponse(feriadoCriado, "Feriado criado com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao adicionar feriado: {Nome}", feriadoDTO.Nome);
                return BadRequest(ApiResponse<FeriadoDTO>.ErrorResponse("Erro ao adicionar feriado", ex.Message));
            }
        }

        /// <summary>
        /// Atualiza um feriado existente
        /// </summary>
        /// <param name="id">ID do feriado</param>
        /// <param name="feriadoDTO">Dados atualizados do feriado</param>
        /// <returns>O feriado atualizado</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPut("UpdateFeriado/{id}")]
        public async Task<ActionResult<ApiResponse<FeriadoDTO>>> UpdateFeriado(int id, [FromBody] FeriadoAtualizarDTO feriadoDTO)
        {
            try
            {
                if (id != feriadoDTO.Id)
                {
                    return BadRequest(ApiResponse<FeriadoDTO>.ErrorResponse("ID da rota diferente do ID no corpo da requisição", null));
                }

                _logger.LogInformation("Atualizando feriado ID: {Id}", id);
                var feriadoAtualizado = await _feriadoWriterService.AtualizarAsync(feriadoDTO);
                
                return Ok(ApiResponse<FeriadoDTO>.SuccessResponse(feriadoAtualizado, "Feriado atualizado com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao atualizar feriado ID: {Id}", id);
                return StatusCode(ex.Message.Contains("não encontrado") ? 404 : 400, 
                    ApiResponse<FeriadoDTO>.ErrorResponse("Erro ao atualizar feriado", ex.Message));
            }
        }

        /// <summary>
        /// Remove um feriado pelo seu ID
        /// </summary>
        /// <param name="id">ID do feriado</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpDelete("DeleteFeriado/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFeriado(int id)
        {
            try
            {
                _logger.LogInformation("Removendo feriado ID: {Id}", id);
                var resultado = await _feriadoWriterService.RemoverAsync(id);
                
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Feriado removido com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover feriado ID: {Id}", id);
                return StatusCode(ex.Message.Contains("não encontrado") ? 404 : 500, 
                    ApiResponse<object>.ErrorResponse("Erro ao remover feriado", ex.Message));
            }
        }

        /// <summary>
        /// Obtém todos os feriados para uma empresa específica
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados da empresa</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListByEmpresa/{empresaId}")]
        public async Task<ActionResult<ApiResponse<List<FeriadoDTO>>>> ListByEmpresa(int empresaId, [FromQuery] int? ano = null)
        {
            try
            {
                _logger.LogInformation("Obtendo feriados para empresa ID: {EmpresaId}, ano: {Ano}", 
                    empresaId, ano?.ToString() ?? "todos");
                
                var feriados = await _feriadoReaderService.ObterFeriadosPorEmpresaAsync(empresaId, ano);
                
                return Ok(ApiResponse<List<FeriadoDTO>>.SuccessResponse(
                    feriados, $"Feriados obtidos com sucesso para empresa {empresaId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados por empresa ID: {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<List<FeriadoDTO>>.ErrorResponse(
                    "Erro ao obter feriados por empresa", ex.Message));
            }
        }

        /// <summary>
        /// Verifica se uma data específica é feriado
        /// </summary>
        /// <param name="data">Data a ser verificada</param>
        /// <param name="empresaId">ID da empresa (opcional)</param>
        /// <param name="considerarRecorrentes">Indica se deve considerar feriados recorrentes</param>
        /// <returns>True se for feriado, False caso contrário</returns>
        [HttpGet("VerificarData")]
        public async Task<ActionResult<bool>> VerificarData(
            [FromQuery] DateTime data,
            [FromQuery] int? empresaId = null,
            [FromQuery] bool considerarRecorrentes = true)
        {
            try
            {
                var resultado = await _feriadoReaderService.VerificarDataFeriadoAsync(data, empresaId, considerarRecorrentes);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Obtém os próximos feriados a partir da data atual
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="quantidade">Quantidade de feriados a retornar</param>
        /// <returns>Lista de próximos feriados</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListProximos/{empresaId}")]
        public async Task<ActionResult<ApiResponse<List<FeriadoDTO>>>> ListProximos(
            int empresaId, 
            [FromQuery] int quantidade = 5)
        {
            try
            {
                _logger.LogInformation("Obtendo {Quantidade} próximos feriados para empresa ID: {EmpresaId}", 
                    quantidade, empresaId);
                
                var feriados = await _feriadoReaderService.ObterProximosFeriadosAsync(empresaId, quantidade);
                
                return Ok(ApiResponse<List<FeriadoDTO>>.SuccessResponse(
                    feriados, $"Próximos {quantidade} feriados obtidos com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter próximos feriados para empresa ID: {EmpresaId}", empresaId);
                
                return StatusCode(500, ApiResponse<List<FeriadoDTO>>.ErrorResponse(
                    "Erro ao obter próximos feriados", ex.Message));
            }
        }

        /// <summary>
        /// Obtém feriados por tipo
        /// </summary>
        /// <param name="tipo">Tipo de feriado (Nacional, Estadual, Municipal, Empresa)</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados do tipo especificado</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListByTipo/{tipo}")]
        public async Task<ActionResult<ApiResponse<List<FeriadoDTO>>>> ListByTipo(
            string tipo, 
            [FromQuery] int? ano = null)
        {
            try
            {
                // Validar o tipo
                if (string.IsNullOrWhiteSpace(tipo) || 
                    (!tipo.Equals("Nacional", StringComparison.OrdinalIgnoreCase) &&
                     !tipo.Equals("Estadual", StringComparison.OrdinalIgnoreCase) &&
                     !tipo.Equals("Municipal", StringComparison.OrdinalIgnoreCase) &&
                     !tipo.Equals("Empresa", StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(ApiResponse<List<FeriadoDTO>>.ErrorResponse(
                        "Tipo inválido. Os tipos válidos são: Nacional, Estadual, Municipal ou Empresa", null));
                }

                _logger.LogInformation("Obtendo feriados do tipo: {Tipo}, ano: {Ano}", 
                    tipo, ano?.ToString() ?? "todos");
                
                var feriados = await _feriadoReaderService.ObterFeriadosPorTipoAsync(tipo, ano);
                
                return Ok(ApiResponse<List<FeriadoDTO>>.SuccessResponse(
                    feriados, $"Feriados do tipo '{tipo}' obtidos com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados do tipo: {Tipo}", tipo);
                
                return StatusCode(500, ApiResponse<List<FeriadoDTO>>.ErrorResponse(
                    "Erro ao obter feriados por tipo", ex.Message));
            }
        }

        /// <summary>
        /// Obtém feriados por UF
        /// </summary>
        /// <param name="uf">Código da UF (2 caracteres)</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados do estado especificado</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListByUF/{uf}")]
        public async Task<ActionResult<ApiResponse<List<FeriadoDTO>>>> ListByUF(
            string uf, 
            [FromQuery] int? ano = null)
        {
            try
            {
                // Validar a UF
                if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
                {
                    return BadRequest(ApiResponse<List<FeriadoDTO>>.ErrorResponse(
                        "UF inválida. A UF deve ter exatamente 2 caracteres", null));
                }

                _logger.LogInformation("Obtendo feriados da UF: {UF}, ano: {Ano}", 
                    uf, ano?.ToString() ?? "todos");
                
                var feriados = await _feriadoReaderService.ObterFeriadosPorUFAsync(uf.ToUpper(), ano);
                
                return Ok(ApiResponse<List<FeriadoDTO>>.SuccessResponse(
                    feriados, $"Feriados da UF '{uf.ToUpper()}' obtidos com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados da UF: {UF}", uf);
                
                return StatusCode(500, ApiResponse<List<FeriadoDTO>>.ErrorResponse(
                    "Erro ao obter feriados por UF", ex.Message));
            }
        }
    }
}