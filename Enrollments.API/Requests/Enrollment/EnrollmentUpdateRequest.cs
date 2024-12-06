using Enrollments.Domain.Validators;
using System;

namespace Enrollments.API.Requests.Enrollment
{
    public class EnrollmentUpdateRequest
    {
        [EnrollmentTypeValidator]
        public string Type { get; set; }

        public DateTime? DueAt { get; set; }
    }
}
