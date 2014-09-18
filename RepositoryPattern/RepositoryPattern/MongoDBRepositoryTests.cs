using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryPattern
{
    /// <summary>
    /// Basic Unit tests
    /// </summary>
    public class MongoDBRepositoryTests
    {
        /// <summary>
        /// Simple Create test
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestInsert()
        {
            var repository = new MongoDBRepository();
            var entity = await repository.Insert(new BaseEntity() { Name = "Alice" });
            Assert.Equal<string>("Alice", entity.Name);
        }

        /// <summary>
        /// Insert specified number of Entities
        /// Note: MongoDB has a Bulk Insert method that should be used for large data ingress
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(10000)]
        public async Task TestMultiInsert(int max)
        {
            var repository = new MongoDBRepository();
            var tasks = new List<Task<BaseEntity>>();
            for (int count = 0; count < max; count++)
            {
                tasks.Add(repository.Insert(new BaseEntity() { Name = String.Format("Alice{0}",count) }));
            }

            var names = new List<string>();
            // Run collection of Inserts concurrently
            foreach (var bucket in TaskProcessor.Interleaved(tasks))
            {
                var t = await bucket;
                try { var entity = await t;
                     names.Add(entity.Name);
                }
                catch (OperationCanceledException) { }
                catch (Exception exc) { 
                    Debug.WriteLine(exc); 
                }
            }

            Assert.Equal(max, names.Count);
        }

     
        /// <summary>
        /// Get a pre-existing entity by Id
        /// Note: Document id must already exist within MongoDB
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("839ab337-538a-4a11-9554-40433d2d3604")]
        public async Task TestGetById(string id)
        {
            var repository = new MongoDBRepository();
            var entity = await repository.Get<BaseEntity>(Guid.Parse(id));
            Assert.Equal<Guid>(Guid.Parse(id), entity.Id);
        }

        /// <summary>
        /// Get all the entities within the collection
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestGet()
        {
            var repository = new MongoDBRepository();
            var entities = await repository.Get<BaseEntity>();
            Assert.NotNull(entities);
            Assert.NotEmpty(entities);
        }

        /// <summary>
        /// Delete Single pre-existing entity
        /// </summary>
        /// <param name="id">Id of pre-existing entity</param>
        /// <returns></returns>
        [Theory]
        [InlineData("33f3f9ac-69c1-41c7-b992-f744c2e184c6")]
        public async Task TestDelete(string id)
        {
            var repository = new MongoDBRepository();
            await repository.Delete<BaseEntity>(Guid.Parse(id));

            Assert.Throws(typeof(AggregateException), delegate
            {
                var entity = repository.Get<BaseEntity>(Guid.Parse(id)).Result;
            });

        }


    }
}
