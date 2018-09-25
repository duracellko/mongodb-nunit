using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoRepo;
using Moq;
using NUnit.Framework;

namespace MongoRepoTests
{
    [TestFixture]
    public class EntityRepositoryTest
    {
        [Test]
        public async Task Get_user1_ReturnsUser()
        {
            var collection = new Mock<IMongoCollection<Entity>>(MockBehavior.Strict);
            var entities = SampleEntities();
            var entity = entities[1];
            collection.Setup(o => o.FindAsync(It.IsAny<FilterDefinition<Entity>>(), It.IsAny<FindOptions<Entity, Entity>>(), default(CancellationToken)))
                .Returns<FilterDefinition<Entity>, FindOptions<Entity, Entity>, CancellationToken>((f, o, c) => Task.FromResult(entities.ToAsyncCursor(((ExpressionFilterDefinition<Entity>)f).Expression)));

            var target = new EntityRepository(collection.Object);

            var result = await target.Get("user1");

            collection.Verify(o => o.FindAsync(It.IsAny<ExpressionFilterDefinition<Entity>>(), default(FindOptions<Entity, Entity>), default(CancellationToken)));
            Assert.AreEqual(entity, result);
        }

        [Test]
        public async Task GetByName_John_Returns2Users()
        {
            var collection = new Mock<IMongoCollection<Entity>>(MockBehavior.Strict);
            var entities = SampleEntities();
            collection.Setup(o => o.FindAsync(It.IsAny<FilterDefinition<Entity>>(), It.IsAny<FindOptions<Entity, Entity>>(), default(CancellationToken)))
                .Returns<FilterDefinition<Entity>, FindOptions<Entity, Entity>, CancellationToken>((f, o, c) => Task.FromResult(entities.ToAsyncCursor(((ExpressionFilterDefinition<Entity>)f).Expression)));

            var target = new EntityRepository(collection.Object);

            var result = await target.GetByName("John");

            collection.Verify(o => o.FindAsync(It.IsAny<ExpressionFilterDefinition<Entity>>(), default(FindOptions<Entity, Entity>), default(CancellationToken)));
            var expectedEntities = new Entity[] { entities[0], entities[3] };
            CollectionAssert.AreEqual(expectedEntities, result);
        }

        [Test]
        public async Task GetByName_Frank_Returns0Users()
        {
            var collection = new Mock<IMongoCollection<Entity>>(MockBehavior.Strict);
            var entities = SampleEntities();
            collection.Setup(o => o.FindAsync(It.IsAny<FilterDefinition<Entity>>(), It.IsAny<FindOptions<Entity, Entity>>(), default(CancellationToken)))
                .Returns<FilterDefinition<Entity>, FindOptions<Entity, Entity>, CancellationToken>((f, o, c) => Task.FromResult(entities.ToAsyncCursor(((ExpressionFilterDefinition<Entity>)f).Expression)));

            var target = new EntityRepository(collection.Object);

            var result = await target.GetByName("Frank");

            collection.Verify(o => o.FindAsync(It.IsAny<ExpressionFilterDefinition<Entity>>(), default(FindOptions<Entity, Entity>), default(CancellationToken)));
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public async Task Update_user1_ReplacesUser()
        {
            var collection = new Mock<IMongoCollection<Entity>>(MockBehavior.Strict);
            var entities = SampleEntities();
            var entity = new Entity
            {
                Id = "user1",
                Name = "Frank"
            };

            FilterDefinition<Entity> filter = null;
            collection.Setup(o => o.FindOneAndReplaceAsync(It.IsAny<FilterDefinition<Entity>>(), entity, It.IsAny<FindOneAndReplaceOptions<Entity, Entity>>(), default(CancellationToken)))
                .Callback<FilterDefinition<Entity>, Entity, FindOneAndReplaceOptions<Entity, Entity>, CancellationToken>((f, e, o, c) => filter = f)
                .ReturnsAsync(entity);

            var target = new EntityRepository(collection.Object);

            await target.Update(entity);

            collection.Verify(o => o.FindOneAndReplaceAsync(It.IsAny<ExpressionFilterDefinition<Entity>>(), entity, It.IsAny<FindOneAndReplaceOptions<Entity, Entity>>(), default(CancellationToken)));
            Assert.IsNotNull(filter);
            Assert.IsInstanceOf<ExpressionFilterDefinition<Entity>>(filter);
            var expressionFilter = (ExpressionFilterDefinition<Entity>)filter;
            var replacedEntities = entities.AsQueryable().Where(expressionFilter.Expression);
            var expectedEntities = new Entity[] { entities[1] };
            CollectionAssert.AreEqual(expectedEntities, replacedEntities);
        }

        private static List<Entity> SampleEntities()
        {
            return new List<Entity>
            {
                new Entity
                {
                    Id = "user",
                    Name = "John"
                },
                new Entity
                {
                    Id = "user1",
                    Name = "Bob"
                },
                new Entity
                {
                    Id = "user2",
                },
                new Entity
                {
                    Id = "user10",
                    Name = "John"
                }
            };
        }
    }
}
