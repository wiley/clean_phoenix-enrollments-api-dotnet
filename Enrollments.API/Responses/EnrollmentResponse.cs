using System;
using Enrollments.Domain;
using Enrollments.Domain.Enrollment;
using Newtonsoft.Json;

namespace Enrollments.API.Responses
{
    public class EnrollmentResponse: GenericEntityResponse
    {
        public int UserId { get; set; }

        public Guid TrainingProgramId { get; set; }

        public int OrganizationId { get; set; }        

        public string Type { get; set; }

        public DateTime DueAt { get; set; }        
    }
}
