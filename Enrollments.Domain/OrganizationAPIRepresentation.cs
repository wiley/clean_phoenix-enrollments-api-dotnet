using System;

namespace Enrollments.Domain
{
    public class OrganizationAPIRepresentation
    {
        public int OrganizationId { get; set; }

        public int OrganizationTypeId { get; set; }

        public string CrunchbaseUuid { get; set; }

        public string OrganizationName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
