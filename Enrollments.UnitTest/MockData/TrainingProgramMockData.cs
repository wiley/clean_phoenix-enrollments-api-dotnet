using Enrollments.Domain;
using Enrollments.Domain.Enrollment;
using System;
using System.Collections.Generic;

namespace Enrollments.UnitTest.MockData
{
    public static class TrainingProgramMockData
    {
        public static TrainingProgramAPIRepresentation GetTrainingProgramAPIRepresentation()
        {
            return GenerateTrainingProgramAPIRepresentation(Guid.NewGuid());
        }

        private static TrainingProgramAPIRepresentation GenerateTrainingProgramAPIRepresentation(Guid id)
        {
            return new TrainingProgramAPIRepresentation
            {
                Id = id.ToString(),
                Title = "Leading From The Middle",
                Description = "This is a mocked training program for Unit Testing",
                LanguageTag = "en-US",
                ReferenceCode = "REF001",
                EstimatedDuration = 100,
                Tags = new List<string> { "tag1", "tag2" },
                Authors = new List<string> {
                    "Author 1 Family Name 1",
                    "Author 2 Family Name 2"
                },
                OrganizationIds = new List<int> { 1, 2, 3, 5 },
                ProductIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
            };
        }
    }
}
