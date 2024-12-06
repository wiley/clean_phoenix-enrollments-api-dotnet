using Enrollments.Domain ;
using Enrollments.Domain.Enrollment;
using Enrollments.Domain.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Enrollments.API.Requests.Enrollment
{
    public class EnrollmentCreateRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public Guid TrainingProgramId { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Required]
        [EnrollmentTypeValidator]
        public string Type { get; set; }

        public DateTime? DueAt { get; set; }
    }
}


