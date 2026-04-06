using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Implementação do serviço de gerenciamento de fila de distribuição
    /// Responsabilidade: Orquestrar operações de fila de distribuição
    /// Implementa métodos específicos para reduzir dependências diretas de repositórios
    /// </summary>
    public class FilaDistribuicaoService : IFilaDistribuicaoService
    {
        private readonly IFilaDistribuicaoRepository _filaRepository;
        private readonly IAtribuicaoLeadService _atribuicaoLeadService;
        private readonly ILeadEstatisticasService _leadEstatisticasService;
        private readonly IDistribuicaoConfiguracaoReaderService _configurationService;
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly ILogger<FilaDistribuicaoService> _logger;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public FilaDistribuicaoService(
            IFilaDistribuicaoRepository filaRepository,
            IAtribuicaoLeadService atribuicaoLeadService,
            ILeadEstatisticasService leadEstatisticasService,
            IDistribuicaoConfiguracaoReaderService configurationService,
            IUsuarioReaderService usuarioReaderService,
            ILogger<FilaDistribuicaoService> logger)
        {
            _filaRepository = filaRepository ?? throw new ArgumentNullException(nameof(filaRepository));
            _atribuicaoLeadService = atribuicaoLeadService ?? throw new ArgumentNullException(nameof(atribuicaoLeadService));
            _leadEstatisticasService = leadEstatisticasService ?? throw new ArgumentNullException(nameof(leadEstatisticasService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém o próximo vendedor na fila de distribuição
        /// </summary>
        public async Task<FilaDistribuicao?> ObterProximoVendedorFilaAsync(int empresaId, bool apenasAtivos = true)
        {
            return await _filaRepository.GetProximoVendedorFilaAsync(empresaId, apenasAtivos);
        }

        /// <summary>
        /// Obtém o próximo vendedor na fila considerando disponibilidade real (horários)
        /// </summary>
        public async Task<(FilaDistribuicao? Vendedor, bool FallbackAplicado, string? DetalhesFallback)> ObterProximoVendedorDisponivelAsync(int empresaId, bool apenasAtivos = true)
        {

            try
            {
                // Obter configuração da empresa para verificar se considera horário
                var configContext = await _configurationService.GetConfiguracaoComRegrasAsync(empresaId);
                if (!configContext.IsValid || configContext.Configuracao == null)
                {
                    _logger.LogWarning("Nenhuma configuração de distribuição ativa encontrada para a empresa {EmpresaId}", empresaId);
                    var vendedor = await ObterProximoVendedorFilaAsync(empresaId, apenasAtivos);
                    return (vendedor, false, null);
                }

                var configuracao = configContext.Configuracao;

                // Obter vendedores disponíveis (com filtros de horário aplicados)
                var (vendedoresDisponiveis, fallbackAplicado, detalhesFallback) = await _usuarioReaderService.ObterVendedoresDisponiveisAsync(empresaId, configuracao);

                _logger.LogInformation("Vendedores disponíveis obtidos: {Count} vendedores. Fallback aplicado: {FallbackAplicado}",
                    vendedoresDisponiveis.Count, fallbackAplicado);

                if (!vendedoresDisponiveis.Any())
                {
                    _logger.LogWarning("Nenhum vendedor disponível para a empresa {EmpresaId}", empresaId);
                    return (null, false, null);
                }

                // Obter próximo vendedor na fila
                var proximoVendedor = await ObterProximoVendedorFilaAsync(empresaId, true);

                if (proximoVendedor == null)
                {
                    _logger.LogWarning("Nenhum vendedor na fila para a empresa {EmpresaId}", empresaId);

                    // Se não há ninguém na fila, pegar o primeiro vendedor disponível
                    var vendedorId = vendedoresDisponiveis.First().Id;
                    proximoVendedor = await InicializarPosicaoFilaVendedorAsync(vendedorId, empresaId);
                }

                // Verificar se o vendedor está na lista de disponíveis
                var vendedorDisponivel = vendedoresDisponiveis.FirstOrDefault(v => v.Id == proximoVendedor.Id);
                if (vendedorDisponivel == null)
                {

                    // Avançar fila e tentar novamente (recursivamente)
                    await AtualizarPosicaoFilaAposAtribuicaoAsync(
                        proximoVendedor.EmpresaId, proximoVendedor.Id, null);

                    return await ObterProximoVendedorDisponivelAsync(empresaId, apenasAtivos);
                }

                // Se o fallback foi aplicado, não verificar disponibilidade no horário (pois o fallback já indica que não há vendedores no horário)
                if (fallbackAplicado)
                {
                    _logger.LogInformation("Fallback aplicado: aceitando vendedor {VendedorId} ({Nome}) mesmo fora do horário",
                        vendedorDisponivel.Id, vendedorDisponivel.Nome);
                }

                return (proximoVendedor, fallbackAplicado, detalhesFallback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter próximo vendedor disponível na fila para empresa {EmpresaId}", empresaId);
                var vendedor = await ObterProximoVendedorFilaAsync(empresaId, apenasAtivos);
                return (vendedor, false, null);
            }
        }

        /// <summary>
        /// Atualiza a posição do vendedor na fila após receber um lead
        /// </summary>
        public async Task<bool> AtualizarPosicaoFilaAposAtribuicaoAsync(int empresaId, int vendedorId, int? leadId)
        {
            _logger.LogDebug("Atualizando posição na fila após atribuição. Empresa: {EmpresaId}, Vendedor: {VendedorId}, Lead: {LeadId}",
                empresaId, vendedorId, leadId);

            try
            {
                // Buscar posição atual na fila
                var posicaoFila = await _filaRepository.GetPosicaoVendedorAsync(empresaId, vendedorId);
                if (posicaoFila == null)
                {
                    _logger.LogWarning("Vendedor {VendedorId} não encontrado na fila da empresa {EmpresaId}", vendedorId, empresaId);

                    // Inicializar posição na fila se não existir
                    posicaoFila = await InicializarPosicaoFilaVendedorAsync(vendedorId, empresaId);
                }

                // Registrar a atribuição do lead
                if (leadId.HasValue)
                {
                    await _filaRepository.RegistrarAtribuicaoLeadAsync(posicaoFila.Id, leadId.Value);
                }

                // Reorganizar a fila (mover para o final)
                await ReorganizarFilaAposDistribuicaoAsync(empresaId, vendedorId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar posição na fila do vendedor {VendedorId}", vendedorId);
                return false;
            }
        }

        /// <summary>
        /// Atribui um lead pelo método de fila simples (round-robin)
        /// </summary>
        public async Task<AtribuicaoLead?> AtribuirPorFilaSimplesAsync(
            int leadId,
            List<MembroEquipe> vendedoresDisponiveis,
            int configuracaoId,
            bool fallbackHorarioAplicado = false,
            string? detalhesFallbackHorario = null,
            int? empresaId = null)
        {

            if (vendedoresDisponiveis.Count == 0)
            {
                _logger.LogWarning("Nenhum vendedor disponível para atribuição");
                return null;
            }

            try
            {
                // Obter o lead para verificar a empresa
                if (empresaId == null)
                {
                    var lead = await _leadEstatisticasService.ObterLeadPorIdAsync(leadId, false);
                    if (lead == null)
                    {
                        _logger.LogWarning("Lead não encontrado. ID: {LeadId}", leadId);
                        return null;
                    }
                    empresaId = lead.EmpresaId;
                }

                // Obter próximo vendedor na fila
                var proximoVendedor = await ObterProximoVendedorFilaAsync(empresaId.Value, true);

                if (proximoVendedor == null)
                {
                    // Se não há ninguém na fila, pegar o primeiro vendedor disponível
                    var vendedorId = vendedoresDisponiveis.First().Id;
                    proximoVendedor = await InicializarPosicaoFilaVendedorAsync(vendedorId, empresaId.Value);
                }

                // Verificar se o vendedor está na lista de disponíveis
                if (!vendedoresDisponiveis.Any(v => v.Id == proximoVendedor.MembroEquipeId))
                {
                    // Avançar fila e tentar novamente (recursivamente)
                    await AtualizarPosicaoFilaAposAtribuicaoAsync(
                        proximoVendedor.EmpresaId, proximoVendedor.MembroEquipeId, null);

                    return await AtribuirPorFilaSimplesAsync(leadId, vendedoresDisponiveis, configuracaoId, empresaId: empresaId);
                }

                // Obter o ID do tipo de atribuição para "FILA_SIMPLES"
                int tipoAtribuicaoId = 1; // Valor padrão para tipo de atribuição por fila

                // Preparar parâmetros aplicados para distribuição por fila
                var parametrosAplicados = new Dictionary<string, object>
                {
                    { "configuracaoId", configuracaoId },
                    { "empresaId", empresaId.Value},
                    { "metodoDistribuicao", "FILA_SIMPLES" },
                    { "vendedorId", proximoVendedor.Id },
                    { "posicaoFila", proximoVendedor.PosicaoFila },
                    { "dataDistribuicao", TimeHelper.GetBrasiliaTime()},
                    { "totalVendedoresDisponiveis", vendedoresDisponiveis.Count }
                };

                string parametrosAplicadosJson = JsonSerializer.Serialize(parametrosAplicados);

                // Criar registro de atribuição
                var atribuicao = new AtribuicaoLead(
                    leadId: leadId,
                    membroAtribuidoId: proximoVendedor.MembroEquipeId,
                    tipoAtribuicaoId: tipoAtribuicaoId,
                    motivoAtribuicao: "Distribuição por fila circular (round-robin)",
                    atribuicaoAutomatica: true,
                    configuracaoDistribuicaoId: configuracaoId,
                    regraDistribuicaoId: null, // Atribuição por fila não usa regra específica
                    scoreVendedor: 1.0m, // Score padrão para distribuição por fila
                    membroAtribuiuId: null, // Atribuição automática
                    parametrosAplicados: parametrosAplicadosJson,
                    vendedoresElegiveis: JsonSerializer.Serialize(vendedoresDisponiveis.Select(v => new { v.Id, v.Usuario.Nome })),
                    scoresCalculados: null
                );

                // Registrar fallback de horário se aplicado
                if (fallbackHorarioAplicado && !string.IsNullOrEmpty(detalhesFallbackHorario))
                {
                    atribuicao.RegistrarFallbackHorario(detalhesFallbackHorario);
                }

                // Adicionar atribuição
                await _atribuicaoLeadService.CriarAtribuicaoAsync(atribuicao);

                // Atualizar posição na fila do vendedor
                await AtualizarPosicaoFilaAposAtribuicaoAsync(
                    proximoVendedor.EmpresaId, proximoVendedor.MembroEquipeId, leadId);

                return atribuicao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir lead {LeadId} por fila simples", leadId);
                return null;
            }
        }

        public async Task<int?> ObterVendedorPorFilaSimples(
         List<MembroEquipe> vendedoresDisponiveis,
         int configuracaoId,
         int empresaId)
        {

            if (vendedoresDisponiveis.Count == 0)
            {
                _logger.LogWarning("Nenhum vendedor disponível para atribuição");
                return null;
            }
            try
            {
                // Obter próximo vendedor na fila
                var proximoVendedor = await ObterProximoVendedorFilaAsync(empresaId, true);

                if (proximoVendedor == null)
                {
                    // Se não há ninguém na fila, pegar o primeiro vendedor disponível
                    var vendedorId = vendedoresDisponiveis.First().Id;
                    proximoVendedor = await InicializarPosicaoFilaVendedorAsync(vendedorId, empresaId);
                }

                // Verificar se o vendedor está na lista de disponíveis
                if (!vendedoresDisponiveis.Any(v => v.Id == proximoVendedor.MembroEquipeId))
                {
                    // Avançar fila e tentar novamente (recursivamente)
                    await AtualizarPosicaoFilaAposAtribuicaoAsync(
                        proximoVendedor.EmpresaId, proximoVendedor.MembroEquipeId, null);

                    return await ObterVendedorPorFilaSimples(vendedoresDisponiveis, configuracaoId, empresaId);
                }

                await AtualizarPosicaoFilaAposAtribuicaoAsync(
                    proximoVendedor.EmpresaId, proximoVendedor.MembroEquipeId, null);

                return proximoVendedor.MembroEquipeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar um usuário para distribuir por fila simples");
                return null;
            }
        }

        /// <summary>
        /// Reorganiza a fila após uma distribuição
        /// </summary>
        public async Task<bool> ReorganizarFilaAposDistribuicaoAsync(int empresaId, int vendedorId)
        {
            _logger.LogDebug("Reorganizando fila após distribuição. Empresa: {EmpresaId}, Vendedor: {VendedorId}",
                empresaId, vendedorId);

            return await _filaRepository.ReorganizarFilaAposDistribuicaoAsync(empresaId, vendedorId);
        }

        /// <summary>
        /// Inicializa a fila de distribuição para um novo vendedor
        /// </summary>
        public async Task<FilaDistribuicao> InicializarPosicaoFilaVendedorAsync(int vendedorId, int empresaId)
        {
            _logger.LogInformation("Inicializando posição na fila para vendedor {VendedorId} na empresa {EmpresaId}",
                vendedorId, empresaId);

            try
            {
                // Verificar se já existe
                var posicaoExistente = await _filaRepository.GetPosicaoVendedorAsync(empresaId, vendedorId);
                if (posicaoExistente != null)
                {
                    return posicaoExistente;
                }

                // Obter dados necessários
                int statusAtivoId = await _filaRepository.GetStatusFilaIdPorCodigoAsync("ATIVO");
                int proximaPosicao = await _filaRepository.GetProximaPosicaoFilaAsync(empresaId);

                // Criar nova posição
                var novaPosicao = new FilaDistribuicao(
                    membroEquipeId: vendedorId,
                    empresaId: empresaId,
                    posicaoFila: proximaPosicao,
                    statusFilaDistribuicaoId: statusAtivoId,
                    dataUltimoLeadRecebido: null
                );

                return await _filaRepository.AddPosicaoFilaAsync(novaPosicao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar posição na fila para vendedor {VendedorId}", vendedorId);
                throw new ApplicationException($"Erro ao inicializar posição na fila: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém a posição de um vendedor na fila
        /// </summary>
        public async Task<FilaDistribuicao?> ObterPosicaoVendedorAsync(int empresaId, int vendedorId)
        {
            _logger.LogDebug("Obtendo posição do vendedor {VendedorId} na fila da empresa {EmpresaId}",
                vendedorId, empresaId);

            return await _filaRepository.GetPosicaoVendedorAsync(empresaId, vendedorId);
        }
        public async Task<FilaDistribuicao?> ObterPosicaoVendedorExcluidoAsync(int empresaId, int vendedorId)
        {
            return await _filaRepository.GetPosicaoVendedorExcluidoAsync(empresaId, vendedorId);
        }

        /// <summary>
        /// Obtém o status da fila por ID
        /// </summary>
        public async Task<StatusFilaDistribuicao?> ObterStatusFilaAsync(int statusFilaId)
        {
            _logger.LogDebug("Obtendo status da fila {StatusFilaId}", statusFilaId);

            return await _filaRepository.GetStatusFilaByIdAsync(statusFilaId);
        }

        /// <summary>
        /// Obtém o status da fila por Código
        /// </summary>
        public async Task<int> ObterStatusFilaPorCodigoAsync(string codigo)
        {
            return await _filaRepository.GetStatusFilaIdPorCodigoAsync(codigo);
        }

        /// <summary>
        /// Obtém todos os vendedores na fila de distribuição para uma empresa
        /// </summary>
        public async Task<List<FilaDistribuicao>> ObterVendedoresNaFilaAsync(int empresaId)
        {
            _logger.LogDebug("Obtendo vendedores na fila para empresa {EmpresaId}", empresaId);

            return await _filaRepository.GetVendedoresNaFilaAsync(empresaId);
        }

        /// <summary>
        /// Atualiza o status de um vendedor na fila
        /// </summary>
        public async Task<bool> AtualizarStatusVendedorAsync(int empresaId, int vendedorId, int statusId)
        {
            _logger.LogDebug("Atualizando status do vendedor {VendedorId} para {StatusId} na empresa {EmpresaId}",
                vendedorId, statusId, empresaId);

            return await _filaRepository.AtualizarStatusVendedorAsync(empresaId, vendedorId, statusId);
        }

        public async Task RemoverVendedorFilaAsync(int empresaId, int vendedorId)
        {
            await _filaRepository.RemoverVendedorFilaAsync(empresaId, vendedorId);
        }

        public async Task RemoverTodosVendedorFilaAsync(List<MembroEquipe> membroEquipes)
        {
            await _filaRepository.RemoverTodosVendedoresFilaAsync(membroEquipes);
        }

        public async Task RestaurarVendedorNaFilaAsync(int empresaId, int vendedorId)
        {
            await _filaRepository.RestaurarVendedorNaFilaAsync(empresaId, vendedorId);
        }
    }
}