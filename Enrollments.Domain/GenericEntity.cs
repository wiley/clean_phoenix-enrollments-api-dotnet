using Enrollments.Domain.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace Enrollments.Domain
{
    public abstract class GenericEntity : IDocument
    {
        [BsonId(IdGenerator = typeof(GuidGenerator))]
        public Guid Id { get; set; }

        [BsonRequired]

        public DateTime CreatedAt { get; set; }

        public UInt32 CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        public UInt32 UpdatedBy { get; set; }

        public GenericEntity(){
            CreatedBy = 0;
            UpdatedBy = 0;
        }
    }
}
