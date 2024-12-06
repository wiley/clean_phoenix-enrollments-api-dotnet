using System;
using Enrollments.Domain.Event;

namespace Enrollments.API.Responses
{
    public class EventResponse: GenericEntityResponse
    {
        public Guid CohortId { get; set; }

        public Guid TrainingProgramId { get; set; }

        public UInt32 OrganizerId { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string Objectives { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public string Timezone { get; set; }

        public EventLocation Location { get; set; }
    }
}
