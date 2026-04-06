using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using System.Linq.Expressions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Base
{
    public class  BaseRepository : IBaseRepository
    {
        protected readonly WebsupplyConnectDbContext _context;
        protected readonly IUnitOfWork _unitOfWork;

        public BaseRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Adiciona uma nova entidade ao repositório
        /// </summary>
        public async Task<TEntity> CreateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                if (entity == null)
                    throw new DomainException("Entidade não pode ser nula", typeof(TEntity).Name);

                await _context.Set<TEntity>().AddAsync(entity);

                return entity;
            }
            catch (DomainException)
            {
                throw; // Re-lança exceções de domínio
            }
            catch (Exception ex)
            {
                throw new DomainException(
                    $"Erro inesperado ao criar a entidade: {ex.Message}",
                    typeof(TEntity).Name);
            }
        }

        /// <summary>
        /// Busca uma entidade pelo ID
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="id">ID da entidade</param>
        /// <param name="includeDeleted">Se deve incluir entidades excluídas logicamente (padrão: false)</param>
        /// <returns>A entidade encontrada ou null se não encontrada</returns>
        public async Task<TEntity?> GetByIdAsync<TEntity>(int id, bool includeDeleted = false) where TEntity : EntidadeBase
        {
            try
            {
                if (id <= 0)
                    return null;

                var query = _context.Set<TEntity>().AsQueryable();

                // Se não deve incluir excluídos, filtra apenas os não excluídos
                if (!includeDeleted)
                {
                    query = query.Where(e => !e.Excluido);
                }

                return await query.FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (InvalidOperationException ex)
            {
                throw new DomainException(
                    $"Operação inválida ao buscar a entidade com ID {id}: {ex}",
                    typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                throw new DomainException(
                    $"Erro inesperado ao buscar a entidade com ID {id}: {ex}",
                    typeof(TEntity).Name);
            }
        }

        /// <summary>
        /// Busca uma entidade por um predicado customizado
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="predicate">Expressão lambda para filtrar a entidade</param>
        /// <param name="includeDeleted">Se deve incluir entidades excluídas logicamente (padrão: false)</param>
        /// <returns>A entidade encontrada ou null se não encontrada</returns>
        public async Task<TEntity?> GetByPredicateAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : EntidadeBase
        {
            try
            {
                if (predicate == null)
                    throw new DomainException("Predicado não pode ser nulo", typeof(TEntity).Name);

                var query = _context.Set<TEntity>().AsQueryable();

                // Se não deve incluir excluídos, filtra apenas os não excluídos
                if (!includeDeleted)
                {
                    query = query.Where(e => !e.Excluido);
                }

                return await query.FirstOrDefaultAsync(predicate);
            }
            catch (InvalidOperationException ex)
            {
                throw new DomainException(
                    $"Operação inválida ao buscar a entidade: {ex}",
                    typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                throw new DomainException(
                    $"Erro inesperado ao buscar a entidade: {ex}",
                    typeof(TEntity).Name);
            }
        }

        /// <summary>
        /// Atualiza uma entidade existente.
        /// Entidades com Id == 0 (recém-criadas, não persistidas) são ignoradas para evitar InvalidOperationException.
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="entity">Entidade a ser atualizada</param>
        /// <returns>A entidade atualizada</returns>
        public TEntity Update<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity is EntidadeBase entidadeBase && entidadeBase.Id == 0)
                return entity;

            _context.Set<TEntity>().Update(entity);

            return entity;
        }

        /// <summary>
        /// Verifica se existe no banco de dados uma entidade do tipo informado com o valor de Id especificado.
        /// A entidade deve possuir uma propriedade pública chamada 'Id'.
        /// Este método é genérico e funciona com qualquer entidade que tenha a propriedade 'Id'.
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade a ser verificada.</typeparam>
        /// <param name="id">Valor do identificador a ser buscado.</param>
        /// <returns>Retorna true se existir um registro com o mesmo Id no banco de dados; caso contrário, false.</returns>
        public async Task<bool> ExistsInDatabaseAsync<TEntity>(int id) where TEntity : class
        {
            try
            {
                var idProperty = typeof(TEntity).GetProperty("Id");
                if (idProperty == null)
                    throw new DomainException("A entidade não possui uma propriedade pública chamada 'Id'.", typeof(TEntity).Name);

                return await _context.Set<TEntity>().AnyAsync(e =>
                    EF.Property<object>(e, "Id").Equals(id));
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException(
                    $"Erro inesperado ao verificar a existência da entidade: {ex.Message}",
                    typeof(TEntity).Name);
            }
        }

        /// <summary>
        /// Busca uma lista de entidades do tipo informado que atendem ao predicado fornecido.
        /// Permite aplicar uma ordenação opcional e filtrar registros excluídos logicamente, se necessário.
        /// Este método é genérico e funciona com qualquer entidade que herde de EntidadeBase.
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade a ser buscada.</typeparam>
        /// <param name="predicate">Expressão lambda que define a condição de filtragem.</param>
        /// <param name="orderBy">
        /// Função opcional para definir a ordenação dos resultados. 
        /// Caso não seja informada, os resultados não serão ordenados.
        /// </param>
        /// <param name="includeDeleted">
        /// Indica se os registros marcados como excluídos (propriedade Excluido = true) devem ser incluídos na consulta.
        /// O padrão é false, ou seja, registros excluídos são ignorados.
        /// </param>
        /// <returns>Retorna uma lista de entidades que atendem ao predicado informado.</returns>

        public async Task<List<TEntity>> GetListByPredicateAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool includeDeleted = false
        ) where TEntity : EntidadeBase
        {
            if (predicate == null)
                throw new DomainException("Predicado não pode ser nulo", typeof(TEntity).Name);

            var query = _context.Set<TEntity>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(e => !e.Excluido);
            }

            query = query.Where(predicate);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

    }
}
