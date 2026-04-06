using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comum;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comum;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comum
{
    /// <summary>
    /// Implementação do repositório de feriados
    /// </summary>
    internal class FeriadoRepository : IFeriadoRepository
    {
        private readonly WebsupplyConnectDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FeriadoRepository> _logger;

        /// <summary>
        /// Construtor do repositório
        /// </summary>
        public FeriadoRepository(
            WebsupplyConnectDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<FeriadoRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adiciona uma entidade genérica ao repositório
        /// </summary>
        public async Task<TEntity> CreateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Obtém uma entidade pelo seu ID
        /// </summary>
        public async Task<TEntity?> GetByIdAsync<TEntity>(int id, bool includeDeleted = false) where TEntity : EntidadeBase
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(e => !e.Excluido);
            }
            
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Obtém uma entidade por predicado
        /// </summary>
        public async Task<TEntity?> GetByPredicateAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : EntidadeBase
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(e => !e.Excluido);
            }
            
            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Atualiza uma entidade
        /// </summary>
        public TEntity Update<TEntity>(TEntity entity) where TEntity : class
        {
            _dbContext.Set<TEntity>().Update(entity);
            return entity;
        }

        /// <summary>
        /// Verifica se uma entidade existe no banco
        /// </summary>
        public async Task<bool> ExistsInDatabaseAsync<TEntity>(int id) where TEntity : class
        {
            // Obtém o nome da propriedade de ID
            var propId = typeof(TEntity).GetProperty("Id");
            if (propId == null)
            {
                throw new ArgumentException($"A entidade {typeof(TEntity).Name} não possui uma propriedade 'Id'");
            }

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, propId);
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

            return await _dbContext.Set<TEntity>().AnyAsync(lambda);
        }

        /// <summary>
        /// Adiciona um novo feriado
        /// </summary>
        public async Task<Feriado> AddAsync(Feriado feriado)
        {
            return await CreateAsync(feriado);
        }

        /// <summary>
        /// Obtém todos os feriados
        /// </summary>
        public async Task<List<Feriado>> GetAllAsync()
        {
            try
            {
                return await _dbContext.Set<Feriado>()
                    .AsNoTracking()
                    .Where(f => !f.Excluido)
                    .OrderBy(f => f.Data)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os feriados");
                throw new InfraException($"Erro ao obter todos os feriados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove um feriado (exclusão lógica)
        /// </summary>
        public async Task<bool> RemoveAsync(int id)
        {
            try
            {
                var feriado = await GetByIdAsync<Feriado>(id);
                if (feriado == null)
                {
                    return false;
                }

                feriado.GetType().GetProperty("Excluido")?.SetValue(feriado, true);
                feriado.GetType().GetProperty("DataModificacao")?.SetValue(feriado, DateTime.Now);

                _dbContext.Update(feriado);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover feriado ID {Id}", id);
                throw new InfraException($"Erro ao remover feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Salva as alterações no banco de dados
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Obtém todos os feriados para uma empresa específica, incluindo feriados nacionais
        /// </summary>
        public async Task<List<Feriado>> ObterFeriadosEmpresaAsync(int empresaId, int? ano = null)
        {
            try
            {
                _logger.LogDebug("Obtendo feriados para empresa {EmpresaId}, ano {Ano}", empresaId, ano);

                var query = _dbContext.Set<Feriado>()
                    .AsNoTracking()
                    .Where(f => !f.Excluido &&
                              (f.EmpresaId == empresaId || // Feriados da empresa
                               f.Tipo.ToUpper() == "NACIONAL")); // Feriados nacionais
                
                // Filtro por ano se especificado
                if (ano.HasValue)
                {
                    query = query.Where(f => f.Data.Year == ano.Value || f.Recorrente);
                }

                var feriados = await query
                    .OrderBy(f => f.Data)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} feriados para empresa {EmpresaId}", 
                    feriados.Count, empresaId);

                return feriados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados para empresa {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao obter feriados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se uma data específica é feriado, opcionalmente filtrando por empresa
        /// </summary>
        public async Task<bool> VerificarDataFeriadoAsync(DateTime data, int? empresaId = null, bool considerarRecorrentes = true)
        {
            try
            {
                _logger.LogDebug("Verificando se {Data} é feriado {EmpresaInfo}", 
                    data.ToShortDateString(), 
                    empresaId.HasValue ? $"para empresa ID: {empresaId}" : "em geral");

                var dataConsulta = data.Date; // Normaliza para meia-noite
                
                // Construir a consulta base
                var query = _dbContext.Set<Feriado>()
                    .AsNoTracking()
                    .Where(f => !f.Excluido);
                    
                // Se empresaId for fornecido, filtrar por empresa e feriados nacionais
                if (empresaId.HasValue)
                {
                    query = query.Where(f => f.EmpresaId == empresaId || f.Tipo.ToUpper() == "NACIONAL");
                }

                // Se considerarmos feriados recorrentes, precisamos verificar dia e mês
                if (considerarRecorrentes)
                {
                    var count = await query.CountAsync(f => 
                        (f.Data.Date == dataConsulta && !f.Recorrente) || // Não recorrentes: data exata
                        (f.Data.Month == dataConsulta.Month && f.Data.Day == dataConsulta.Day && f.Recorrente)); // Recorrentes: dia e mês
                    
                    return count > 0;
                }
                else
                {
                    // Se não considerarmos recorrentes, verificamos apenas data exata
                    var count = await query.CountAsync(f => f.Data.Date == dataConsulta && !f.Recorrente);
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se {Data} é feriado {EmpresaInfo}", 
                    data.ToShortDateString(), 
                    empresaId.HasValue ? $"para empresa ID: {empresaId}" : "em geral");
                throw new InfraException($"Erro ao verificar data de feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém os próximos feriados a partir da data atual
        /// </summary>
        public async Task<List<Feriado>> ObterProximosFeriadosAsync(int empresaId, int quantidade = 5)
        {
            try
            {
                _logger.LogDebug("Obtendo {Quantidade} próximos feriados para empresa {EmpresaId}", 
                    quantidade, empresaId);

                var dataAtual = DateTime.Today;
                var anoAtual = dataAtual.Year;
                
                // Obtém todos os feriados da empresa e nacionais
                var todosFeriados = await _dbContext.Set<Feriado>()
                    .AsNoTracking()
                    .Where(f => !f.Excluido &&
                             (f.EmpresaId == empresaId || f.Tipo.ToUpper() == "NACIONAL"))
                    .ToListAsync();

                // Lista para armazenar os próximos feriados
                var proximosFeriados = new List<Feriado>();
                
                // Primeiro, verifica feriados não recorrentes deste ano
                var feriadosNaoRecorrentesAnoAtual = todosFeriados
                    .Where(f => !f.Recorrente && f.Data.Year == anoAtual && f.Data.Date >= dataAtual)
                    .OrderBy(f => f.Data)
                    .ToList();
                
                proximosFeriados.AddRange(feriadosNaoRecorrentesAnoAtual);

                // Depois, verifica feriados recorrentes para este ano
                var feriadosRecorrentes = todosFeriados
                    .Where(f => f.Recorrente)
                    .ToList();

                foreach (var feriado in feriadosRecorrentes)
                {
                    // Cria uma data para este ano
                    var dataFeriado = new DateTime(anoAtual, feriado.Data.Month, feriado.Data.Day);
                    
                    // Se já passou, avança para o próximo ano
                    if (dataFeriado < dataAtual)
                    {
                        dataFeriado = new DateTime(anoAtual + 1, feriado.Data.Month, feriado.Data.Day);
                    }
                    
                    // Cria uma cópia do feriado com a data ajustada para este ano ou próximo
                    var feriadoAjustado = new Feriado(
                        feriado.Nome,
                        dataFeriado,
                        feriado.Tipo,
                        feriado.Recorrente,
                        feriado.Descricao,
                        feriado.EmpresaId,
                        feriado.UF,
                        feriado.CodigoMunicipio);
                    
                    // Define o mesmo ID para possibilitar rastreamento
                    var propriedadeId = feriadoAjustado.GetType().GetProperty("Id");
                    if (propriedadeId != null && propriedadeId.CanWrite)
                    {
                        propriedadeId.SetValue(feriadoAjustado, feriado.Id);
                    }
                    
                    proximosFeriados.Add(feriadoAjustado);
                }

                // Se ainda não temos feriados suficientes, podemos incluir feriados não recorrentes do próximo ano
                if (proximosFeriados.Count < quantidade)
                {
                    var feriadosNaoRecorrentesProximoAno = todosFeriados
                        .Where(f => !f.Recorrente && f.Data.Year == anoAtual + 1)
                        .OrderBy(f => f.Data)
                        .ToList();
                    
                    proximosFeriados.AddRange(feriadosNaoRecorrentesProximoAno);
                }

                // Ordena por data e limita pela quantidade
                return proximosFeriados
                    .OrderBy(f => f.Data)
                    .Take(quantidade)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter próximos feriados para empresa {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao obter próximos feriados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém feriados por tipo
        /// </summary>
        public async Task<List<Feriado>> ObterFeriadosPorTipoAsync(string tipo, int? ano = null)
        {
            try
            {
                _logger.LogDebug("Obtendo feriados do tipo {Tipo}, ano {Ano}", tipo, ano);

                var query = _dbContext.Set<Feriado>()
                    .AsNoTracking()
                    .Where(f => !f.Excluido && f.Tipo.ToUpper() == tipo.ToUpper());
                
                // Filtro por ano se especificado
                if (ano.HasValue)
                {
                    query = query.Where(f => f.Data.Year == ano.Value || f.Recorrente);
                }

                var feriados = await query
                    .OrderBy(f => f.Data)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} feriados do tipo {Tipo}", 
                    feriados.Count, tipo);

                return feriados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados do tipo {Tipo}", tipo);
                throw new InfraException($"Erro ao obter feriados por tipo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém feriados por UF
        /// </summary>
        public async Task<List<Feriado>> ObterFeriadosPorUFAsync(string uf, int? ano = null)
        {
            try
            {
                _logger.LogDebug("Obtendo feriados da UF {UF}, ano {Ano}", uf, ano);

                var query = _dbContext.Set<Feriado>()
                    .AsNoTracking()
                    .Where(f => !f.Excluido && 
                             f.UF.ToUpper() == uf.ToUpper() && 
                             f.Tipo.ToUpper() == "ESTADUAL");
                
                // Filtro por ano se especificado
                if (ano.HasValue)
                {
                    query = query.Where(f => f.Data.Year == ano.Value || f.Recorrente);
                }

                var feriados = await query
                    .OrderBy(f => f.Data)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} feriados da UF {UF}", 
                    feriados.Count, uf);

                return feriados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados da UF {UF}", uf);
                throw new InfraException($"Erro ao obter feriados por UF: {ex.Message}", ex);
            }
        }

        public Task<List<TEntity>> GetListByPredicateAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool includeDeleted = false) where TEntity : EntidadeBase
        {
            throw new NotImplementedException();
        }
    }
}