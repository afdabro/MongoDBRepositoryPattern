using System.Linq;
using System.Threading.Tasks;

namespace RepositoryPattern
{
    /// <summary>
    /// Repository Pattern
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Insert Entity into Repository
        /// </summary>
        /// <typeparam name="T">Type of Entity to Insert</typeparam>
        /// <param name="entity">Entity to Insert</param>
        /// <returns>Entity</returns>
        Task<T> Insert<T>(T entity) where T : BaseEntity;

        /// <summary>
        /// Get All Entities
        /// </summary>
        /// <typeparam name="T">Type of Entity</typeparam>
        /// <returns>Queryable collection</returns>
        Task<IQueryable<T>> Get<T>() where T : BaseEntity;

        /// <summary>
        /// Get Entity by Id
        /// </summary>
        /// <typeparam name="T">Type of Entity</typeparam>
        /// <param name="id">Id of Entity</param>
        /// <returns>Entity with given Id</returns>
        Task<T> Get<T>(object id) where T : BaseEntity;

        /// <summary>
        /// Update Entity
        /// </summary>
        /// <typeparam name="T">Type of Entity</typeparam>
        /// <param name="entity">Updated Entity</param>
        /// <returns>Task</returns>
        Task Update<T>(T entity) where T : BaseEntity;

        /// <summary>
        /// Delete Entity
        /// </summary>
        /// <typeparam name="T">Type of Entity</typeparam>
        /// <param name="id">Id of Entity</param>
        /// <returns>Task</returns>
        Task Delete<T>(object id) where T : BaseEntity;
    }
}
