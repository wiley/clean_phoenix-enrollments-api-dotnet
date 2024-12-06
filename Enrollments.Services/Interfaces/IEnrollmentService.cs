using Enrollments.Domain.Enrollment;
using Enrollments.Domain.Pagination;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrollments.Services.Interfaces
{
    public interface IEnrollmentService
    {
        int TotalFound { get; }

        Task<List<Enrollment>> GetAllEnrollments(PageRequest request, string trainingProgramId = null, int? userId = null);

        Enrollment GetEnrollment(Guid id);

        Task CreateEnrollment(Enrollment enrollment);

        Task<Enrollment> UpdateEnrollment(Guid Id, Enrollment enrollment);

        Enrollment DeleteEnrollment(Guid Id);

        void GenerateKafkaEvents();
    }
}
