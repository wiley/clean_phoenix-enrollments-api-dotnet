using Enrollments.Domain.Enrollment;
using System.Threading.Tasks;

namespace Enrollments.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<bool> CheckIfOrganizationExists(int id);
    }
}
