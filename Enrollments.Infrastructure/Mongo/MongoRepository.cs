using Enrollments.Domain;
using Enrollments.Domain.Interfaces;
using Enrollments.Infrastructure.Interface.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Enrollments.Infrastructure.Mongo
{
    public class MongoRepository<TDocument> : IMongoRepository<TDocument>
        where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> _collection;
        public IMongoCollection<TDocument> Collection;
        private readonly IConnectionThrottlingPipeline _pipeline;

        public MongoRepository(IMongoDBSettings settings, IMongoClient client, IConnectionThrottlingPipeline pipeline)
        {
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
            _pipeline = pipeline;

            /*if(!BsonClassMap.IsClassMapRegistered(typeof(GenericEntity)))
                BsonClassMap.RegisterClassMap<GenericEntity>(
                    map =>
                    {
                        map.AutoMap();
                        map.MapProperty(x => x.Id).SetSerializer(new GuidSerializer(BsonType.String));
                    });*/
        }

        public IMongoCollection<TDocument> getCollection()
        {
            return _collection;
        }

        public async Task<Int64> GetGuid()
        {
            var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Gt("currentId", 0));
            var updates = Builders<TDocument>.Update.Inc("currentId", 1);
            var r = await _collection.FindOneAndUpdateAsync<BsonDocument>(filter, updates);

            return r["currentId"].ToInt64();
        }

        private protected string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                    typeof(BsonCollectionAttribute),
                    true)
                .FirstOrDefault())?.CollectionName;
        }

        public virtual IQueryable<TDocument> AsQueryable()
        {
            return _collection.AsQueryable();
        }

        public virtual IEnumerable<TDocument> FilterBy(
            FilterDefinition<TDocument> filterExpression,
            SortDefinition<TDocument> sortDefinition = null,
            int? offset = null,
            int? pageSize = null)
        {
            return _collection.Find(filterExpression).Sort(sortDefinition).Skip(offset).Limit(pageSize).ToEnumerable();
        }

        public virtual IEnumerable<TDocument> FilterBy(
            Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).ToEnumerable();
        }

        public virtual IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression)
        {
            return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
        }

        public virtual TDocument FindOne(FilterDefinition<TDocument> filterExpression)
        {
            return _collection.Find(filterExpression).FirstOrDefault();
        }

        public virtual Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _pipeline.AddRequest(_collection.Find(filterExpression).FirstOrDefaultAsync());
        }

        public virtual TDocument FindById(Guid id)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return _collection.Find(filter).SingleOrDefault();
        }

        public virtual Task<TDocument> FindByIdAsync(Guid id)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return _pipeline.AddRequest(_collection.Find(filter).SingleOrDefaultAsync());
        }

        public virtual void InsertOne(TDocument document)
        {
            _collection.InsertOne(document);
        }

        public virtual Task InsertOneAsync(TDocument document)
        {
            return _pipeline.AddRequest(_collection.InsertOneAsync(document));
        }

        public void InsertMany(ICollection<TDocument> documents)
        {
            _collection.InsertMany(documents);
        }

        public virtual Task InsertManyAsync(ICollection<TDocument> documents)
        {
            return _pipeline.AddRequest(_collection.InsertManyAsync(documents));
        }

        public void UpdateOne(Guid Id, UpdateDefinition<TDocument> document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, Id);
            _collection.UpdateOne(filter, document);
        }

        public virtual Task UpdateOneAsync(Guid Id, UpdateDefinition<TDocument> document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, Id);
            return _pipeline.AddRequest(_collection.UpdateOneAsync(filter, document));
        }

        public void ReplaceOne(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            _collection.FindOneAndReplace(filter, document);
        }

        public virtual Task ReplaceOneAsync(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            return _pipeline.AddRequest(_collection.FindOneAndReplaceAsync(filter, document));
        }

        public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            _collection.FindOneAndDelete(filterExpression);
        }

        public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _pipeline.AddRequest(_collection.FindOneAndDeleteAsync(filterExpression));
        }

        public TDocument DeleteById(Guid id)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return _collection.FindOneAndDelete(filter);
        }

        public Task DeleteByIdAsync(Guid id)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return _pipeline.AddRequest(_collection.FindOneAndDeleteAsync(filter));
        }

        public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
        {
            _collection.DeleteMany(filterExpression);
        }

        public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _pipeline.AddRequest(_collection.DeleteManyAsync(filterExpression));
        }
    }
}