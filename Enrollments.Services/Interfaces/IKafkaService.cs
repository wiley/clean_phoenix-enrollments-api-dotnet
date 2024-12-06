namespace Enrollments.Services.Interfaces
{
    public interface IKafkaService
    {
        void SendKafkaMessage(string id, string subject, object data);

        void GenerateKafkaEvents();
    }
}
