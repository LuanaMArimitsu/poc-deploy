using WebsupplyConnect.Domain.Entities.Base;
using System.Linq.Expressions;

namespace WebsupplyConnect.Domain.Interfaces.Base
{
    public interface IBaseRepository
    {
        /// <summary>
        /// Adiciona uma nova entidade ao repositório
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns>A entidade adicionada</returns>
        Task<TEntity> CreateAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Busca uma entidade pelo ID
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="id">ID da entidade</param>
        /// <param name="includeDeleted">Se deve incluir entidades excluídas logicamente (padrão: false)</param>
        /// <returns>A entidade encontrada ou null se não encontrada</returns>
        Task<TEntity?> GetByIdAsync<TEntity>(int id, bool includeDeleted = false) where TEntity : EntidadeBase;

        /// <summary>
        /// Busca uma entidade por um predicado customizado
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="predicate">Expressão lambda para filtrar a entidade</param>
        /// <param name="includeDeleted">Se deve incluir entidades excluídas logicamente (padrão: false)</param>
        /// <returns>A entidade encontrada ou null se não encontrada</returns>
        Task<TEntity?> GetByPredicateAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false) where TEntity : EntidadeBase;

        /// <summary>
        /// Atualiza uma entidade existente
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade</typeparam>
        /// <param name="entity">Entidade a ser atualizada</param>
        /// <returns>A entidade atualizada</returns>
        TEntity Update<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Verifica se existe no banco de dados uma entidade do tipo informado com o valor de Id especificado.
        /// A entidade deve possuir uma propriedade pública chamada 'Id'.
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade a ser verificada.</typeparam>
        /// <param name="id">Valor do identificador a ser buscado.</param>
        /// <returns>Retorna true se existir um registro com o mesmo Id no banco de dados; caso contrário, false.</returns>
        Task<bool> ExistsInDatabaseAsync<TEntity>(int id) where TEntity : class;

        /// <summary>
        /// Obtém uma lista de entidades do tipo informado que satisfaçam o predicado especificado.
        /// Permite ordenação opcional e inclusão de registros logicamente excluídos.
        /// </summary>
        /// <typeparam name="TEntity">Tipo da entidade a ser recuperada.</typeparam>
        /// <param name="predicate">Expressão booleana utilizada como filtro para os registros.</param>
        /// <param name="orderBy">Função opcional para ordenar os resultados.</param>
        /// <param name="includeDeleted">Indica se os registros marcados como excluídos logicamente devem ser incluídos.</param>
        /// <returns>Retorna uma lista de entidades que atendem ao critério especificado.</returns>
        Task<List<TEntity>> GetListByPredicateAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool includeDeleted = false) where TEntity : EntidadeBase;


    }
}
