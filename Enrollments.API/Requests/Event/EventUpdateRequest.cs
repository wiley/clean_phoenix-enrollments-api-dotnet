using Enrollments.Domain.Event;
using System;

namespace Enrollments.API.Requests.Event
{
    public class EventUpdateRequest
    {
        public string CohortId { get; set; }

        public string TrainingProgramId { get; set; }

        public UInt32 OrganizerId { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string Objectives { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public string Timezone { get; set; }

        public EventLocation Location { get; set; }
    }
}
