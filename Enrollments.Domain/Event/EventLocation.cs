using Enrollments.Domain.Validators;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Enrollments.Domain.Event
{
    public class EventLocation {
        [BsonIgnoreIfNull]
        [EventLocationTypeValidator]
        public string Type { get; set; }

        [BsonIgnoreIfNull]
        [StringLength(65535, ErrorMessage = "The value is too long. Maximum allowed length is 65535.")]
        public string Description { get; set; }

        [BsonIgnoreIfNull]
        [StringLength(2048, ErrorMessage = "The value is too long. Maximum allowed length is 2048.")]
        public string Link { get; set; }
    }
}
