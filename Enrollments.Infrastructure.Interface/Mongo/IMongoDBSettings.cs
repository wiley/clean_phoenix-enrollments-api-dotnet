namespace Enrollments.Infrastructure.Interface.Mongo
{
    public interface IMongoDBSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}