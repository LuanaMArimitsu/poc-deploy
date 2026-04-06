using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao
{
    /// <summary>
    /// Implementação do repositório de regras de distribuição
    /// </summary>
    public class RegraDistribuicaoRepository : BaseRepository, IRegraDistribuicaoRepository
    {
        private readonly ILogger<RegraDistribuicaoRepository> _logger;

        /// <summary>
        /// Construtor do repositório
        /// </summary>
        /// <param name="dbContext">Contexto do banco de dados</param>
        /// <param name="unitOfWork">Unidade de trabalho para transações</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public RegraDistribuicaoRepository(
            WebsupplyConnectDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<RegraDistribuicaoRepository> logger)
            : base(dbContext, unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém uma regra de distribuição pelo ID
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Regra encontrada ou null</returns>
        public async Task<RegraDistribuicao?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Obtendo regra de distribuição por ID: {Id}", id);

                var query = _context.Set<RegraDistribuicao>().AsQueryable();

                // Incluir relacionamentos para carregamento antecipado
                query = query
                    .Include(r => r.TipoRegra)
                    .Include(r => r.Parametros);

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(r => !r.Excluido);

                // Filtrar por ID
                var regra = await query.FirstOrDefaultAsync(r => r.Id == id);

                if (regra == null)
                    _logger.LogWarning("Regra de distribuição não encontrada. ID: {Id}", id);
                else
                    _logger.LogDebug("Regra de distribuição encontrada: {Nome}", regra.Nome);

                return regra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter regra de distribuição por ID: {Id}", id);
                throw new InfraException($"Erro ao obter regra de distribuição: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todas as regras ativas para uma configuração específica
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Lista de regras ativas ordenadas por ordem</returns>
        public async Task<List<RegraDistribuicao>> ListRegrasAtivasPorConfiguracaoAsync(int configuracaoId, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Listando regras ativas para configuração: {ConfiguracaoId}", configuracaoId);

                var query = _context.Set<RegraDistribuicao>().AsQueryable();

                // Incluir relacionamentos para carregamento antecipado
                query = query
                    .Include(r => r.TipoRegra)
                    .Include(r => r.Parametros);

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(r => !r.Excluido);

                // Filtrar por configuração e status ativo
                var regras = await query
                    .Where(r => r.ConfiguracaoDistribuicaoId == configuracaoId && r.Ativo)
                    .OrderBy(r => r.Ordem)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} regras ativas para configuração {ConfiguracaoId}", regras.Count, configuracaoId);
                return regras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar regras ativas para configuração: {ConfiguracaoId}", configuracaoId);
                throw new InfraException($"Erro ao listar regras ativas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todas as regras para uma configuração específica
        /// </summary>
        /// <param name="configuracaoId">ID da configuração</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Lista de regras ordenadas por ordem</returns>
        public async Task<List<RegraDistribuicao>> ListRegrasPorConfiguracaoAsync(int configuracaoId, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Listando todas as regras para configuração: {ConfiguracaoId}", configuracaoId);

                var query = _context.Set<RegraDistribuicao>().AsQueryable();

                // Incluir relacionamentos para carregamento antecipado
                query = query
                    .Include(r => r.TipoRegra)
                    .Include(r => r.Parametros);

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(r => !r.Excluido);

                // Filtrar por configuração
                var regras = await query
                    .Where(r => r.ConfiguracaoDistribuicaoId == configuracaoId)
                    .OrderBy(r => r.Ordem)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} regras para configuração {ConfiguracaoId}", regras.Count, configuracaoId);
                return regras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar regras para configuração: {ConfiguracaoId}", configuracaoId);
                throw new InfraException($"Erro ao listar regras: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Atualiza a ordem de uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="novaOrdem">Nova ordem</param>
        /// <returns>True se atualizada com sucesso, false caso contrário</returns>
        public async Task<bool> AtualizarOrdemRegraAsync(int id, int novaOrdem)
        {
            try
            {
                _logger.LogDebug("Atualizando ordem da regra. ID: {Id}, Nova Ordem: {NovaOrdem}", id, novaOrdem);

                // Verificar se a regra existe
                var regra = await GetByIdAsync(id);
                if (regra == null)
                {
                    _logger.LogWarning("Regra não encontrada. ID: {Id}", id);
                    return false;
                }

                int ordemAtual = regra.Ordem;
                int configuracaoId = regra.ConfiguracaoDistribuicaoId;

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Atualizar a ordem das demais regras para acomodar a mudança
                    if (novaOrdem < ordemAtual)
                    {
                        // Incrementar a ordem das regras entre a nova ordem e a ordem atual
                        await _context.Set<RegraDistribuicao>()
                            .Where(r => r.ConfiguracaoDistribuicaoId == configuracaoId && 
                                   r.Ordem >= novaOrdem && 
                                   r.Ordem < ordemAtual && 
                                   !r.Excluido)
                            .ExecuteUpdateAsync(r => r
                                .SetProperty(x => x.Ordem, x => x.Ordem + 1)
                                .SetProperty(x => x.DataModificacao, TimeHelper.GetBrasiliaTime()));
                    }
                    else if (novaOrdem > ordemAtual)
                    {
                        // Decrementar a ordem das regras entre a ordem atual e a nova ordem
                        await _context.Set<RegraDistribuicao>()
                            .Where(r => r.ConfiguracaoDistribuicaoId == configuracaoId && 
                                   r.Ordem > ordemAtual && 
                                   r.Ordem <= novaOrdem && 
                                   !r.Excluido)
                            .ExecuteUpdateAsync(r => r
                                .SetProperty(x => x.Ordem, x => x.Ordem - 1)
                                .SetProperty(x => x.DataModificacao, TimeHelper.GetBrasiliaTime()));
                    }
                    else
                    {
                        // Ordem não mudou, nada a fazer
                        await _unitOfWork.CommitAsync();
                        return true;
                    }

                    // Atualizar a ordem da regra
                    var updated = await _context.Set<RegraDistribuicao>()
                        .Where(r => r.Id == id)
                        .ExecuteUpdateAsync(r => r
                            .SetProperty(x => x.Ordem, novaOrdem)
                            .SetProperty(x => x.DataModificacao, TimeHelper.GetBrasiliaTime()));

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Ordem da regra atualizada com sucesso. ID: {Id}, Nova Ordem: {NovaOrdem}", id, novaOrdem);
                    return updated > 0;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao atualizar ordem da regra. ID: {Id}", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar ordem da regra. ID: {Id}", id);
                throw new InfraException($"Erro ao atualizar ordem da regra: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ativa ou desativa uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="ativo">Novo status de ativação</param>
        /// <returns>True se atualizada com sucesso, false caso contrário</returns>
        public async Task<bool> AtivarDesativarRegraAsync(int id, bool ativo)
        {
            try
            {
                _logger.LogDebug("Alterando status da regra. ID: {Id}, Ativo: {Ativo}", id, ativo);

                // Verificar se a regra existe
                var regra = await GetByIdAsync(id);
                if (regra == null)
                {
                    _logger.LogWarning("Regra não encontrada. ID: {Id}", id);
                    return false;
                }

                // Verificar se a regra é obrigatória e está tentando desativar
                if (regra.Obrigatoria && !ativo)
                {
                    _logger.LogWarning("Não é possível desativar uma regra obrigatória. ID: {Id}", id);
                    throw new InfraException("Não é possível desativar uma regra obrigatória.");
                }

                // Atualizar o status da regra
                var updated = await _context.Set<RegraDistribuicao>()
                    .Where(r => r.Id == id)
                    .ExecuteUpdateAsync(r => r
                        .SetProperty(x => x.Ativo, ativo)
                        .SetProperty(x => x.DataModificacao, TimeHelper.GetBrasiliaTime()));

                _logger.LogInformation("Status da regra atualizado com sucesso. ID: {Id}, Ativo: {Ativo}", id, ativo);
                return updated > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status da regra. ID: {Id}", id);
                throw new InfraException($"Erro ao alterar status da regra: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém regras por tipo
        /// </summary>
        /// <param name="tipoRegraId">ID do tipo de regra</param>
        /// <param name="includeDeleted">Se deve incluir regras excluídas</param>
        /// <returns>Lista de regras do tipo especificado</returns>
        public async Task<List<RegraDistribuicao>> GetRegrasPorTipoAsync(int tipoRegraId, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Obtendo regras por tipo: {TipoRegraId}", tipoRegraId);

                var query = _context.Set<RegraDistribuicao>().AsQueryable();

                // Incluir relacionamentos para carregamento antecipado
                query = query
                    .Include(r => r.TipoRegra)
                    .Include(r => r.Parametros);

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(r => !r.Excluido);

                // Filtrar por tipo
                var regras = await query
                    .Where(r => r.TipoRegraId == tipoRegraId)
                    .OrderBy(r => r.ConfiguracaoDistribuicaoId)
                    .ThenBy(r => r.Ordem)
                    .ToListAsync();

                _logger.LogDebug("Encontradas {Count} regras do tipo {TipoRegraId}", regras.Count, tipoRegraId);
                return regras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter regras por tipo: {TipoRegraId}", tipoRegraId);
                throw new InfraException($"Erro ao obter regras por tipo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Atualiza os parâmetros de uma regra
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <param name="parametros">Lista de parâmetros atualizada</param>
        /// <returns>True se atualizada com sucesso, false caso contrário</returns>
        public async Task<bool> AtualizarParametrosRegraAsync(int id, List<ParametroRegraDistribuicao> parametros)
        {
            try
            {
                _logger.LogDebug("Atualizando parâmetros da regra. ID: {Id}, Quantidade de Parâmetros: {Count}", id, parametros?.Count ?? 0);

                // Verificar se a regra existe
                var regra = await GetByIdAsync(id);
                if (regra == null)
                {
                    _logger.LogWarning("Regra não encontrada. ID: {Id}", id);
                    return false;
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Remover parâmetros existentes primeiro
                    // Em vez de fazer a exclusão direta, vamos buscar e remover
                    var parametrosExistentes = await _context.Set<ParametroRegraDistribuicao>()
                        .Where(p => p.RegraDistribuicaoId == id)
                        .ToListAsync();
                    
                    if (parametrosExistentes.Any())
                    {
                        foreach (var parametro in parametrosExistentes)
                        {
                            parametro.Excluir();
                            _context.Update(parametro);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Adicionar novos parâmetros
                    if (parametros != null && parametros.Any())
                    {
                        foreach (var p in parametros)
                        {
                            // Criar uma nova instância usando o construtor
                            var novoParametro = new ParametroRegraDistribuicao(
                                id,
                                p.NomeParametro,
                                p.ValorParametro,
                                p.Descricao ?? ""
                            );
                            
                            await _context.Set<ParametroRegraDistribuicao>().AddAsync(novoParametro);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Atualizar JSON na regra
                    var parametrosJson = parametros != null && parametros.Any() 
                        ? System.Text.Json.JsonSerializer.Serialize(parametros.ToDictionary(p => p.NomeParametro, p => p.ValorParametro))
                        : "{}";

                    await _context.Set<RegraDistribuicao>()
                        .Where(r => r.Id == id)
                        .ExecuteUpdateAsync(r => r
                            .SetProperty(x => x.ParametrosJson, parametrosJson)
                            .SetProperty(x => x.DataModificacao, TimeHelper.GetBrasiliaTime()));

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Parâmetros da regra atualizados com sucesso. ID: {Id}", id);
                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao atualizar parâmetros da regra. ID: {Id}", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar parâmetros da regra. ID: {Id}", id);
                throw new InfraException($"Erro ao atualizar parâmetros da regra: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cria uma nova regra de distribuição
        /// </summary>
        /// <param name="regra">Entidade RegraDistribuicao a ser criada</param>
        /// <returns>Regra criada com ID gerado</returns>
        public async Task<RegraDistribuicao> CreateRegraAsync(RegraDistribuicao regra)
        {
            try
            {
                _logger.LogDebug("Criando nova regra de distribuição: {Nome}", regra.Nome);

                // Verificar se a configuração existe
                var configuracaoExiste = await _context.Set<ConfiguracaoDistribuicao>()
                    .AnyAsync(c => c.Id == regra.ConfiguracaoDistribuicaoId && !c.Excluido);

                if (!configuracaoExiste)
                {
                    _logger.LogWarning("Configuração não encontrada. ID: {ConfiguracaoId}", regra.ConfiguracaoDistribuicaoId);
                    throw new InfraException($"Configuração não encontrada. ID: {regra.ConfiguracaoDistribuicaoId}");
                }

                // Verificar se o tipo de regra existe
                var tipoRegraExiste = await _context.Set<TipoRegraDistribuicao>()
                    .AnyAsync(t => t.Id == regra.TipoRegraId && !t.Excluido);

                if (!tipoRegraExiste)
                {
                    _logger.LogWarning("Tipo de regra não encontrado. ID: {TipoRegraId}", regra.TipoRegraId);
                    throw new InfraException($"Tipo de regra não encontrado. ID: {regra.TipoRegraId}");
                }

                // Definir a ordem (se não especificada)
                if (regra.Ordem <= 0)
                {
                    var ultimaOrdem = await _context.Set<RegraDistribuicao>()
                        .Where(r => r.ConfiguracaoDistribuicaoId == regra.ConfiguracaoDistribuicaoId && !r.Excluido)
                        .OrderByDescending(r => r.Ordem)
                        .Select(r => r.Ordem)
                        .FirstOrDefaultAsync();

                    // A propriedade Ordem é privada no modelo, então vamos usar reflexão para ajustá-la
                    var ordemProperty = typeof(RegraDistribuicao).GetProperty("Ordem");
                    ordemProperty?.SetValue(regra, ultimaOrdem + 1);
                }

                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Adicionar a regra
                    await _context.Set<RegraDistribuicao>().AddAsync(regra);
                    await _context.SaveChangesAsync();

                    // Armazenar os parâmetros originais
                    var parametrosOriginais = new List<ParametroRegraDistribuicao>();
                    if (regra.Parametros != null)
                    {
                        parametrosOriginais.AddRange(regra.Parametros);
                    }

                    // Limpar a coleção de parâmetros (para evitar erros de tracking)
                    regra.Parametros.Clear();
                    
                    // Adicionar parâmetros
                    if (parametrosOriginais.Any())
                    {
                        foreach (var parametroOriginal in parametrosOriginais)
                        {
                            // Criar uma nova instância usando o construtor
                            var novoParametro = new ParametroRegraDistribuicao(
                                regra.Id,
                                parametroOriginal.NomeParametro,
                                parametroOriginal.ValorParametro,
                                parametroOriginal.Descricao
                            );
                            
                            await _context.Set<ParametroRegraDistribuicao>().AddAsync(novoParametro);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    // Recarregar a regra com os parâmetros
                    regra = await GetByIdAsync(regra.Id) ?? regra;

                    _logger.LogInformation("Nova regra de distribuição criada com sucesso. ID: {Id}, Nome: {Nome}", regra.Id, regra.Nome);
                    return regra;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao criar regra de distribuição: {Nome}", regra.Nome);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar regra de distribuição: {Nome}", regra.Nome);
                throw new InfraException($"Erro ao criar regra de distribuição: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exclui logicamente uma regra de distribuição
        /// </summary>
        /// <param name="id">ID da regra</param>
        /// <returns>True se excluída com sucesso, false caso contrário</returns>
        public async Task<bool> DeleteRegraAsync(int id)
        {
            try
            {
                _logger.LogDebug("Excluindo regra de distribuição. ID: {Id}", id);

                // Verificar se a regra existe
                var regra = await GetByIdAsync(id);
                if (regra == null)
                {
                    _logger.LogWarning("Regra não encontrada. ID: {Id}", id);
                    return false;
                }

                // Verificar se a regra é obrigatória
                if (regra.Obrigatoria)
                {
                    _logger.LogWarning("Não é possível excluir uma regra obrigatória. ID: {Id}", id);
                    throw new InfraException("Não é possível excluir uma regra obrigatória.");
                }

                // Em vez de usar ExecuteUpdateAsync, vamos carregar e atualizar manualmente
                
                // Iniciar transação
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Marcar a regra como excluída
                    regra.Excluir();
                    _context.Update(regra);
                    
                    // Carregar e marcar parâmetros como excluídos
                    var parametros = await _context.Set<ParametroRegraDistribuicao>()
                        .Where(p => p.RegraDistribuicaoId == id)
                        .ToListAsync();
                        
                    foreach (var parametro in parametros)
                    {
                        parametro.Excluir();
                        _context.Update(parametro);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    // Commit da transação
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation("Regra de distribuição excluída com sucesso. ID: {Id}", id);
                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Erro ao excluir regra de distribuição. ID: {Id}", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir regra de distribuição. ID: {Id}", id);
                throw new InfraException($"Erro ao excluir regra de distribuição: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todos os tipos de regras disponíveis
        /// </summary>
        /// <param name="includeDeleted">Se deve incluir tipos excluídos</param>
        /// <returns>Lista de tipos de regras</returns>
        public async Task<List<TipoRegraDistribuicao>> ListTiposRegrasAsync(bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Listando tipos de regras de distribuição");

                var query = _context.Set<TipoRegraDistribuicao>().AsQueryable();

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(t => !t.Excluido);

                // Ordenar por ID
                var tiposRegras = await query
                    .OrderBy(t => t.Id)
                    .ToListAsync();

                _logger.LogDebug("Encontrados {Count} tipos de regras", tiposRegras.Count);
                return tiposRegras;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar tipos de regras de distribuição");
                throw new InfraException($"Erro ao listar tipos de regras: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém um tipo de regra pelo ID
        /// </summary>
        /// <param name="id">ID do tipo de regra</param>
        /// <param name="includeDeleted">Se deve incluir tipos excluídos</param>
        /// <returns>Tipo de regra encontrado ou null</returns>
        public async Task<TipoRegraDistribuicao?> GetTipoRegraByIdAsync(int id, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Obtendo tipo de regra por ID: {Id}", id);

                var query = _context.Set<TipoRegraDistribuicao>().AsQueryable();

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(t => !t.Excluido);

                // Filtrar por ID
                var tipoRegra = await query.FirstOrDefaultAsync(t => t.Id == id);

                if (tipoRegra == null)
                    _logger.LogWarning("Tipo de regra não encontrado. ID: {Id}", id);
                else
                    _logger.LogDebug("Tipo de regra encontrado: {Nome}", tipoRegra.Nome);

                return tipoRegra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipo de regra por ID: {Id}", id);
                throw new InfraException($"Erro ao obter tipo de regra: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém um tipo de regra pelo código
        /// </summary>
        /// <param name="codigo">Código do tipo de regra</param>
        /// <param name="includeDeleted">Se deve incluir tipos excluídos</param>
        /// <returns>Tipo de regra encontrado ou null</returns>
        public async Task<TipoRegraDistribuicao?> GetTipoRegraByCodigoAsync(string codigo, bool includeDeleted = false)
        {
            try
            {
                _logger.LogDebug("Obtendo tipo de regra por código: {Codigo}", codigo);

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    _logger.LogWarning("Código do tipo de regra não informado");
                    throw new InfraException("Código do tipo de regra não informado");
                }

                var query = _context.Set<TipoRegraDistribuicao>().AsQueryable();

                // Aplicar filtro de excluídos se necessário
                if (!includeDeleted)
                    query = query.Where(t => !t.Excluido);

                // Filtrar por código
                var tipoRegra = await query.FirstOrDefaultAsync(t => t.Codigo == codigo);

                if (tipoRegra == null)
                    _logger.LogWarning("Tipo de regra não encontrado. Código: {Codigo}", codigo);
                else
                    _logger.LogDebug("Tipo de regra encontrado: {Nome}", tipoRegra.Nome);

                return tipoRegra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipo de regra por código: {Codigo}", codigo);
                throw new InfraException($"Erro ao obter tipo de regra: {ex.Message}", ex);
            }
        }
    }
}