using NUnit.Framework;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Enrollments.Domain.Enrollment;
using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Domain.Pagination;
using Enrollments.UnitTest.MockData;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using System.Linq;
using CompanyAPI.Domain.Exceptions;
using MongoDB.Bson;
using System.Runtime.InteropServices;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.Clusters;
using System.Net;
using System.Reflection;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.Runtime.Serialization;
using NSubstitute;

namespace Enrollments.UnitTest.Services
{
    public class EnrollmentServiceTest
    {
        private IEnrollmentService _service;
        private readonly Mock<IMongoRepository<Enrollment>> _repository;    
        private readonly Mock<ILogger<EnrollmentService>> _logger;
        private readonly Mock<IPaginationService<Enrollment>>_paginationService;
        private readonly IKafkaService _kafkaService;

        public EnrollmentServiceTest()
        {
            _repository = new Mock<IMongoRepository<Enrollment>>();
            _logger = new Mock<ILogger<EnrollmentService>>();
            _paginationService = new Mock<IPaginationService<Enrollment>>();
            _kafkaService = Substitute.For<IKafkaService>();

            this.configureMockRepository();
        }

        [Test]
        public async Task GetAllEnrollmentsTest()
        {
            List<Enrollment> mockEnrollments = EnrollmentMockData.GetEnrollmentListData();
            Mock<PageRequest> _pageRequest = new Mock<PageRequest>();

            _paginationService.Setup(r => r.ApplyPaginationAsync(It.IsAny<IQueryable<Enrollment>>(), _pageRequest.Object)).Returns(Task.FromResult(mockEnrollments));
            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            List<Enrollment> enrollments = await _service.GetAllEnrollments(_pageRequest.Object);
            CollectionAssert.AreEqual(enrollments, mockEnrollments);
        }

        [Test]
        public void GetEnrollment()
        {
            Guid id = Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a");
            Enrollment mockEnrollment = EnrollmentMockData.GetEnrollment(id);

            _repository.Setup(r => r.FindById(id)).Returns(mockEnrollment);
            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            Enrollment enrollment = _service.GetEnrollment(id);
            Assert.AreEqual(enrollment, mockEnrollment);
        }

        [Test]
        public async Task CreateEnrollment_Success()
        {
            Guid id = Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a");
            Enrollment mockEnrollment = EnrollmentMockData.GetEnrollment(id);
            
            _repository.Setup(r => r.InsertOneAsync(It.IsAny<Enrollment>())).Returns(Task.FromResult(mockEnrollment));
            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            await _service.CreateEnrollment(mockEnrollment);
            Assert.AreEqual(mockEnrollment, mockEnrollment);
            _kafkaService.Received(1).SendKafkaMessage(Arg.Any<string>(), "EnrollmentCreated", Arg.Any<object>());    
        }
                
        [Test]
        public async Task CreateEnrollment_ThrowsException_ResourceAlreadyExistException()
        {
            Guid id = Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a");
            Enrollment mockEnrollment = EnrollmentMockData.GetEnrollment(id);

            this.prepareMockToThrowException(ServerErrorCategory.DuplicateKey);

            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            Assert.ThrowsAsync<ResourceAlreadyExistsException>(() => _service.CreateEnrollment(mockEnrollment));
            _kafkaService.Received(1).SendKafkaMessage(Arg.Any<string>(), "EnrollmentCreated", Arg.Any<object>());
        }

        [Test]
        public async Task CreateEnrollment_ThrowsException_OtherThanResourceAlreadyExistException() {
            Guid id = Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a");
            Enrollment mockEnrollment = EnrollmentMockData.GetEnrollment(id);

            this.prepareMockToThrowException(ServerErrorCategory.ExecutionTimeout);

            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            Assert.ThrowsAsync<MongoWriteException>(() => _service.CreateEnrollment(mockEnrollment));
            _kafkaService.Received(1).SendKafkaMessage(Arg.Any<string>(), "EnrollmentCreated", Arg.Any<object>());
        }

        [Test]
        public async Task UpdateEnrollment()
        {
            Guid id = Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a");
            Enrollment mockEnrollment = EnrollmentMockData.GetEnrollment(id);

            mockEnrollment.UserId = 5;
            mockEnrollment.TrainingProgramId = Guid.NewGuid();
            mockEnrollment.OrganizationId = 0;
            mockEnrollment.Type = "RECOMMENDED";
            mockEnrollment.DueAt = DateTime.UtcNow;

            Enrollment updatedEnrollment = mockEnrollment;

            _repository.Setup(r => r.UpdateOneAsync(id, It.IsAny<UpdateDefinition<Enrollment>>())).Returns(Task.FromResult(updatedEnrollment));
            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            await _service.UpdateEnrollment(id, updatedEnrollment);
            Assert.True(mockEnrollment.UserId == updatedEnrollment.UserId);
            Assert.True(mockEnrollment.TrainingProgramId == updatedEnrollment.TrainingProgramId);
            Assert.True(mockEnrollment.OrganizationId == updatedEnrollment.OrganizationId);
            Assert.True(mockEnrollment.Type == updatedEnrollment.Type);
            Assert.True(mockEnrollment.DueAt == updatedEnrollment.DueAt);
            Assert.GreaterOrEqual(updatedEnrollment.UpdatedAt, mockEnrollment.UpdatedAt);
            _kafkaService.Received(1).SendKafkaMessage(Arg.Any<string>(), "EnrollmentUpdated", Arg.Any<object>());
        }

        [Test]
        public void DeleteEnrollment()
        {
            Guid id = Guid.Parse("c3008b5b-0a73-4def-8496-ea850f154d9a");
            Enrollment mockEnrollment = EnrollmentMockData.GetEnrollment(id);

            _repository.Setup(r => r.DeleteById(It.IsAny<Guid>())).Returns(mockEnrollment);
            _service = new EnrollmentService(_repository.Object, _logger.Object, _paginationService.Object, _kafkaService);

            Enrollment enrollment = _service.DeleteEnrollment(id);
            Assert.AreEqual(enrollment, mockEnrollment);
            _kafkaService.Received(1).SendKafkaMessage(Arg.Any<string>(), "EnrollmentDeleted", Arg.Any<object>());
        }

        private void configureMockRepository()
        {
            Mock<IMongoCollection<Enrollment>> collection = new Mock<IMongoCollection<Enrollment>>();
            _repository.Setup(r => r.getCollection()).Returns(collection.Object);

            Mock<IMongoIndexManager<Enrollment>> indexes = new Mock<IMongoIndexManager<Enrollment>>();
            collection.Setup(c => c.Indexes).Returns(indexes.Object);
        }

        private void prepareMockToThrowException(ServerErrorCategory serverErrorCategory) {
            ClusterId clusterId = new ClusterId();
            Mock<EndPoint> endPoint = new Mock<EndPoint>();
            ServerId serverId = new ServerId(clusterId, endPoint.Object);
            ConnectionId connectionId = new ConnectionId(serverId);

            Mock<WriteError> writeError = new Mock<WriteError>(serverErrorCategory, 1, "", new BsonDocument());
            MongoWriteException mongoWriteException = new MongoWriteException(connectionId, writeError.Object, null, null);

            _repository.Setup(r => r.InsertOneAsync(It.IsAny<Enrollment>())).Throws(mongoWriteException);
        }
    }
}
