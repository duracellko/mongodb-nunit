using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoRepo
{
    public class EntityRepository
    {
        private readonly IMongoCollection<Entity> _collection;

        public EntityRepository(IMongoCollection<Entity> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public async Task<Entity> Get(string id)
        {
            var cursor = await _collection.FindAsync(o => o.Id == id);
            return await cursor.SingleAsync();
        }

        public async Task<IEnumerable<Entity>> GetByName(string nameFilter)
        {
            var cursor = await _collection.FindAsync(o => o.Name != null && o.Name.Contains(nameFilter));
            return await cursor.ToListAsync();
        }

        public async Task Update(Entity entity)
        {
            await _collection.FindOneAndReplaceAsync(e => e.Id == entity.Id, entity);
        }
    }
}
