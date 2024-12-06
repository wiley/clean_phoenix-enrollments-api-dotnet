using System;

namespace Enrollments.Domain.Event
{
    public interface IEvent
    {
        Guid? CohortId { get; set; }

        Guid? TrainingProgramId { get; set; }

        UInt32? OrganizerId { get; set; }

        string Type { get; set; }

        string Title { get; set; }

        string Objectives { get; set; }

        DateTime? StartAt { get; set; }

        DateTime? EndAt { get; set; }

        string Timezone { get; set; }

        EventLocation Location { get; set; }
    }
}
