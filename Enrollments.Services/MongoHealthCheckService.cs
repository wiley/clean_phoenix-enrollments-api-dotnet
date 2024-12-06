using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Services.Interfaces;

namespace Enrollments.Services
{
    public class MongoHealthCheckService : IMongoHealthCheckService
    {
        private readonly IMongoTestConnection _mongoTestConnection;

        public MongoHealthCheckService(IMongoTestConnection mongoTestConnection)
        {
            _mongoTestConnection = mongoTestConnection;
        }

        public bool IsAlive()
        {
            try
            {
                return _mongoTestConnection.Test();
            }
            catch
            {
                return false;
            }
        }
    }
}