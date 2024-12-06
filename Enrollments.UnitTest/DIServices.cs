using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Enrollments.UnitTest
{
    public class DIServices
    {
        public ServiceProvider GenerateDependencyInjection()
        {
            var services = new ServiceCollection();
            services.AddScoped(typeof(IPaginationService<>), typeof(PaginationService<>));

            return services
                .AddLogging()
                .BuildServiceProvider();
        }
    }
}