using System;
using System.Collections.Generic;

namespace Enrollments.Domain
{
    public class TrainingProgramAPIRepresentation
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string LanguageTag { get; set; }

        public string ReferenceCode { get; set; }

        public int? EstimatedDuration { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Authors { get; set; }

        public List<int> OrganizationIds { get; set; }

        public List<Guid> ProductIds { get; set; }

        public string ThumbnailPath { get; set; }

        public bool IsDiscoverable { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
