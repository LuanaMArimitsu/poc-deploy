using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.API.Controllers.Distribuicao
{
    /// <summary>
    /// Controller para gerenciamento de regras de distribuição
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegraDistribuicaoController : ControllerBase
    {
        private readonly IRegraDistribuicaoRepository _regraRepository;
        private readonly IRegraDistribuicaoService _regraService;
        private readonly IRegraDistribuicaoProvider _provider;
        private readonly ILogger<RegraDistribuicaoController> _logger;

        /// <summary>
        /// Construtor do controller
        /// </summary>
        public RegraDistribuicaoController(
            IRegraDistribuicaoRepository regraRepository,
            IRegraDistribuicaoService regraService,
            IRegraDistribuicaoProvider provider,
            ILogger<RegraDistribuicaoController> logger)
        {
            _regraRepository = regraRepository ?? throw new ArgumentNullException(nameof(regraRepository));
            _regraService = regraService ?? throw new ArgumentNullException(nameof(regraService));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as regras de distribuição
        /// </summary>
        /// <returns>Lista de regras de distribuição</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("List")]
        public async Task<ActionResult<ApiResponse<List<RegraDistribuicaoInfoDTO>>>> List()
        {
            try
            {
                _logger.LogInformation("Obtendo todas as regras de distribuição");
                
                // Usando o método ListRegrasPorConfiguracaoAsync com ID 0 para listar todas
                var regras = await _regraRepository.ListRegrasPorConfiguracaoAsync(0, true);
                
                // Converter entidades para DTOs
                var regrasDTO = regras.Select(r => MapRegraToDTO(r)).ToList();
                
                return Ok(ApiResponse<List<RegraDistribuicaoInfoDTO>>.SuccessResponse(
                    regrasDTO, 
                    $"Regras obtidas com sucesso. Total: {regrasDTO.Count}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todas as regras de distribuição");
                return StatusCode(500, ApiResponse<List<RegraDistribuicaoInfoDTO>>.ErrorResponse(
                    "Erro ao obter regras de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Obtém uma regra de distribuição pelo seu ID
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <returns>A regra encontrada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ApiResponse<RegraDistribuicaoInfoDTO>>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Obtendo regra de distribuição por ID: {Id}", id);
                
                var regra = await _regraRepository.GetByIdAsync(id);
                
                if (regra == null)
                {
                    return NotFound(ApiResponse<RegraDistribuicaoInfoDTO>.ErrorResponse(
                        "Regra não encontrada", 
                        $"Não foi encontrada nenhuma regra com ID {id}"));
                }
                
                // Converter entidade para DTO
                var regraDTO = MapRegraToDTO(regra);
                
                return Ok(ApiResponse<RegraDistribuicaoInfoDTO>.SuccessResponse(
                    regraDTO, 
                    "Regra obtida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter regra de distribuição por ID: {Id}", id);
                return StatusCode(500, ApiResponse<RegraDistribuicaoInfoDTO>.ErrorResponse(
                    "Erro ao obter regra de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Obtém todas as regras ativas para uma configuração de distribuição
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Lista de regras ativas para a configuração</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListByConfiguracao/{configuracaoId}")]
        public async Task<ActionResult<ApiResponse<List<RegraDistribuicaoInfoDTO>>>> ListByConfiguracao(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Obtendo regras ativas para configuração ID: {ConfiguracaoId}", configuracaoId);
                
                var regras = await _regraRepository.ListRegrasAtivasPorConfiguracaoAsync(configuracaoId);
                
                // Converter entidades para DTOs
                var regrasDTO = regras.Select(r => MapRegraToDTO(r)).ToList();
                
                return Ok(ApiResponse<List<RegraDistribuicaoInfoDTO>>.SuccessResponse(
                    regrasDTO, 
                    $"Regras ativas obtidas com sucesso. Total: {regrasDTO.Count}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter regras ativas para configuração ID: {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<List<RegraDistribuicaoInfoDTO>>.ErrorResponse(
                    "Erro ao obter regras ativas para configuração", ex.Message));
            }
        }

        /// <summary>
        /// Obtém todos os tipos de regras de distribuição disponíveis
        /// </summary>
        /// <returns>Lista de tipos de regras</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListTipos")]
        public async Task<ActionResult<ApiResponse<List<TipoRegraDistribuicao>>>> ListTipos()
        {
            try
            {
                _logger.LogInformation("Obtendo tipos de regras de distribuição");
                
                // Usando o método correto conforme a interface
                var tiposRegra = await _regraRepository.ListTiposRegrasAsync();
                
                return Ok(ApiResponse<List<TipoRegraDistribuicao>>.SuccessResponse(
                    tiposRegra, 
                    "Tipos de regras obtidos com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de regras de distribuição");
                return StatusCode(500, ApiResponse<List<TipoRegraDistribuicao>>.ErrorResponse(
                    "Erro ao obter tipos de regras", ex.Message));
            }
        }

        /// <summary>
        /// Adiciona uma nova regra de distribuição
        /// </summary>
        /// <param name="regraDTO">Dados para criar a regra</param>
        /// <returns>A regra criada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AddRegra")]
        public async Task<ActionResult<ApiResponse<RegraDistribuicaoInfoDTO>>> AddRegra(
            [FromBody] NovaRegraDistribuicaoDTO regraDTO)
        {
            try
            {
                _logger.LogInformation("Adicionando nova regra de distribuição: {Nome}", regraDTO.Nome);
                
                // Validação já é feita pelos atributos [Required] na DTO
                
                // Criar nova instância da regra usando o construtor adequado
                var regra = new RegraDistribuicao(
                    configuracaoDistribuicaoId: regraDTO.ConfiguracaoDistribuicaoId,
                    tipoRegraId: regraDTO.TipoRegraId,
                    nome: regraDTO.Nome,
                    descricao: regraDTO.Descricao,
                    ordem: regraDTO.Ordem,
                    peso: regraDTO.Peso,
                    ativo: regraDTO.Ativo,
                    obrigatoria: regraDTO.Obrigatoria,
                    pontuacaoMinima: regraDTO.PontuacaoMinima,
                    pontuacaoMaxima: regraDTO.PontuacaoMaxima,
                    parametrosJson: regraDTO.ParametrosJson
                );
                
                // Usar o método CreateRegraAsync para salvar a regra
                var regraCriada = await _regraRepository.CreateRegraAsync(regra);
                
                // Converter entidade para DTO
                var regraCriadaDTO = MapRegraToDTO(regraCriada);
                
                return Created($"/api/RegraDistribuicao/GetById/{regraCriadaDTO.Id}", 
                    ApiResponse<RegraDistribuicaoInfoDTO>.SuccessResponse(
                        regraCriadaDTO, 
                        "Regra de distribuição criada com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar regra de distribuição: {Nome}", regraDTO.Nome);
                return StatusCode(500, ApiResponse<RegraDistribuicaoInfoDTO>.ErrorResponse(
                    "Erro ao adicionar regra de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Atualiza a ordem de uma regra de distribuição
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="dto">DTO com a nova ordem</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPatch("AtualizarOrdem/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> AtualizarOrdem(
            int id,
            [FromBody] AtualizarOrdemRegraDTO dto)
        {
            try
            {
                _logger.LogInformation("Atualizando ordem da regra ID: {Id} para {NovaOrdem}", id, dto.NovaOrdem);
                
                // Verificar se a regra existe
                var regra = await _regraRepository.GetByIdAsync(id);
                if (regra == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        "Regra não encontrada", 
                        $"Não foi encontrada nenhuma regra com ID {id}"));
                }
                
                // Atualizar ordem
                var resultado = await _regraRepository.AtualizarOrdemRegraAsync(id, dto.NovaOrdem);
                
                if (!resultado)
                {
                    return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                        "Erro ao atualizar ordem", 
                        "Não foi possível atualizar a ordem da regra."));
                }
                
                return Ok(ApiResponse<bool>.SuccessResponse(
                    true, 
                    "Ordem da regra atualizada com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar ordem da regra ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Erro ao atualizar ordem da regra", ex.Message));
            }
        }

        /// <summary>
        /// Ativa ou desativa uma regra de distribuição
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="dto">DTO com o novo status</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPatch("AlterarStatus/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> AlterarStatus(
            int id,
            [FromBody] AtualizarStatusRegraDTO dto)
        {
            try
            {
                _logger.LogInformation("Alterando status da regra de distribuição ID: {Id} para {Status}", 
                    id, dto.Ativar ? "Ativa" : "Inativa");
                
                // Verificar se a regra existe
                var regra = await _regraRepository.GetByIdAsync(id);
                if (regra == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        "Regra não encontrada", 
                        $"Não foi encontrada nenhuma regra com ID {id}"));
                }
                
                // Usar o método correto da interface
                var resultado = await _regraRepository.AtivarDesativarRegraAsync(id, dto.Ativar);
                
                if (!resultado)
                {
                    return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                        "Erro ao alterar status", 
                        "Não foi possível alterar o status da regra."));
                }
                
                return Ok(ApiResponse<bool>.SuccessResponse(
                    true, 
                    $"Regra {(dto.Ativar ? "ativada" : "desativada")} com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status da regra de distribuição ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Erro ao alterar status da regra", ex.Message));
            }
        }

        /// <summary>
        /// Remove uma regra de distribuição
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpDelete("DeleteRegra/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRegra(int id)
        {
            try
            {
                _logger.LogInformation("Removendo regra de distribuição ID: {Id}", id);
                
                // Verificar se a regra existe
                var regra = await _regraRepository.GetByIdAsync(id);
                if (regra == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        "Regra não encontrada", 
                        $"Não foi encontrada nenhuma regra com ID {id}"));
                }
                
                // Verificar se está sendo usada em alguma configuração
                // Verificamos pelo ID da configuração
                if (regra.ConfiguracaoDistribuicaoId > 0)
                {
                    // Obter a configuração e verificar se está ativa
                    var regrasAtivas = await _regraRepository.ListRegrasAtivasPorConfiguracaoAsync(regra.ConfiguracaoDistribuicaoId);
                    if (regrasAtivas.Count > 0 && regrasAtivas.Any(r => r.Id == id && r.Ativo))
                    {
                        return BadRequest(ApiResponse<bool>.ErrorResponse(
                            "Operação não permitida", 
                            "Não é possível remover uma regra que está associada a configurações ativas."));
                    }
                }
                
                // Remover regra
                var resultado = await _regraRepository.DeleteRegraAsync(id);
                
                if (!resultado)
                {
                    return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                        "Erro ao remover regra", 
                        "Não foi possível remover a regra especificada."));
                }
                
                return Ok(ApiResponse<bool>.SuccessResponse(
                    true, 
                    "Regra de distribuição removida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover regra de distribuição ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Erro ao remover regra de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Atualiza os parâmetros de uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="parametrosDTO">Lista de parâmetros atualizada</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPut("AtualizarParametros/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> AtualizarParametros(
            int id,
            [FromBody] List<ParametroRegraDTO> parametrosDTO)
        {
            try
            {
                _logger.LogInformation("Atualizando parâmetros da regra ID: {Id}", id);
                
                // Verificar se a regra existe
                var regra = await _regraRepository.GetByIdAsync(id);
                if (regra == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        "Regra não encontrada", 
                        $"Não foi encontrada nenhuma regra com ID {id}"));
                }
                
                // Converter DTOs para entidades
                var parametros = parametrosDTO.Select(p => new ParametroRegraDistribuicao(
                    regraDistribuicaoId: id,
                    nomeParametro: p.NomeParametro,
                    tipoParametro: p.TipoParametro,
                    valorParametro: p.ValorParametro,
                    descricao: p.Descricao,
                    obrigatorio: p.Obrigatorio,
                    validacaoRegex: p.ValidacaoRegex,
                    valorPadrao: p.ValorPadrao
                )).ToList();
                
                // Atualizar parâmetros
                var resultado = await _regraRepository.AtualizarParametrosRegraAsync(id, parametros);
                
                if (!resultado)
                {
                    return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                        "Erro ao atualizar parâmetros", 
                        "Não foi possível atualizar os parâmetros da regra."));
                }
                
                return Ok(ApiResponse<bool>.SuccessResponse(
                    true, 
                    "Parâmetros da regra atualizados com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar parâmetros da regra ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Erro ao atualizar parâmetros da regra", ex.Message));
            }
        }

        /// <summary>
        /// Mapeia uma entidade RegraDistribuicao para um DTO RegraDistribuicaoInfoDTO
        /// </summary>
        private RegraDistribuicaoInfoDTO MapRegraToDTO(RegraDistribuicao regra)
        {
            var regraDTO = new RegraDistribuicaoInfoDTO
            {
                Id = regra.Id,
                ConfiguracaoDistribuicaoId = regra.ConfiguracaoDistribuicaoId,
                NomeConfiguracao = regra.ConfiguracaoDistribuicao?.Nome ?? "Não definido",
                TipoRegraId = regra.TipoRegraId,
                NomeTipoRegra = regra.TipoRegra?.Nome ?? "Não definido",
                Nome = regra.Nome,
                Descricao = regra.Descricao,
                Ordem = regra.Ordem,
                Peso = regra.Peso,
                Ativo = regra.Ativo,
                Obrigatoria = regra.Obrigatoria,
                PontuacaoMinima = regra.PontuacaoMinima,
                PontuacaoMaxima = regra.PontuacaoMaxima,
                ParametrosJson = regra.ParametrosJson,
                DataCriacao = regra.DataCriacao,
                DataModificacao = regra.DataModificacao
            };

            // Mapear parâmetros
            if (regra.Parametros != null && regra.Parametros.Any())
            {
                regraDTO.Parametros = regra.Parametros.Select(p => new ParametroRegraDTO
                {
                    Id = p.Id,
                    RegraDistribuicaoId = p.RegraDistribuicaoId,
                    NomeParametro = p.NomeParametro,
                    TipoParametro = p.TipoParametro,
                    ValorParametro = p.ValorParametro,
                    Descricao = p.Descricao,
                    Obrigatorio = p.Obrigatorio,
                    ValidacaoRegex = p.ValidacaoRegex,
                    ValorPadrao = p.ValorPadrao
                }).ToList();
            }

            return regraDTO;
        }

        /// <summary>
        /// Valida as regras de uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Resultado da validação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ValidarRegras/{configuracaoId}")]
        public async Task<ActionResult<ApiResponse<object>>> ValidarRegras(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Validando regras da configuração {ConfiguracaoId}", configuracaoId);
                
                var validacao = await _regraService.ValidarRegrasConfiguracaoAsync(configuracaoId);
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    IsValid = validacao.IsValid,
                    TotalErros = validacao.Errors?.Count ?? 0,
                    TotalWarnings = validacao.Warnings?.Count ?? 0,
                    Erros = validacao.Errors,
                    Warnings = validacao.Warnings,
                    DataValidacao = DateTime.UtcNow
                };
                
                if (!validacao.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Configuração possui erros de validação", 
                        $"Encontrados {validacao.Errors?.Count ?? 0} erros e {validacao.Warnings?.Count ?? 0} warnings"));
                }
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    "Configuração validada com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar regras da configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao validar regras", ex.Message));
            }
        }

        /// <summary>
        /// Obtém estatísticas das regras de uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Estatísticas das regras</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("EstatisticasRegras/{configuracaoId}")]
        public async Task<ActionResult<ApiResponse<object>>> ObterEstatisticasRegras(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Obtendo estatísticas das regras da configuração {ConfiguracaoId}", configuracaoId);
                
                var estatisticas = await _regraService.ObterEstatisticasRegrasAsync(configuracaoId);
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    DataConsulta = DateTime.UtcNow,
                    Estatisticas = estatisticas
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    "Estatísticas obtidas com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas das regras da configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao obter estatísticas", ex.Message));
            }
        }

        /// <summary>
        /// Obtém tipos de regra disponíveis
        /// </summary>
        /// <returns>Lista de tipos de regra</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("TiposRegra")]
        public ActionResult<ApiResponse<object>> ObterTiposRegra()
        {
            try
            {
                _logger.LogInformation("Obtendo tipos de regra disponíveis");
                
                var tiposDisponiveis = _provider.GetAvailableRuleTypes();
                
                var resultado = new
                {
                    TotalTipos = tiposDisponiveis.Count,
                    TiposDisponiveis = tiposDisponiveis.Select(tipo => new
                    {
                        Tipo = tipo,
                        Disponivel = _provider.IsStrategyAvailable(tipo)
                    }).ToList(),
                    DataConsulta = DateTime.UtcNow
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    $"{tiposDisponiveis.Count} tipos de regra disponíveis"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de regra disponíveis");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao obter tipos de regra", ex.Message));
            }
        }

        /// <summary>
        /// Verifica se uma configuração possui regras ativas
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Status das regras</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("StatusRegras/{configuracaoId}")]
        public async Task<ActionResult<ApiResponse<object>>> VerificarStatusRegras(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Verificando status das regras da configuração {ConfiguracaoId}", configuracaoId);
                
                var possuiRegrasAtivas = await _regraService.PossuiRegrasAtivasAsync(configuracaoId);
                var totalRegrasAtivas = await _regraService.ContarRegrasAtivasAsync(configuracaoId);
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    PossuiRegrasAtivas = possuiRegrasAtivas,
                    TotalRegrasAtivas = totalRegrasAtivas,
                    StatusDistribuicao = possuiRegrasAtivas ? "Pronta" : "Configuração Incompleta",
                    DataVerificacao = DateTime.UtcNow
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    $"Status verificado: {resultado.StatusDistribuicao}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status das regras da configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao verificar status", ex.Message));
            }
        }
    }
}