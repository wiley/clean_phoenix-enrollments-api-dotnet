using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Enrollments.Domain.Interfaces
{
    public interface IDocument
    {
        [BsonRepresentation(BsonType.String)]
        [BsonId(IdGenerator = typeof(GuidGenerator))]

        Guid Id { get; set; }
    }
}