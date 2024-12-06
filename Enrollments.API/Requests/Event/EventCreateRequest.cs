using Enrollments.Domain.Event;
using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Enrollments.API.Requests.Event
{
    public class EventCreateRequest
    {
        [Required]
        public string CohortId { get; set; }

        [Required]
        public string TrainingProgramId { get; set; }

        [Required]
        public UInt32 OrganizerId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Title { get; set; }

        public string Objectives { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public string Timezone { get; set; }

        public EventLocation Location { get; set; }
    }
}
