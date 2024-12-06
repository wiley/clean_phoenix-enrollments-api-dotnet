using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Enrollments.Domain.Enrollment
{
    [BsonCollection("enrollment")]
    [BsonIgnoreExtraElements]
    public class Enrollment : GenericEntity
    {
        [BsonIgnoreIfNull]
        public int UserId { get; set; }

        [BsonIgnoreIfNull]
        public Guid TrainingProgramId { get; set; }

        [BsonIgnoreIfNull]
        public int OrganizationId { get; set; }

        [BsonIgnoreIfNull]
        public string Type { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? DueAt { get; set; }

    }
}
