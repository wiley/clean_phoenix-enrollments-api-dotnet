using System;
using Enrollments.Domain;

namespace Enrollments.UnitTest.MockData {
    public static class OrganizationMockData
    {
        
        public static OrganizationAPIRepresentation GetOrganizationAPIRepresentation()
        {
            Random random = new();
            return GenerateOrganizationAPIRepresentation(random.Next());
        }

        private static OrganizationAPIRepresentation GenerateOrganizationAPIRepresentation(int id)
        {
            return new OrganizationAPIRepresentation
            {
                OrganizationId = id,
                OrganizationTypeId = 5972,
                CrunchbaseUuid = Guid.NewGuid().ToString(),
                OrganizationName = "Mocked Wiley Organization",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }
}
