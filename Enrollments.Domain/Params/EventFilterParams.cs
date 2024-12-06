using System;

namespace Enrollments.Domain.Params
{
    public class EventFilterParams
    {
        public UInt32? OrganizerId { get; set; } = null;

        public Guid? CohortId { get; set; } = null;

        public Guid? TrainingProgramId { get; set; } = null;

        public DateTime? StartAtGreaterThan { get; set; } = null;

        public DateTime? StartAtGreaterThanEqual { get; set; } = null;

        public DateTime? StartAtLowerThan { get; set; } = null;

        public DateTime? StartAtLowerThanEqual { get; set; } = null;

        public DateTime? StartAtEqual { get; set; } = null;

        public DateTime? EndAtGreaterThan { get; set; } = null;

        public DateTime? EndAtGreaterThanEqual { get; set; } = null;

        public DateTime? EndAtLowerThan { get; set; } = null;

        public DateTime? EndAtLowerThanEqual { get; set; } = null;

        public DateTime? EndAtEqual { get; set; } = null;
    }
}
