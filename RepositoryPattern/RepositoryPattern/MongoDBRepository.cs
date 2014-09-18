using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryPattern
{
    /// <summary>
    /// MongoDB Repository pattern in practice
    /// </summary>
    public class MongoDBRepository : IRepository
    {
        #region private properties
        /// <summary>
        /// Gets or sets the MongoDB Client
        /// </summary>
        private MongoClient client { get; set; }

        /// <summary>
        /// Gets or sets the MongoDB Server
        /// </summary>
        private MongoServer server { get; set; }

        /// <summary>
        /// Gets or sets the MongoDB database
        /// </summary>
        private MongoDatabase database { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public MongoDBRepository()
        {
            // Get the MongoDB Connection String from App.config
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            this.client = new MongoClient(connectionString);
            this.server = client.GetServer();

            // Get the DatabaseName from the connection String
            var connnectionUrl = new MongoUrlBuilder(connectionString);
            this.database = this.server.GetDatabase(connnectionUrl.DatabaseName);
        }
        #endregion

        /// <summary>
        /// Insert entity asynchronously
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity"></param>
        /// <returns>awaitable task of entity type</returns>
        public async Task<T> Insert<T>(T entity) where T : BaseEntity
        {

            Task<T> task = Task<T>.Factory.StartNew(() =>
                {
                    entity.Id = Guid.NewGuid();
                    var type = entity.GetType().Name;

                    var collection = database.GetCollection<T>(type);
                    var result = collection.Insert(entity);

                    if (!result.Ok)
                        throw new Exception(result.ErrorMessage);

                    return entity;
                });
            return await task;
        }

        /// <summary>
        /// Get all the entities within the collection as Queryable
        /// </summary>
        /// <typeparam name="T">Type of Entity</typeparam>
        /// <returns>awaitable task of queryable collection</returns>
        public async Task<IQueryable<T>> Get<T>() where T : BaseEntity
        {
            Task<IQueryable<T>> task = Task<IQueryable<T>>.Factory.StartNew(() =>
            {
                var type = typeof(T).Name;

                var collection = database.GetCollection<T>(type);
                return collection.FindAll().AsQueryable();
            });
            return await task;
        }

        /// <summary>
        /// Get specific entity by id
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="id">Id of entity</param>
        /// <returns>awaitable task of entity</returns>
        public async Task<T> Get<T>(object id) where T : BaseEntity
        {
            Task<T> task = Task<T>.Factory.StartNew(() =>
            {
                var type = typeof(T).Name;
                var collection = database.GetCollection<T>(type);
                var query = Query<T>.EQ(e => e.Id, id);
                var entity = collection.FindOne(query);
                if(entity == null)
                     throw new Exception("Not Found");
                return entity;
            });
            return await task;
        }

        /// <summary>
        /// Update a given entity
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>awaitable task of entity</returns>
        public async Task Update<T>(T entity) where T : BaseEntity
        {
            Task task = Task.Factory.StartNew(() =>
            {
                var query = Query<T>.EQ(e => e.Id, entity.Id);
                var update = MongoDB.Driver.Builders.Update<T>.Set(e => e, entity);
                var type = typeof(T).Name;
                var collection = database.GetCollection<T>(type);
                collection.Update(query, update);
            });
            await task;
        }

        /// <summary>
        /// Delete entity by Id
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="id">Id of entity</param>
        /// <returns>awaitable task</returns>
        public async Task Delete<T>(object id) where T : BaseEntity
        {
            Task task = Task.Factory.StartNew(() =>
            {
                var type = typeof(T).Name;
                var collection = database.GetCollection<T>(type);
                var entity = Query<T>.EQ(e => e.Id, id);
                var exists = Get<T>(id).Result;

                var result = collection.Remove(entity);

                if (!result.Ok)
                    throw new Exception(result.ErrorMessage);
            });
            await task;
        }
    }
}
