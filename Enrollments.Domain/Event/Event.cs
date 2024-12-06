using Enrollments.Domain.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Enrollments.Domain.Event
{
    [BsonCollection("event")]
    [BsonIgnoreExtraElements]
    public class Event : GenericEntity
    {
        [BsonIgnoreIfNull]
        public Guid? CohortId { get; set; }

        [BsonIgnoreIfNull]
        public Guid? TrainingProgramId { get; set; }

        [BsonIgnoreIfNull]
        public UInt32? OrganizerId { get; set; }

        [BsonIgnoreIfNull]
        [EventTypeValidator]
        public string Type { get; set; }

        [BsonIgnoreIfNull]
        [StringLength(255, ErrorMessage = "The value is too long. Maximum allowed length is 255.")]
        public string Title { get; set; }

        [BsonIgnoreIfNull]
        [StringLength(65535, ErrorMessage = "The value is too long. Maximum allowed length is 65535.")]
        public string Objectives { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? StartAt { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? EndAt { get; set; }

        [BsonIgnoreIfNull]
        [StringLength(255, ErrorMessage = "The value is too long. Maximum allowed length is 255.")]
        public string Timezone { get; set; }

        [BsonIgnoreIfNull]
        public EventLocation Location { get; set; }
    }
}
