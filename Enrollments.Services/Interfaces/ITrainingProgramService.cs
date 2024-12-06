using Enrollments.Domain.Enrollment;
using System.Threading.Tasks;

namespace Enrollments.Services.Interfaces
{
    public interface ITrainingProgramService
    {
        Task<bool> CheckIfTrainingProgramExists(string id);
    }
}
