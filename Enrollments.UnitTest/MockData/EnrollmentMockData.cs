using System;
using System.Collections.Generic;
using Enrollments.Domain.Enrollment;

namespace Enrollments.UnitTest.MockData {
    public static class EnrollmentMockData
    {
        public static List<Enrollment> GetEnrollmentListData()
        {
            List<Enrollment> enrollments = new List<Enrollment>();
            enrollments.Add(GenerateEnrollment(Guid.Parse("3fa35f64-5717-4562-b3fc-2c963f66afa6")));
            enrollments.Add(GenerateEnrollment(Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a")));

            return enrollments;
        }

        public static Enrollment GetEnrollment(Guid id) {
            return GenerateEnrollment(id);
        }

        private static Enrollment GenerateEnrollment(Guid id) {
        return new Enrollment
            {

                Id = id,
                CreatedBy = 0,
                CreatedAt = DateTime.Parse("2023-02-22T21:40:18.243Z"),
                UpdatedAt = DateTime.Parse("2023-02-22T21:40:18.243Z"),
                UserId = 0,
                TrainingProgramId = Guid.NewGuid(),
                OrganizationId = 0,
                Type = "MANDATORY",
                DueAt = DateTime.Parse("2023-02-22T21:40:18.243Z")
            };
        }
    }
}
