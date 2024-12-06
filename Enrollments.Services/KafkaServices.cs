using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using WLS.KafkaMessenger.Services.Interfaces;
using Enrollments.Domain.Enrollment;
using Enrollments.Services.Interfaces;
using Enrollments.Infrastructure.Interface.Mongo;
using System.Linq.Dynamic.Core;

namespace Enrollments.Services
{
    public class KafkaService : IKafkaService
    {
        private readonly KafkaMessageChannel _channel;
        private readonly GenerateKafkaEventsMutex _mutex;

        public KafkaService(KafkaMessageChannel channel, GenerateKafkaEventsMutex mutex)
        {
            _channel = channel;
            _mutex = mutex;
        }

        public void SendKafkaMessage(string id, string subject, object data)
        {
            _channel.Send(new KafkaMessage { Id = id, Subject = subject, Data = data });
        }

        public void GenerateKafkaEvents()
        {
            try
            {
                _mutex.Release();
            }
            catch (SemaphoreFullException)
            {
                throw new GenerateKafkaEventsAlreadyRunningException();
            }
        }
    }

    public class GenerateKafkaEventsAlreadyRunningException : Exception
    {
    }

    public class KafkaMessage
    {
        public string Id;
        public string Subject;
        public object Data;
    }

    public class KafkaMessageChannel
    {
        private readonly Channel<KafkaMessage> _channel = Channel.CreateUnbounded<KafkaMessage>();

        public void Send(KafkaMessage message)
        {
            _channel.Writer.TryWrite(message);
        }

        public ChannelReader<KafkaMessage> Reader()
        {
            return _channel.Reader;
        }
    }

    public class SendKafkaMessageService : BackgroundService
    {
        private readonly ILogger<SendKafkaMessageService> _logger;
        private readonly IServiceScopeFactory _factory;
        private readonly KafkaMessageChannel _channel;

        public SendKafkaMessageService(ILogger<SendKafkaMessageService> logger, KafkaMessageChannel channel, IServiceScopeFactory factory)
        {
            _logger = logger;
            _channel = channel;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{instance} is running", this);
            string topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            using (var scope = _factory.CreateScope())
            {
                var messenger = scope.ServiceProvider.GetRequiredService<IKafkaMessengerService>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("{instance} is waiting for incoming messages", this);
                    while (await _channel.Reader().WaitToReadAsync(stoppingToken))
                    {
                        while (_channel.Reader().TryRead(out KafkaMessage message))
                        {

                            try
                            {
                                await messenger.SendKafkaMessage(message.Id, message.Subject, message.Data);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "{instance} error while sending the message - id:{id}, subject:{subject}, topic: {topic}", this, message.Id, message.Subject, topic);
                            }
                        }
                    }
                }
            }
            _logger.LogInformation("{instance} is shutting down", this);
        }
    }

    public class GenerateKafkaEventsMutex : SemaphoreSlim
    {
        public GenerateKafkaEventsMutex() : base(0, 1) // Limit the amount of concurrent jobs to 1
        {
        }
    }

    public class GenerateKafkaEventsService : BackgroundService
    {
        private readonly ILogger<GenerateKafkaEventsService> _logger;
        private readonly IServiceScopeFactory _factory;
        private readonly KafkaMessageChannel _channel;
        private readonly GenerateKafkaEventsMutex _mutex;

        public GenerateKafkaEventsService(ILogger<GenerateKafkaEventsService> logger, KafkaMessageChannel channel, GenerateKafkaEventsMutex mutex, IServiceScopeFactory factory)
        {
            _logger = logger;
            _factory = factory;
            _channel = channel;
            _mutex = mutex;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{instance} is running", this);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("{instance} is waiting for a signal", this);
                await _mutex.WaitAsync(stoppingToken);
                _mutex.Release(); // Re-acquire the lock immediately to prevent multiple GenerateKafkaEvents() tasks from running in parallel
                _logger.LogInformation("{instance} receieved a signal", this);

                try
                {
                    using var scope = _factory.CreateScope();
                    GenerateKafkaEvents(
                        scope.ServiceProvider.GetRequiredService<IMongoRepository<Enrollment>>()
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{insatnce} exception", this);
                }
                finally
                {
                    _mutex.Wait(); // Release the lock
                }
            }
            _logger.LogInformation("{instance} is shutting down", this);
        }

        private void GenerateKafkaEvents(IMongoRepository<Enrollment> repository)
        {
            var recordsProcessedCount = 0;
            var totalRecordsCount = 0;
            DateTime lastCreatedAt = DateTime.Now;
            try
            {
                var firstRecord = repository.AsQueryable()
                    .OrderBy(x => x.CreatedAt)
                    .Take(1)
                    .FirstOrDefault();
                if (null == firstRecord)
                {
                    _logger.LogInformation("GenerateKafkaEvents - Skipping, no records found");
                    return;
                }

                totalRecordsCount = repository.AsQueryable().Count();
                _logger.LogInformation(
                    "GenerateKafkaEvents - Start, first record created at: {createdAt}, total records count: {count}",
                    firstRecord.CreatedAt,
                    totalRecordsCount
                );
                _channel.Send(new KafkaMessage
                {
                    Id = firstRecord.Id.ToString(),
                    Subject = "EnrollmentUpdated",
                    Data = firstRecord
                });
                recordsProcessedCount++;
                lastCreatedAt = firstRecord.CreatedAt;
                List<Enrollment> enrollments;
                do
                {
                    enrollments = repository.AsQueryable()
                        .OrderBy(x => x.CreatedAt)
                        .Skip(recordsProcessedCount)
                        .Take(100)
                        .ToList();
                    foreach (var enrollment in enrollments)
                    {
                        _channel.Send(new KafkaMessage
                        {
                            Id = enrollment.Id.ToString(),
                            Subject = "EnrollmentUpdated",
                            Data = enrollment
                        });
                        lastCreatedAt = enrollment.CreatedAt;
                        recordsProcessedCount++;
                    }
                    _logger.LogDebug(
                        "GenerateKafkaEvents - Generate {count} events, {recordsProcessedCount} out of {totalRecordsCount}, ids: {firstId} -> {lastId}",
                        enrollments.Count,
                        recordsProcessedCount,
                        totalRecordsCount,
                        enrollments.FirstOrDefault()?.Id,
                        enrollments.LastOrDefault()?.Id
                    );
                }
                while (enrollments.Count > 0);
                _logger.LogInformation(
                    "GenerateKafkaEvents - End, last record created at: {createdAt}, processed {recordsProcessedCount} out of {totalRecordsCount}",
                    lastCreatedAt,
                    recordsProcessedCount,
                    totalRecordsCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    @"GenerateKafkaEvents - Error, last record: {lastCreatedAt},
                    records processed: {recordsProcessedCount}, expected number of records: {totalRecordsCount}",
                    lastCreatedAt,
                    recordsProcessedCount,
                    totalRecordsCount
                );
            }
        }
    }
}
