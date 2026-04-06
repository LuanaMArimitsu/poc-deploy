using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao
{
    /// <summary>
    /// Implementação do repositório de configurações de distribuição.
    /// </summary>
    internal class ConfiguracaoDistribuicaoRepository : BaseRepository, IConfiguracaoDistribuicaoRepository
    {
        private readonly ILogger<ConfiguracaoDistribuicaoRepository> _logger;

        /// <summary>
        /// Construtor do repositório
        /// </summary>
        public ConfiguracaoDistribuicaoRepository(
            WebsupplyConnectDbContext dbContext, 
            IUnitOfWork unitOfWork,
            ILogger<ConfiguracaoDistribuicaoRepository> logger) 
            : base(dbContext, unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém a configuração de distribuição ativa para a empresa especificada
        /// </summary>
        public async Task<ConfiguracaoDistribuicao?> GetConfiguracaoAtivaAsync(int? empresaId, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Obtendo configuração de distribuição ativa para empresa {EmpresaId}", empresaId);

                var query = _context.ConfiguracaoDistribuicao
                    .Include(c => c.Regras)
                        .ThenInclude(r => r.TipoRegra)
                    .Include(c => c.Regras)
                        .ThenInclude(r => r.Parametros)
                    .Where(c => c.EmpresaId == empresaId && c.Ativo);

                if (!includeDeleted)
                {
                    query = query.Where(c => !c.Excluido);
                }

                // Considera vigência atual
                var dataAtual = DateTime.Now;
                query = query.Where(c => 
                    (!c.DataInicioVigencia.HasValue || c.DataInicioVigencia.Value <= dataAtual) &&
                    (!c.DataFimVigencia.HasValue || c.DataFimVigencia.Value >= dataAtual));

                var configuracao = await query.FirstOrDefaultAsync();

                if (configuracao == null)
                {
                    _logger.LogWarning("Nenhuma configuração de distribuição ativa encontrada para empresa {EmpresaId}", empresaId);
                }
                else
                {
                    _logger.LogDebug("Configuração de distribuição ativa encontrada: {ConfiguracaoId}", configuracao.Id);
                }

                return configuracao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração de distribuição ativa para empresa {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao obter configuração de distribuição ativa: {ex.Message}");
            }
        }

        /// <summary>
        /// Lista todas as configurações de distribuição para a empresa especificada
        /// </summary>
        public async Task<List<ConfiguracaoDistribuicao>> ListConfiguracoesAsync(int empresaId, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Listando configurações de distribuição para empresa {EmpresaId}", empresaId);

                var query = _context.ConfiguracaoDistribuicao
                    .Include(c => c.Regras)
                        .ThenInclude(r => r.TipoRegra)
                    .Where(c => c.EmpresaId == empresaId);

                if (!includeDeleted)
                {
                    query = query.Where(c => !c.Excluido);
                }

                var configuracoes = await query
                    .OrderByDescending(c => c.Ativo)
                    .ThenByDescending(c => c.DataModificacao)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} configurações para empresa {EmpresaId}", 
                    configuracoes.Count, empresaId);

                return configuracoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar configurações de distribuição para empresa {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao listar configurações de distribuição: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém uma configuração de distribuição pelo ID
        /// </summary>
        public async Task<ConfiguracaoDistribuicao?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Obtendo configuração de distribuição por ID {ConfiguracaoId}", id);

                var query = _context.ConfiguracaoDistribuicao
                    .Include(c => c.Regras)
                        .ThenInclude(r => r.TipoRegra)
                    .Include(c => c.Regras)
                        .ThenInclude(r => r.Parametros)
                    .Include(c => c.Empresa)
                    .Where(c => c.Id == id);

                if (!includeDeleted)
                {
                    query = query.Where(c => !c.Excluido);
                }

                var configuracao = await query.FirstOrDefaultAsync();

                if (configuracao == null)
                {
                    _logger.LogWarning("Configuração de distribuição {ConfiguracaoId} não encontrada", id);
                }

                return configuracao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração de distribuição por ID {ConfiguracaoId}", id);
                throw new InfraException($"Erro ao obter configuração de distribuição: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria uma nova configuração de distribuição
        /// </summary>
        public async Task<ConfiguracaoDistribuicao> CreateAsync(ConfiguracaoDistribuicao configuracao)
        {
            try
            {
                _logger.LogDebug("Criando nova configuração de distribuição: {Nome}", configuracao.Nome);

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Adicionar a configuração
                    await _context.ConfiguracaoDistribuicao.AddAsync(configuracao);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Configuração de distribuição criada com sucesso. ID: {Id}, Nome: {Nome}", 
                        configuracao.Id, configuracao.Nome);
                    return configuracao;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar configuração de distribuição: {Nome}", configuracao.Nome);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar configuração de distribuição: {Nome}", configuracao.Nome);
                throw new InfraException($"Erro ao criar configuração de distribuição: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Atualiza uma configuração de distribuição existente
        /// </summary>
        public async Task<ConfiguracaoDistribuicao> UpdateAsync(ConfiguracaoDistribuicao configuracao)
        {
            try
            {
                _logger.LogDebug("Atualizando configuração de distribuição: {Id}", configuracao.Id);

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Atualizar a configuração
                    _context.ConfiguracaoDistribuicao.Update(configuracao);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Configuração de distribuição atualizada com sucesso. ID: {Id}", 
                        configuracao.Id);
                    return configuracao;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao atualizar configuração de distribuição: {Id}", configuracao.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar configuração de distribuição: {Id}", configuracao.Id);
                throw new InfraException($"Erro ao atualizar configuração de distribuição: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se existe uma configuração ativa para a empresa especificada
        /// </summary>
        public async Task<bool> ExisteConfiguracaoAtivaAsync(int empresaId)
        {
            try
            {
                _logger.LogDebug("Verificando existência de configuração ativa para empresa {EmpresaId}", empresaId);

                var query = _context.ConfiguracaoDistribuicao
                    .Where(c => c.EmpresaId == empresaId && c.Ativo && !c.Excluido);

                // Considera vigência atual
                var dataAtual = DateTime.Now;
                query = query.Where(c => 
                    (!c.DataInicioVigencia.HasValue || c.DataInicioVigencia.Value <= dataAtual) &&
                    (!c.DataFimVigencia.HasValue || c.DataFimVigencia.Value >= dataAtual));

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de configuração ativa para empresa {EmpresaId}", 
                    empresaId);
                throw new InfraException($"Erro ao verificar existência de configuração ativa: {ex.Message}");
            }
        }

        /// <summary>
        /// Ativa uma configuração de distribuição e desativa outras da mesma empresa
        /// </summary>
        public async Task<bool> AtivarConfiguracaoAsync(int id)
        {
            try
            {
                _logger.LogDebug("Ativando configuração de distribuição {ConfiguracaoId}", id);

                var configuracao = await _context.ConfiguracaoDistribuicao
                    .FirstOrDefaultAsync(c => c.Id == id && !c.Excluido);

                if (configuracao == null)
                {
                    _logger.LogWarning("Configuração de distribuição {ConfiguracaoId} não encontrada para ativação", id);
                    return false;
                }

                try
                {
                    // Iniciar transação
                    await _unitOfWork.BeginTransactionAsync();

                    // Desativar todas as outras configurações da empresa
                    await DesativarTodasAsync(configuracao.EmpresaId);

                    // Ativar a configuração solicitada
                    configuracao.Ativar();
                    _context.Update(configuracao);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Configuração de distribuição {ConfiguracaoId} ativada com sucesso", id);
                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao ativar configuração de distribuição {ConfiguracaoId}", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar configuração de distribuição {ConfiguracaoId}", id);
                throw new InfraException($"Erro ao ativar configuração de distribuição: {ex.Message}");
            }
        }

        /// <summary>
        /// Desativa outras configurações da mesma empresa, mantendo apenas uma ativa
        /// </summary>
        public async Task<bool> DesativarOutrasConfiguracoesAsync(int empresaId, int configuracaoIdManter)
        {
            try
            {
                _logger.LogDebug("Desativando outras configurações da empresa {EmpresaId}, mantendo {ConfiguracaoIdManter}", 
                    empresaId, configuracaoIdManter);

                // Buscar configurações ativas (exceto a que deve permanecer)
                var configuracoesParaDesativar = await _context.ConfiguracaoDistribuicao
                    .Where(c => c.EmpresaId == empresaId && c.Ativo && !c.Excluido && c.Id != configuracaoIdManter)
                    .ToListAsync();

                if (!configuracoesParaDesativar.Any())
                {
                    _logger.LogDebug("Nenhuma configuração para desativar encontrada para empresa {EmpresaId}", empresaId);
                    return true;
                }

                // Desativar cada configuração
                foreach (var config in configuracoesParaDesativar)
                {
                    config.Desativar();
                    _context.Update(config);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Desativadas {Count} configurações da empresa {EmpresaId}, mantendo {ConfiguracaoIdManter} ativa", 
                    configuracoesParaDesativar.Count, empresaId, configuracaoIdManter);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar outras configurações da empresa {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao desativar outras configurações: {ex.Message}");
            }
        }

        /// <summary>
        /// Desativa todas as configurações de distribuição de uma empresa
        /// </summary>
        public async Task<bool> DesativarTodasAsync(int empresaId)
        {
            try
            {
                _logger.LogDebug("Desativando todas as configurações da empresa {EmpresaId}", empresaId);

                // Iniciar transação se ainda não houver uma ativa
                bool iniciarTransacao = !_unitOfWork.HasActiveTransaction;
                if (iniciarTransacao)
                    await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Buscar configurações ativas
                    var configuracoesAtivas = await _context.ConfiguracaoDistribuicao
                        .Where(c => c.EmpresaId == empresaId && c.Ativo && !c.Excluido)
                        .ToListAsync();

                    if (!configuracoesAtivas.Any())
                    {
                        _logger.LogDebug("Nenhuma configuração ativa encontrada para empresa {EmpresaId}", empresaId);
                        
                        // Commit da transação se foi iniciada aqui
                        if (iniciarTransacao)
                            await _unitOfWork.CommitAsync();
                            
                        return true;
                    }

                    // Desativar cada configuração
                    foreach (var config in configuracoesAtivas)
                    {
                        config.Desativar();
                        _context.Update(config);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação se foi iniciada aqui
                    if (iniciarTransacao)
                        await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Desativadas {Count} configurações da empresa {EmpresaId}", 
                        configuracoesAtivas.Count, empresaId);
                    return true;
                }
                catch (Exception ex)
                {
                    if (iniciarTransacao)
                        await _unitOfWork.RollbackAsync();
                        
                    _logger.LogError(ex, "Erro ao desativar configurações da empresa {EmpresaId}", empresaId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao desativar configurações da empresa {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao desativar configurações: {ex.Message}");
            }
        }

        /// <summary>
        /// Exclui logicamente uma configuração de distribuição
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogDebug("Excluindo configuração de distribuição {ConfiguracaoId}", id);

                // Verificar se a configuração existe
                var configuracao = await _context.ConfiguracaoDistribuicao
                    .FirstOrDefaultAsync(c => c.Id == id && !c.Excluido);

                if (configuracao == null)
                {
                    _logger.LogWarning("Configuração de distribuição {ConfiguracaoId} não encontrada para exclusão", id);
                    return false;
                }

                // Verificar se a configuração tem histórico de distribuição
                if (await TemHistoricoDistribuicaoAsync(id))
                {
                    _logger.LogWarning("Não é possível excluir configuração {ConfiguracaoId} pois possui histórico de distribuição", id);
                    throw new InfraException("Não é possível excluir configuração que possui histórico de distribuição");
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Excluir logicamente
                    configuracao.Excluir();
                    _context.Update(configuracao);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Configuração de distribuição {ConfiguracaoId} excluída com sucesso", id);
                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao excluir configuração de distribuição {ConfiguracaoId}", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir configuração de distribuição {ConfiguracaoId}", id);
                throw new InfraException($"Erro ao excluir configuração de distribuição: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se uma configuração de distribuição já foi utilizada em distribuições
        /// </summary>
        public async Task<bool> TemHistoricoDistribuicaoAsync(int configuracaoId)
        {
            try
            {
                _logger.LogDebug("Verificando se configuração {ConfiguracaoId} tem histórico de distribuição", configuracaoId);

                return await _context.HistoricoDistribuicao
                    .AnyAsync(h => h.ConfiguracaoDistribuicaoId == configuracaoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar histórico de distribuição para configuração {ConfiguracaoId}", configuracaoId);
                throw new InfraException($"Erro ao verificar histórico de distribuição: {ex.Message}");
            }
        }

        /// <summary>
        /// Associa regras de distribuição a uma configuração
        /// </summary>
        public async Task<bool> AssociarRegrasAsync(int configuracaoId, List<int> regrasIds)
        {
            try
            {
                _logger.LogDebug("Associando regras à configuração {ConfiguracaoId}", configuracaoId);

                // Verificar se a configuração existe
                var configuracao = await _context.ConfiguracaoDistribuicao
                    .Include(c => c.Regras)
                    .FirstOrDefaultAsync(c => c.Id == configuracaoId && !c.Excluido);

                if (configuracao == null)
                {
                    _logger.LogWarning("Configuração {ConfiguracaoId} não encontrada para associar regras", configuracaoId);
                    return false;
                }

                // Verificar se as regras existem
                var regras = await _context.RegraDistribuicao
                    .Where(r => regrasIds.Contains(r.Id) && !r.Excluido)
                    .ToListAsync();

                if (regras.Count != regrasIds.Count)
                {
                    _logger.LogWarning("Algumas regras não foram encontradas. Solicitadas: {Total}, Encontradas: {Encontradas}",
                        regrasIds.Count, regras.Count);
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Associar cada regra à configuração (se ainda não estiver associada)
                    foreach (var regra in regras)
                    {
                        if (!configuracao.Regras.Any(r => r.Id == regra.Id))
                        {
                            configuracao.AdicionarRegra(regra);
                        }
                    }

                    _context.Update(configuracao);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Associadas {Count} regras à configuração {ConfiguracaoId}", 
                        regras.Count, configuracaoId);
                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao associar regras à configuração {ConfiguracaoId}", configuracaoId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao associar regras à configuração {ConfiguracaoId}", configuracaoId);
                throw new InfraException($"Erro ao associar regras: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza as regras associadas a uma configuração
        /// </summary>
        public async Task<bool> AtualizarRegrasAsync(int configuracaoId, List<int> regrasIds)
        {
            try
            {
                _logger.LogDebug("Atualizando regras da configuração {ConfiguracaoId}", configuracaoId);

                // Verificar se a configuração existe
                var configuracao = await _context.ConfiguracaoDistribuicao
                    .Include(c => c.Regras)
                    .FirstOrDefaultAsync(c => c.Id == configuracaoId && !c.Excluido);

                if (configuracao == null)
                {
                    _logger.LogWarning("Configuração {ConfiguracaoId} não encontrada para atualizar regras", configuracaoId);
                    return false;
                }

                // Verificar se as regras existem
                var regras = await _context.RegraDistribuicao
                    .Where(r => regrasIds.Contains(r.Id) && !r.Excluido)
                    .ToListAsync();

                if (regras.Count != regrasIds.Count)
                {
                    _logger.LogWarning("Algumas regras não foram encontradas. Solicitadas: {Total}, Encontradas: {Encontradas}",
                        regrasIds.Count, regras.Count);
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Limpar regras existentes
                    configuracao.LimparRegras();

                    // Adicionar novas regras
                    foreach (var regra in regras)
                    {
                        configuracao.AdicionarRegra(regra);
                    }

                    _context.Update(configuracao);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Atualizadas regras da configuração {ConfiguracaoId}. Total: {Count}", 
                        configuracaoId, regras.Count);
                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao atualizar regras da configuração {ConfiguracaoId}", configuracaoId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar regras da configuração {ConfiguracaoId}", configuracaoId);
                throw new InfraException($"Erro ao atualizar regras: {ex.Message}");
            }
        }
    }
}