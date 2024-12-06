using System;

namespace Enrollments.Domain.KafkaMessage
{
    public class EnrollmentRemoved
    {
        public Guid Id { get; set; }
        public UInt32 UpdatedBy { get; set; }
    }
}
