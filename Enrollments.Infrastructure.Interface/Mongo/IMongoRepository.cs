using Enrollments.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Enrollments.Infrastructure.Interface.Mongo
{
    public interface IMongoRepository<TDocument> where TDocument : IDocument
    {

        public IMongoCollection<TDocument> getCollection();

        Task<Int64> GetGuid();

        IQueryable<TDocument> AsQueryable();

        IEnumerable<TDocument> FilterBy(
            FilterDefinition<TDocument> filterExpression,
            SortDefinition<TDocument> sortDefinition = null,
            int? offset = null,
            int? pageSize = null);

        IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression);

        TDocument FindOne(FilterDefinition<TDocument> filterExpression);

        Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);

        TDocument FindById(Guid id);

        Task<TDocument> FindByIdAsync(Guid id);

        void InsertOne(TDocument document);

        Task InsertOneAsync(TDocument document);

        void InsertMany(ICollection<TDocument> documents);

        Task InsertManyAsync(ICollection<TDocument> documents);

        void UpdateOne(Guid Id, UpdateDefinition<TDocument> document);

        Task UpdateOneAsync(Guid Id, UpdateDefinition<TDocument> document);

        void ReplaceOne(TDocument document);

        Task ReplaceOneAsync(TDocument document);

        void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);

        Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression);

        TDocument DeleteById(Guid id);

        Task DeleteByIdAsync(Guid id);

        void DeleteMany(Expression<Func<TDocument, bool>> filterExpression);

        Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);
    }
}