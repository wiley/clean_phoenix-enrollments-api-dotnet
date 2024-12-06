using Enrollments.Infrastructure.Interface.Mongo;

namespace Enrollments.Infrastructure.Mongo
{
    public class MongoDBSettings : IMongoDBSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}