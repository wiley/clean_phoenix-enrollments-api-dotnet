using Enrollments.Domain;
using System.Threading.Tasks;

namespace Enrollments.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RequestHealthCheck();

        Task<bool> CheckIfUserExists(int id);
    }
}
