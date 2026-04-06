using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.API.Controllers.Distribuicao
{
    /// <summary>
    /// Controller para gerenciamento de configurações de distribuição
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracaoDistribuicaoController : ControllerBase
    {
        private readonly IConfiguracaoDistribuicaoRepository _configuracaoRepository;
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly IVendedorEstatisticasService _estatisticasService;
        private readonly IFilaDistribuicaoService _filaService;
        private readonly IHorariosDistribuicaoService _horariosService;
        private readonly ILogger<ConfiguracaoDistribuicaoController> _logger;

        /// <summary>
        /// Construtor do controller
        /// </summary>
        public ConfiguracaoDistribuicaoController(
            IConfiguracaoDistribuicaoRepository configuracaoRepository,
            IUsuarioReaderService usuarioReaderService,
            IVendedorEstatisticasService estatisticasService,
            IFilaDistribuicaoService filaService,
            IHorariosDistribuicaoService horariosService,
            ILogger<ConfiguracaoDistribuicaoController> logger)
        {
            _configuracaoRepository = configuracaoRepository ?? throw new ArgumentNullException(nameof(configuracaoRepository));
            _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
            _estatisticasService = estatisticasService ?? throw new ArgumentNullException(nameof(estatisticasService));
            _filaService = filaService ?? throw new ArgumentNullException(nameof(filaService));
            _horariosService = horariosService ?? throw new ArgumentNullException(nameof(horariosService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as configurações de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Lista de configurações de distribuição</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("ListByEmpresa/{empresaId}")]
        public async Task<ActionResult<ApiResponse<List<ConfiguracaoDistribuicaoListagemDTO>>>> ListByEmpresa(int empresaId)
        {
            try
            {
                _logger.LogInformation("Obtendo configurações de distribuição para empresa {EmpresaId}", empresaId);
                
                var configuracoes = await _configuracaoRepository.ListConfiguracoesAsync(empresaId);
                
                var configuracoesDTO = configuracoes.Select(c => new ConfiguracaoDistribuicaoListagemDTO
                {
                    Id = c.Id,
                    EmpresaId = c.EmpresaId,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    Ativo = c.Ativo,
                    DataInicioVigencia = c.DataInicioVigencia,
                    DataFimVigencia = c.DataFimVigencia,
                    PermiteAtribuicaoManual = c.PermiteAtribuicaoManual,
                    MaxLeadsAtivosVendedor = c.MaxLeadsAtivosVendedor,
                    ConsiderarHorarioTrabalho = c.ConsiderarHorarioTrabalho,
                    ConsiderarFeriados = c.ConsiderarFeriados,
                    ParametrosGerais = c.ParametrosGerais,
                    DataCriacao = c.DataCriacao,
                    DataModificacao = c.DataModificacao,
                    TotalRegras = c.Regras?.Count ?? 0,
                    RegrasAtivas = c.Regras?.Count(r => r.Ativo) ?? 0
                }).ToList();
                
                return Ok(ApiResponse<List<ConfiguracaoDistribuicaoListagemDTO>>.SuccessResponse(
                    configuracoesDTO, 
                    $"Configurações obtidas com sucesso. Total: {configuracoesDTO.Count}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configurações de distribuição para empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<List<ConfiguracaoDistribuicaoListagemDTO>>.ErrorResponse(
                    "Erro ao obter configurações de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Obtém uma configuração de distribuição pelo seu ID
        /// </summary>
        /// <param name="id">ID da configuração</param>
        /// <returns>A configuração encontrada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoDistribuicao>>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Obtendo configuração de distribuição por ID: {Id}", id);
                
                var configuracao = await _configuracaoRepository.GetByIdAsync(id);
                
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {id}"));
                }
                
                return Ok(ApiResponse<ConfiguracaoDistribuicao>.SuccessResponse(
                    configuracao, 
                    "Configuração obtida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração de distribuição por ID: {Id}", id);
                return StatusCode(500, ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                    "Erro ao obter configuração de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Obtém a configuração ativa para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>A configuração ativa</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("GetConfiguracaoAtiva/{empresaId}")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoDistribuicao>>> GetConfiguracaoAtiva(int empresaId)
        {
            try
            {
                _logger.LogInformation("Obtendo configuração ativa para empresa {EmpresaId}", empresaId);
                
                var configuracao = await _configuracaoRepository.GetConfiguracaoAtivaAsync(empresaId);
                
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                        "Configuração ativa não encontrada", 
                        $"Não foi encontrada nenhuma configuração ativa para a empresa {empresaId}"));
                }
                
                return Ok(ApiResponse<ConfiguracaoDistribuicao>.SuccessResponse(
                    configuracao, 
                    "Configuração ativa obtida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração ativa para empresa {EmpresaId}", empresaId);
                return StatusCode(500, ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                    "Erro ao obter configuração ativa", ex.Message));
            }
        }

        /// <summary>
        /// Adiciona uma nova configuração de distribuição
        /// </summary>
        /// <param name="configuracaoDTO">Dados da configuração a ser criada</param>
        /// <returns>A configuração criada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("AddConfiguracao")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoDistribuicao>>> AddConfiguracao(
            [FromBody] ConfiguracaoDistribuicaoCriarDTO configuracaoDTO)
        {
            try
            {
                _logger.LogInformation("Adicionando nova configuração de distribuição para empresa {EmpresaId}: {Nome}", 
                    configuracaoDTO.EmpresaId, configuracaoDTO.Nome);
                
                // Obter ID do usuário logado
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int usuarioCriouId))
                {
                    return BadRequest(ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                        "Erro de autenticação", 
                        "Não foi possível identificar o usuário que está criando a configuração"));
                }
                
                // Criar a configuração usando o método de fábrica
                var configuracao = ConfiguracaoDistribuicao.Criar(
                    empresaId: configuracaoDTO.EmpresaId,
                    nome: configuracaoDTO.Nome,
                    descricao: configuracaoDTO.Descricao,
                    ativo: configuracaoDTO.Ativa,
                    dataInicioVigencia: configuracaoDTO.DataInicioVigencia,
                    dataFimVigencia: configuracaoDTO.DataFimVigencia,
                    maxLeadsAtivosVendedor: configuracaoDTO.MaxLeadsAtivosPorVendedor,
                    considerarHorarioTrabalho: true, // SIMPLIFICADO: sempre considerar horário de trabalho
                    considerarFeriados: true,
                    permiteAtribuicaoManual: true,
                    // Criar um JSON para armazenar os parâmetros adicionais (SIMPLIFICADO)
                    parametrosGerais: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        UsuarioCriouId = usuarioCriouId
                    })
                );
                
                var configuracaoCriada = await _configuracaoRepository.CreateAsync(configuracao);
                
                // Se a nova configuração está ativa, desativar outras da mesma empresa
                if (configuracaoCriada.Ativo)
                {
                    await _configuracaoRepository.DesativarOutrasConfiguracoesAsync(
                        configuracaoCriada.EmpresaId, configuracaoCriada.Id);
                    
                    _logger.LogInformation("Desativadas outras configurações da empresa {EmpresaId} para manter apenas {ConfiguracaoId} ativa", 
                        configuracaoCriada.EmpresaId, configuracaoCriada.Id);
                }
                
                // Adicionar regras associadas
                if (configuracaoDTO.RegrasDistribuicaoIds?.Count > 0)
                {
                    await _configuracaoRepository.AssociarRegrasAsync(
                        configuracaoCriada.Id, configuracaoDTO.RegrasDistribuicaoIds);
                }
                
                return Created($"/api/ConfiguracaoDistribuicao/GetById/{configuracaoCriada.Id}", 
                    ApiResponse<ConfiguracaoDistribuicao>.SuccessResponse(
                        configuracaoCriada, 
                        "Configuração de distribuição criada com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar configuração de distribuição: {Nome}", 
                    configuracaoDTO.Nome);
                    
                return StatusCode(500, ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                    "Erro ao adicionar configuração de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Atualiza uma configuração de distribuição existente
        /// </summary>
        /// <param name="id">ID da configuração</param>
        /// <param name="configuracaoDTO">Dados atualizados da configuração</param>
        /// <returns>A configuração atualizada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPut("UpdateConfiguracao/{id}")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoDistribuicao>>> UpdateConfiguracao(
            int id,
            [FromBody] ConfiguracaoDistribuicaoAtualizarDTO configuracaoDTO)
        {
            try
            {
                if (id != configuracaoDTO.Id)
                {
                    return BadRequest(ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                        "ID da rota diferente do ID no corpo da requisição", null));
                }
                
                _logger.LogInformation("Atualizando configuração de distribuição ID: {Id}", id);
                
                // Verificar se a configuração existe
                var configuracaoExistente = await _configuracaoRepository.GetByIdAsync(id);
                if (configuracaoExistente == null)
                {
                    return NotFound(ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {id}"));
                }
                
                // Atualizar propriedades usando método do domínio
                configuracaoExistente.Atualizar(
                    nome: configuracaoDTO.Nome,
                    descricao: configuracaoDTO.Descricao,
                    ativo: configuracaoDTO.Ativa,
                    dataInicioVigencia: configuracaoDTO.DataInicioVigencia,
                    dataFimVigencia: configuracaoDTO.DataFimVigencia,
                    maxLeadsAtivosVendedor: configuracaoDTO.MaxLeadsAtivosPorVendedor,
                    considerarHorarioTrabalho: true // SIMPLIFICADO: sempre considerar horário de trabalho
                );
                
                // Parâmetros extras simplificados (não há mais campos complexos)
                
                var configuracaoAtualizada = await _configuracaoRepository.UpdateAsync(configuracaoExistente);
                
                // Se a configuração foi ativada, desativar outras da mesma empresa
                if (configuracaoAtualizada.Ativo)
                {
                    await _configuracaoRepository.DesativarOutrasConfiguracoesAsync(
                        configuracaoAtualizada.EmpresaId, configuracaoAtualizada.Id);
                    
                    _logger.LogInformation("Desativadas outras configurações da empresa {EmpresaId} para manter apenas {ConfiguracaoId} ativa", 
                        configuracaoAtualizada.EmpresaId, configuracaoAtualizada.Id);
                }
                
                // Atualizar regras associadas
                if (configuracaoDTO.RegrasDistribuicaoIds != null)
                {
                    await _configuracaoRepository.AtualizarRegrasAsync(
                        configuracaoAtualizada.Id, configuracaoDTO.RegrasDistribuicaoIds);
                }
                
                return Ok(ApiResponse<ConfiguracaoDistribuicao>.SuccessResponse(
                    configuracaoAtualizada, 
                    "Configuração de distribuição atualizada com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar configuração de distribuição ID: {Id}", id);
                return StatusCode(500, ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                    "Erro ao atualizar configuração de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Valida a configuração de horários
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidarConfiguracaoHorarios(ConfigurarHorariosDistribuicaoDTO configuracaoHorarios)
        {
            // Validar horários de expediente se fornecidos
            if (!string.IsNullOrEmpty(configuracaoHorarios.HorarioInicioExpediente) && 
                !string.IsNullOrEmpty(configuracaoHorarios.HorarioFimExpediente))
            {
                if (!TimeSpan.TryParse(configuracaoHorarios.HorarioInicioExpediente, out var inicio) ||
                    !TimeSpan.TryParse(configuracaoHorarios.HorarioFimExpediente, out var fim))
                {
                    return (false, "Horários de expediente devem estar no formato HH:mm");
                }
                
                if (inicio >= fim)
                {
                    return (false, "Horário de fim do expediente deve ser maior que o horário de início");
                }
            }
            
            // Validar horários por dia
            if (configuracaoHorarios.HorariosPorDia != null)
            {
                foreach (var horarioDia in configuracaoHorarios.HorariosPorDia)
                {
                    if (horarioDia.TrabalhaNesteDia)
                    {
                        if (string.IsNullOrEmpty(horarioDia.HorarioInicio) || string.IsNullOrEmpty(horarioDia.HorarioFim))
                        {
                            return (false, $"Horários de início e fim são obrigatórios para o dia {horarioDia.DiaSemanaId}");
                        }
                        
                        if (!TimeSpan.TryParse(horarioDia.HorarioInicio, out var inicioDia) ||
                            !TimeSpan.TryParse(horarioDia.HorarioFim, out var fimDia))
                        {
                            return (false, $"Horários do dia {horarioDia.DiaSemanaId} devem estar no formato HH:mm");
                        }
                        
                        if (inicioDia >= fimDia)
                        {
                            return (false, $"Horário de fim deve ser maior que o início para o dia {horarioDia.DiaSemanaId}");
                        }
                        
                        // Validar intervalo de almoço
                        if (horarioDia.IntervaloAlmoco.HasValue)
                        {
                            if (horarioDia.IntervaloAlmoco.Value < 0 || horarioDia.IntervaloAlmoco.Value > 240)
                            {
                                return (false, $"Intervalo de almoço deve estar entre 0 e 240 minutos para o dia {horarioDia.DiaSemanaId}");
                            }
                            
                            // Verificar se o intervalo de almoço não excede o tempo de trabalho
                            var tempoTrabalho = fimDia - inicioDia;
                            if (horarioDia.IntervaloAlmoco.Value >= tempoTrabalho.TotalMinutes)
                            {
                                return (false, $"Intervalo de almoço não pode ser maior que o tempo de trabalho para o dia {horarioDia.DiaSemanaId}");
                            }
                        }
                    }
                }
            }
            
            // Validar fuso horário se fornecido
            if (!string.IsNullOrEmpty(configuracaoHorarios.FusoHorario))
            {
                try
                {
                    var fusoHorario = TimeZoneInfo.FindSystemTimeZoneById(configuracaoHorarios.FusoHorario);
                    if (fusoHorario == null)
                    {
                        return (false, "Fuso horário inválido");
                    }
                }
                catch
                {
                    return (false, "Fuso horário inválido");
                }
            }
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Método auxiliar para atualizar parâmetros extras da configuração
        /// </summary>
        private void AtualizarParametrosExtras(ConfiguracaoDistribuicao configuracao, object parametrosExtras)
        {
            try
            {
                // Obter a propriedade ParametrosGerais usando reflexão
                var tipo = typeof(ConfiguracaoDistribuicao);
                var propriedadeParametros = tipo.GetProperty("ParametrosGerais");
                
                if (propriedadeParametros != null && propriedadeParametros.CanRead)
                {
                    // Ler o valor atual
                    var parametrosAtuais = propriedadeParametros.GetValue(configuracao) as string ?? "{}";
                    
                    // Desserializar em um dicionário
                    var parametrosDict = new System.Collections.Generic.Dictionary<string, object>();
                    var doc = System.Text.Json.JsonDocument.Parse(parametrosAtuais);
                    
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        // Convertendo o JsonElement para o tipo apropriado
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                            parametrosDict[prop.Name] = prop.Value.GetString() ?? "";
                        else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                            parametrosDict[prop.Name] = prop.Value.GetDouble();
                        else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.True || 
                                prop.Value.ValueKind == System.Text.Json.JsonValueKind.False)
                            parametrosDict[prop.Name] = prop.Value.GetBoolean();
                        else
                            parametrosDict[prop.Name] = prop.Value.ToString() ?? "";
                    }
                    
                    // Atualizar com os novos valores
                    var parametrosExtrasDict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(
                        System.Text.Json.JsonSerializer.Serialize(parametrosExtras));
                        
                    if (parametrosExtrasDict != null)
                    {
                        foreach (var item in parametrosExtrasDict)
                        {
                            parametrosDict[item.Key] = item.Value;
                        }
                    }
                    
                    // Serializar de volta para JSON
                    var novoJson = System.Text.Json.JsonSerializer.Serialize(parametrosDict);
                    
                    // Atualizar a propriedade se for possível escrever
                    if (propriedadeParametros.CanWrite)
                    {
                        propriedadeParametros.SetValue(configuracao, novoJson);
                        
                        // Chamar manualmente o método para atualizar a data de modificação
                        var metodoAtualizarData = tipo.GetMethod("AtualizarDataModificacao", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        metodoAtualizarData?.Invoke(configuracao, null);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro, mas não propaga a exceção para não afetar o fluxo principal
                _logger.LogError(ex, "Erro ao atualizar parâmetros extras da configuração");
            }
        }

        /// <summary>
        /// Ativa ou desativa uma configuração de distribuição
        /// </summary>
        /// <param name="id">ID da configuração</param>
        /// <param name="ativar">Indica se deve ativar (true) ou desativar (false)</param>
        /// <returns>A configuração atualizada</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPatch("AlterarStatus/{id}")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoDistribuicao>>> AlterarStatus(
            int id,
            [FromQuery] bool ativar)
        {
            try
            {
                _logger.LogInformation("Alterando status da configuração de distribuição ID: {Id} para {Status}", 
                    id, ativar ? "Ativa" : "Inativa");
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(id);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {id}"));
                }
                
                // Se estiver ativando, usar o método específico que desativa as outras
                if (ativar)
                {
                    var resultado = await _configuracaoRepository.AtivarConfiguracaoAsync(id);
                    if (!resultado)
                    {
                        return StatusCode(500, ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                            "Erro ao ativar configuração", 
                            "Não foi possível ativar a configuração. Verifique os logs para mais detalhes."));
                    }
                    
                    // Recarregar a configuração atualizada
                    configuracao = await _configuracaoRepository.GetByIdAsync(id);
                }
                else
                {
                    // Se estiver desativando, apenas atualizar a configuração
                    configuracao.Desativar();
                    await _configuracaoRepository.UpdateAsync(configuracao);
                }
                
                return Ok(ApiResponse<ConfiguracaoDistribuicao>.SuccessResponse(
                    configuracao!, 
                    $"Configuração {(ativar ? "ativada" : "desativada")} com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status da configuração de distribuição ID: {Id}", id);
                return StatusCode(500, ApiResponse<ConfiguracaoDistribuicao>.ErrorResponse(
                    "Erro ao alterar status da configuração", ex.Message));
            }
        }

        /// <summary>
        /// Remove uma configuração de distribuição
        /// </summary>
        /// <param name="id">ID da configuração</param>
        /// <returns>Resultado da operação</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpDelete("DeleteConfiguracao/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteConfiguracao(int id)
        {
            try
            {
                _logger.LogInformation("Removendo configuração de distribuição ID: {Id}", id);
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(id);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {id}"));
                }
                
                // Verificar se é a configuração ativa atual
                if (configuracao.Ativo)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Operação não permitida", 
                        "Não é possível remover uma configuração ativa. Desative-a primeiro."));
                }
                
                // Verificar se há histórico de distribuição associado
                if (await _configuracaoRepository.TemHistoricoDistribuicaoAsync(id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Operação não permitida", 
                        "Não é possível remover uma configuração que já foi utilizada em distribuições."));
                }
                
                // Remover configuração
                await _configuracaoRepository.DeleteAsync(id);
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { }, 
                    "Configuração de distribuição removida com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover configuração de distribuição ID: {Id}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao remover configuração de distribuição", ex.Message));
            }
        }

        /// <summary>
        /// Obtém os vendedores configurados para uma configuração de distribuição
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Lista de vendedores configurados</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("{configuracaoId}/vendedores")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoVendedoresResponseDTO>>> ObterVendedoresConfiguracao(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Obtendo vendedores da configuração {ConfiguracaoId}", configuracaoId);
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<ConfiguracaoVendedoresResponseDTO>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {configuracaoId}"));
                }
                
                // Obter vendedores que estão na fila de distribuição
                var vendedoresDistribuicao = new List<VendedorDistribuicaoDTO>();
                
                // Buscar vendedores na fila para esta empresa
                var vendedoresNaFila = await _filaService.ObterVendedoresNaFilaAsync(configuracao.EmpresaId);
                
                _logger.LogDebug("Encontrados {Count} vendedores na fila para empresa {EmpresaId}", 
                    vendedoresNaFila.Count, configuracao.EmpresaId);
                
                foreach (var posicaoFila in vendedoresNaFila)
                {
                    // Obter dados do usuário
                    var vendedor = await _usuarioReaderService.ObterUsuarioDetalhadoAsync(posicaoFila.MembroEquipeId);
                    if (vendedor == null) continue;
                    
                    // Verificar se o vendedor está ativo
                    if (!vendedor.Ativo)
                    {
                        _logger.LogDebug("Vendedor {VendedorId} não está ativo, pulando", vendedor.Id);
                        continue;
                    }
                    
                    var taxaConversao = await _estatisticasService.CalcularTaxaConversaoAsync(vendedor.Id, configuracao.EmpresaId, 30);
                    var velocidadeMedia = await _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(vendedor.Id, configuracao.EmpresaId, 30);
                    
                    vendedoresDistribuicao.Add(new VendedorDistribuicaoDTO
                    {
                        VendedorId = vendedor.Id,
                        NomeVendedor = vendedor.Nome,
                        EmailVendedor = vendedor.Email,
                        AtivoDistribuicao = true, // Se está na fila e ativo, está ativo na distribuição
                        PosicaoFila = posicaoFila.PosicaoFila,
                        LeadsAtivos = 0, // TODO: Implementar contagem de leads ativos
                        TaxaConversao = taxaConversao,
                        VelocidadeMediaAtendimento = velocidadeMedia,
                        ScoreAtual = 0, // TODO: Implementar cálculo de score atual
                        DataUltimaAtribuicao = null, // TODO: Implementar busca da última atribuição
                        Disponivel = posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento == true,
                        MotivoIndisponibilidade = posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento != true ? "Indisponível" : null
                    });
                }
                
                var resultado = new ConfiguracaoVendedoresResponseDTO
                {
                    ConfiguracaoDistribuicaoId = configuracaoId,
                    TotalVendedores = vendedoresDistribuicao.Count,
                    VendedoresAtivos = vendedoresDistribuicao.Count(v => v.AtivoDistribuicao),
                    VendedoresInativos = vendedoresDistribuicao.Count(v => !v.AtivoDistribuicao),
                    Vendedores = vendedoresDistribuicao,
                    DataConfiguracao = DateTime.UtcNow
                };
                
                _logger.LogInformation("Retornando {Count} vendedores para configuração {ConfiguracaoId}", 
                    resultado.TotalVendedores, configuracaoId);
                
                return Ok(ApiResponse<ConfiguracaoVendedoresResponseDTO>.SuccessResponse(
                    resultado, 
                    $"Vendedores obtidos com sucesso. Total: {resultado.TotalVendedores}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter vendedores da configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<ConfiguracaoVendedoresResponseDTO>.ErrorResponse(
                    "Erro ao obter vendedores da configuração", ex.Message));
            }
        }

        /// <summary>
        /// Configura vendedores para uma configuração de distribuição
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="vendedores">Lista de vendedores a configurar</param>
        /// <returns>Resultado da configuração</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPost("{configuracaoId}/vendedores")]
        public async Task<ActionResult<ApiResponse<ConfiguracaoVendedoresResponseDTO>>> ConfigurarVendedores(
            int configuracaoId,
            [FromBody] List<ConfigurarVendedorDistribuicaoDTO> vendedores)
        {
            try
            {
                _logger.LogInformation("Configurando {Count} vendedores para configuração {ConfiguracaoId}", 
                    vendedores?.Count ?? 0, configuracaoId);
                
                if (vendedores == null || !vendedores.Any())
                {
                    return BadRequest(ApiResponse<ConfiguracaoVendedoresResponseDTO>.ErrorResponse(
                        "Dados inválidos", 
                        "É necessário fornecer pelo menos um vendedor para configuração"));
                }
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<ConfiguracaoVendedoresResponseDTO>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {configuracaoId}"));
                }
                
                // Configurar cada vendedor
                var vendedoresConfigurados = new List<VendedorDistribuicaoDTO>();
                
                foreach (var configVendedor in vendedores)
                {
                    if (configVendedor.AtivoDistribuicao)
                    {
                        // Adicionar vendedor à fila se não estiver
                        await _filaService.InicializarPosicaoFilaVendedorAsync(configVendedor.VendedorId, configuracao.EmpresaId);
                        
                        // Atualizar posição se especificada
                        if (configVendedor.PosicaoFila.HasValue)
                        {
                            // TODO: Implementar atualização de posição específica
                            _logger.LogInformation("Atualizando posição do vendedor {VendedorId} para {Posicao}", 
                                configVendedor.VendedorId, configVendedor.PosicaoFila.Value);
                        }
                    }
                    else
                    {
                        // Remover vendedor da fila
                        // TODO: Implementar remoção da fila
                        _logger.LogInformation("Removendo vendedor {VendedorId} da fila de distribuição", 
                            configVendedor.VendedorId);
                    }
                    
                    // Obter dados atualizados do vendedor
                    var vendedor = await _usuarioReaderService.ObterUsuarioDetalhadoAsync(configVendedor.VendedorId);
                    if (vendedor != null)
                    {
                        var posicaoFila = await _filaService.ObterPosicaoVendedorAsync(configuracao.EmpresaId, configVendedor.VendedorId);
                        var taxaConversao = await _estatisticasService.CalcularTaxaConversaoAsync(configVendedor.VendedorId, configuracao.EmpresaId, 30);
                        var velocidadeMedia = await _estatisticasService.CalcularVelocidadeMediaAtendimentoAsync(configVendedor.VendedorId, configuracao.EmpresaId, 30);
                        
                        vendedoresConfigurados.Add(new VendedorDistribuicaoDTO
                        {
                            VendedorId = configVendedor.VendedorId,
                            NomeVendedor = vendedor.Nome,
                            EmailVendedor = vendedor.Email,
                            AtivoDistribuicao = configVendedor.AtivoDistribuicao,
                            PosicaoFila = posicaoFila?.PosicaoFila,
                            LeadsAtivos = 0,
                            TaxaConversao = taxaConversao,
                            VelocidadeMediaAtendimento = velocidadeMedia,
                            ScoreAtual = 0,
                            DataUltimaAtribuicao = null,
                                                    Disponivel = posicaoFila != null && posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento == true,
                        MotivoIndisponibilidade = posicaoFila?.StatusFilaDistribuicao?.PermiteRecebimento != true ? "Indisponível" : null
                        });
                    }
                }
                
                var resultado = new ConfiguracaoVendedoresResponseDTO
                {
                    ConfiguracaoDistribuicaoId = configuracaoId,
                    TotalVendedores = vendedoresConfigurados.Count,
                    VendedoresAtivos = vendedoresConfigurados.Count(v => v.AtivoDistribuicao),
                    VendedoresInativos = vendedoresConfigurados.Count(v => !v.AtivoDistribuicao),
                    Vendedores = vendedoresConfigurados,
                    DataConfiguracao = DateTime.UtcNow
                };
                
                return Ok(ApiResponse<ConfiguracaoVendedoresResponseDTO>.SuccessResponse(
                    resultado, 
                    $"Vendedores configurados com sucesso. Total: {resultado.TotalVendedores}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar vendedores para configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<ConfiguracaoVendedoresResponseDTO>.ErrorResponse(
                    "Erro ao configurar vendedores", ex.Message));
            }
        }

        /// <summary>
        /// Configura horários de trabalho para uma configuração de distribuição
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="configuracaoHorarios">Configuração de horários</param>
        /// <returns>Resultado da configuração</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpPut("{configuracaoId}/horarios")]
        public async Task<ActionResult<ApiResponse<object>>> ConfigurarHorarios(
            int configuracaoId,
            [FromBody] ConfigurarHorariosDistribuicaoDTO configuracaoHorarios)
        {
            try
            {
                _logger.LogInformation("Configurando horários para configuração {ConfiguracaoId}", configuracaoId);
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {configuracaoId}"));
                }
                
                // Validar configuração de horários
                var validacaoResultado = ValidarConfiguracaoHorarios(configuracaoHorarios);
                if (!validacaoResultado.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Configuração de horários inválida", 
                        validacaoResultado.ErrorMessage));
                }
                
                // Atualizar configuração com os novos parâmetros de horário
                configuracao.Atualizar(
                    configuracao.Nome,
                    configuracao.Descricao,
                    configuracao.Ativo,
                    configuracao.DataInicioVigencia,
                    configuracao.DataFimVigencia,
                    configuracao.MaxLeadsAtivosVendedor,
                    configuracaoHorarios.ConsiderarHorarioTrabalho,
                    configuracaoHorarios.ConsiderarFeriados,
                    configuracao.PermiteAtribuicaoManual
                );
                
                // Salvar parâmetros específicos de horário no campo ParametrosGerais
                var parametrosHorarios = new
                {
                    HorarioInicioExpediente = configuracaoHorarios.HorarioInicioExpediente,
                    HorarioFimExpediente = configuracaoHorarios.HorarioFimExpediente,
                    FusoHorario = configuracaoHorarios.FusoHorario,
                    HorariosPorDia = configuracaoHorarios.HorariosPorDia
                };
                
                // Atualizar parâmetros extras via JSON
                AtualizarParametrosExtras(configuracao, parametrosHorarios);
                
                _logger.LogInformation("Configuração de horários salva: ConsiderarHorarioTrabalho={ConsiderarHorarioTrabalho}, " +
                    "ConsiderarFeriados={ConsiderarFeriados}, " +
                    "HorarioInicioExpediente={HorarioInicioExpediente}, " +
                    "HorarioFimExpediente={HorarioFimExpediente}, " +
                    "FusoHorario={FusoHorario}",
                    configuracaoHorarios.ConsiderarHorarioTrabalho,
                    configuracaoHorarios.ConsiderarFeriados,
                    configuracaoHorarios.HorarioInicioExpediente,
                    configuracaoHorarios.HorarioFimExpediente,
                    configuracaoHorarios.FusoHorario);
                
                if (configuracaoHorarios.HorariosPorDia != null)
                {
                    foreach (var horarioDia in configuracaoHorarios.HorariosPorDia)
                    {
                        _logger.LogInformation("Horário para {DiaSemanaId}: Trabalha={TrabalhaNesteDia}, " +
                            "Início={HorarioInicio}, Fim={HorarioFim}, IntervaloAlmoço={IntervaloAlmoco}",
                            horarioDia.DiaSemanaId,
                            horarioDia.TrabalhaNesteDia,
                            horarioDia.HorarioInicio,
                            horarioDia.HorarioFim,
                            horarioDia.IntervaloAlmoco);
                    }
                }
                
                // Salvar configuração atualizada
                await _configuracaoRepository.UpdateAsync(configuracao);
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    ConsiderarHorarioTrabalho = configuracaoHorarios.ConsiderarHorarioTrabalho,
                    ConsiderarFeriados = configuracaoHorarios.ConsiderarFeriados,
                    HorarioInicioExpediente = configuracaoHorarios.HorarioInicioExpediente,
                    HorarioFimExpediente = configuracaoHorarios.HorarioFimExpediente,
                    FusoHorario = configuracaoHorarios.FusoHorario,
                    TotalDiasConfigurados = configuracaoHorarios.HorariosPorDia?.Count ?? 0,
                    DataConfiguracao = DateTime.UtcNow
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    "Horários configurados com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar horários para configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao configurar horários", ex.Message));
            }
        }

        /// <summary>
        /// Obtém os horários configurados para uma configuração de distribuição
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Configuração de horários</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("{configuracaoId}/horarios")]
        public async Task<ActionResult<ApiResponse<ConfigurarHorariosDistribuicaoDTO>>> ObterHorarios(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Obtendo horários da configuração {ConfiguracaoId}", configuracaoId);
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<ConfigurarHorariosDistribuicaoDTO>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {configuracaoId}"));
                }
                
                // Criar DTO de resposta com dados básicos
                var horariosDTO = new ConfigurarHorariosDistribuicaoDTO
                {
                    ConfiguracaoDistribuicaoId = configuracaoId,
                    ConsiderarHorarioTrabalho = configuracao.ConsiderarHorarioTrabalho,
                    ConsiderarFeriados = configuracao.ConsiderarFeriados
                };
                
                // Tentar deserializar parâmetros específicos de horário
                if (!string.IsNullOrEmpty(configuracao.ParametrosGerais))
                {
                    try
                    {
                        var parametros = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(
                            configuracao.ParametrosGerais);
                        
                        if (parametros != null)
                        {
                            // Extrair horários específicos
                            if (parametros.TryGetValue("HorarioInicioExpediente", out var inicioExpediente))
                                horariosDTO.HorarioInicioExpediente = inicioExpediente?.ToString();
                            
                            if (parametros.TryGetValue("HorarioFimExpediente", out var fimExpediente))
                                horariosDTO.HorarioFimExpediente = fimExpediente?.ToString();
                            
                            if (parametros.TryGetValue("FusoHorario", out var fusoHorario))
                                horariosDTO.FusoHorario = fusoHorario?.ToString();
                            
                            // Extrair horários por dia
                            if (parametros.TryGetValue("HorariosPorDia", out var horariosPorDia))
                            {
                                if (horariosPorDia is System.Text.Json.JsonElement horariosElement)
                                {
                                    var horariosDia = System.Text.Json.JsonSerializer.Deserialize<List<HorarioDiaSemanaDTO>>(
                                        horariosElement.GetRawText());
                                    horariosDTO.HorariosPorDia = horariosDia;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao deserializar parâmetros de horário da configuração {ConfiguracaoId}", configuracaoId);
                        // Continua sem os parâmetros específicos
                    }
                }
                
                return Ok(ApiResponse<ConfigurarHorariosDistribuicaoDTO>.SuccessResponse(
                    horariosDTO, 
                    "Horários obtidos com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter horários da configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<ConfigurarHorariosDistribuicaoDTO>.ErrorResponse(
                    "Erro ao obter horários", ex.Message));
            }
        }

        /// <summary>
        /// Verifica se a distribuição está ativa baseado nos horários configurados
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="dataHora">Data e hora para verificação (opcional)</param>
        /// <returns>Status da distribuição</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("{configuracaoId}/distribuicao-ativa")]
        public async Task<ActionResult<ApiResponse<object>>> VerificarDistribuicaoAtiva(
            int configuracaoId,
            [FromQuery] DateTime? dataHora = null)
        {
            try
            {
                _logger.LogInformation("Verificando se distribuição está ativa para configuração {ConfiguracaoId}", configuracaoId);
                
                var distribuicaoAtiva = await _horariosService.VerificarDistribuicaoAtivaAsync(configuracaoId, dataHora);
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    DistribuicaoAtiva = distribuicaoAtiva,
                    DataHoraVerificacao = dataHora ?? DateTime.Now,
                    DataHoraVerificacaoUtc = dataHora?.ToUniversalTime() ?? DateTime.UtcNow
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    $"Distribuição {(distribuicaoAtiva ? "ativa" : "inativa")}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar distribuição ativa para configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao verificar distribuição ativa", ex.Message));
            }
        }

        /// <summary>
        /// Verifica se um vendedor está disponível para receber leads
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="dataHora">Data e hora para verificação (opcional)</param>
        /// <returns>Status de disponibilidade do vendedor</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("{configuracaoId}/vendedores/{vendedorId}/disponibilidade")]
        public async Task<ActionResult<ApiResponse<object>>> VerificarDisponibilidadeVendedor(
            int configuracaoId,
            int vendedorId,
            [FromQuery] DateTime? dataHora = null)
        {
            try
            {
                _logger.LogInformation("Verificando disponibilidade do vendedor {VendedorId} para configuração {ConfiguracaoId}", 
                    vendedorId, configuracaoId);
                
                var disponivel = await _horariosService.VerificarDisponibilidadeVendedorAsync(configuracaoId, vendedorId, dataHora);
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    VendedorId = vendedorId,
                    Disponivel = disponivel,
                    DataHoraVerificacao = dataHora ?? DateTime.Now,
                    DataHoraVerificacaoUtc = dataHora?.ToUniversalTime() ?? DateTime.UtcNow
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    $"Vendedor {(disponivel ? "disponível" : "indisponível")}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar disponibilidade do vendedor {VendedorId} para configuração {ConfiguracaoId}", 
                    vendedorId, configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao verificar disponibilidade do vendedor", ex.Message));
            }
        }

        /// <summary>
        /// Obtém os próximos horários de disponibilidade para uma configuração
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="dias">Número de dias para calcular (padrão: 7)</param>
        /// <returns>Lista de horários de disponibilidade</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("{configuracaoId}/proximos-horarios")]
        public async Task<ActionResult<ApiResponse<List<HorarioDisponibilidadeDTO>>>> ObterProximosHorarios(
            int configuracaoId,
            [FromQuery] int dias = 7)
        {
            try
            {
                _logger.LogInformation("Obtendo próximos {Dias} dias de horários para configuração {ConfiguracaoId}", 
                    dias, configuracaoId);
                
                var horarios = await _horariosService.ObterProximosHorariosDisponibilidadeAsync(configuracaoId, dias);
                
                return Ok(ApiResponse<List<HorarioDisponibilidadeDTO>>.SuccessResponse(
                    horarios, 
                    $"Próximos {dias} dias de horários obtidos com sucesso. Total: {horarios.Count}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter próximos horários para configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<List<HorarioDisponibilidadeDTO>>.ErrorResponse(
                    "Erro ao obter próximos horários", ex.Message));
            }
        }

        /// <summary>
        /// Diagnóstico: Obtém todos os vendedores da empresa com seus status
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <returns>Diagnóstico completo dos vendedores</returns>
        [Authorize(Policy = "HorarioTrabalho")]
        [HttpGet("{configuracaoId}/diagnostico-vendedores")]
        public async Task<ActionResult<ApiResponse<object>>> DiagnosticoVendedores(int configuracaoId)
        {
            try
            {
                _logger.LogInformation("Executando diagnóstico de vendedores para configuração {ConfiguracaoId}", configuracaoId);
                
                // Verificar se a configuração existe
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        "Configuração não encontrada", 
                        $"Não foi encontrada nenhuma configuração com ID {configuracaoId}"));
                }
                
                // 1. Obter todos os vendedores da empresa
                var todosVendedores = await _usuarioReaderService.UsuariosEmpresa(configuracao.EmpresaId);
                
                // 2. Obter todos os vendedores na fila
                var vendedoresNaFila = await _filaService.ObterVendedoresNaFilaAsync(configuracao.EmpresaId);
                
                // 3. Criar diagnóstico
                var diagnostico = new List<object>();
                
                foreach (var vendedor in todosVendedores.OrderBy(v => v.Id))
                {
                    var posicaoFila = vendedoresNaFila.FirstOrDefault(v => v.MembroEquipeId == vendedor.Id);
                    
                    // Obter dados detalhados do usuário
                    var usuarioDetalhado = await _usuarioReaderService.ObterUsuarioPorIdAsync(vendedor.Id);
                    
                    diagnostico.Add(new
                    {
                        VendedorId = vendedor.Id,
                        Nome = vendedor.Nome,
                        Email = usuarioDetalhado?.Email ?? "N/A",
                        UsuarioAtivo = usuarioDetalhado?.Ativo ?? false,
                        NaFila = posicaoFila != null,
                        PosicaoFila = posicaoFila?.PosicaoFila,
                        StatusFila = posicaoFila?.StatusFilaDistribuicao?.Nome,
                        StatusFilaId = posicaoFila?.StatusFilaDistribuicaoId,
                        ElegivelParaDistribuicao = (usuarioDetalhado?.Ativo ?? false) && posicaoFila != null && 
                                                  posicaoFila.StatusFilaDistribuicao?.PermiteRecebimento == true
                    });
                }
                
                var resultado = new
                {
                    ConfiguracaoId = configuracaoId,
                    EmpresaId = configuracao.EmpresaId,
                    TotalVendedoresEmpresa = todosVendedores.Count,
                    TotalVendedoresNaFila = vendedoresNaFila.Count,
                    VendedoresElegiveis = diagnostico.Count(d => (bool)((dynamic)d).ElegivelParaDistribuicao),
                    Diagnostico = diagnostico
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    resultado, 
                    $"Diagnóstico concluído. {resultado.VendedoresElegiveis} vendedores elegíveis para distribuição"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar diagnóstico de vendedores para configuração {ConfiguracaoId}", configuracaoId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Erro ao executar diagnóstico", ex.Message));
            }
        }


    }
}