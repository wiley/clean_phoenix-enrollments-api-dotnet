using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyAPI.Domain.Exceptions;
using DarwinAuthorization.Models;
using Enrollments.Domain.Enrollment;
using Enrollments.Domain.KafkaMessage;
using Enrollments.Domain.Pagination;
using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Enrollments.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IMongoRepository<Enrollment> _enrollmentRepository;
        private readonly IPaginationService<Enrollment> _paginationService;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly DarwinAuthorizationContext _authorizationContext;

        public EnrollmentService(
            IMongoRepository<Enrollment> enrollmentRepository,
            ILogger<EnrollmentService> logger,
            IPaginationService<Enrollment> paginationService,
            IKafkaService kafkaService,
            DarwinAuthorizationContext authorizationContext)
        {
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
            _paginationService = paginationService;
            _kafkaService = kafkaService;
            _authorizationContext = authorizationContext;

            CreateIndexes();
        }

        public int TotalFound => _paginationService.TotalRecords;

        public Task<List<Enrollment>> GetAllEnrollments(PageRequest request, string trainingProgramId = null, int? userId = null)
        {
            IQueryable<Enrollment> enrollments = _enrollmentRepository.AsQueryable();
            if (trainingProgramId != null)
                enrollments = enrollments.Where(e => e.TrainingProgramId == new Guid(trainingProgramId));
            if (userId != null)
                enrollments = enrollments.Where(e => e.UserId == userId);

            return _paginationService.ApplyPaginationAsync(enrollments, request);
        }

        public Enrollment GetEnrollment(Guid id)
        {
            return _enrollmentRepository.FindById(id);
        }

        public async Task CreateEnrollment(Enrollment enrollment)
        {
            enrollment.CreatedAt = DateTime.Now;
            enrollment.UpdatedAt = DateTime.Now;
            enrollment.CreatedBy = (uint) _authorizationContext.UserId;
            enrollment.UpdatedBy = (uint) _authorizationContext.UserId;

            try
            {
                await _enrollmentRepository.InsertOneAsync(enrollment);
                _kafkaService.SendKafkaMessage(enrollment.Id.ToString(), "EnrollmentUpdated", enrollment);
            }
            catch (MongoWriteException exception)
            {
                if (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    _logger.LogInformation(exception, "CreateEnrollment - Enrollment already exists");
                    throw new ResourceAlreadyExistsException();
                }

                _logger.LogError(exception, "CreateEnrollment - Failed to write on database");
                throw;
            }

            _logger.LogInformation("Inserted Enrollment - {enrollment}", enrollment);
        }

        public async Task<Enrollment> UpdateEnrollment(Guid id, Enrollment enrollment)
        {
            var update = Builders<Enrollment>.Update.Set(p => p.Id, id);

            if (!string.IsNullOrWhiteSpace(enrollment.Type))
                update = update.Set(e => e.Type, enrollment.Type);

            if (enrollment.DueAt != null)
                update = update.Set(e => e.DueAt, enrollment.DueAt);

            update = update.Set(e => e.UpdatedAt, DateTime.UtcNow)
                .Set(e => e.UpdatedBy, (uint) _authorizationContext.UserId);

            await _enrollmentRepository.UpdateOneAsync(id, update);
            enrollment = GetEnrollment(id);
            _logger.LogInformation("Updated Enrollment Data - {update}", update);

            _kafkaService.SendKafkaMessage(enrollment.Id.ToString(), "EnrollmentUpdated", enrollment);

            return enrollment;
        }

        public Enrollment DeleteEnrollment(Guid id)
        {
            var result = _enrollmentRepository.DeleteById(id);

            _logger.LogInformation("Deleted Enrollment Data - {result}", result);

            if (null != result)
            {
                _kafkaService.SendKafkaMessage(id.ToString(), "EnrollmentRemoved", new EnrollmentRemoved
                {
                    Id = result.Id,
                    UpdatedBy = (uint) _authorizationContext.UserId
                });
            }

            return result;
        }

        public void GenerateKafkaEvents()
        {
            _kafkaService.GenerateKafkaEvents();
        }

        private void CreateIndexes()
        {
            var indexKeysDefinition = Builders<Enrollment>.IndexKeys
                .Ascending(e => e.UserId)
                .Ascending(e => e.OrganizationId)
                .Ascending(e => e.TrainingProgramId);

            CreateIndexOptions options = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Enrollment>(indexKeysDefinition, options);

            IMongoCollection<Enrollment> collection = _enrollmentRepository.getCollection();
            collection.Indexes.CreateOne(indexModel);
        }
    }
}
