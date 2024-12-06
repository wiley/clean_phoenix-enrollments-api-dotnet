using NUnit.Framework;
using System.Threading.Tasks;
using Moq;
using Enrollments.Domain;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using System.Net;
using System.Net.Http;
using Enrollments.UnitTest.MockData;
using Newtonsoft.Json;
using Enrollments.Domain.Enrollment;
using Microsoft.Extensions.Logging;
using System;

namespace Enrollments.UnitTest.Services
{
    public class TrainingProgramServiceTest
    {
        private ITrainingProgramService _service;
        private readonly Mock<IHttpClientFactory> _httpFactory;
        private readonly Mock<ILogger<TrainingProgramService>> _logger;

        public TrainingProgramServiceTest()
        {
            _httpFactory = new Mock<IHttpClientFactory>();
            _logger = new Mock<ILogger<TrainingProgramService>>();
        }

        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("TRAINING_PROGRAM_API_BASE_URL", "http://training-program-mock.com");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("TRAINING_PROGRAM_API_BASE_URL", null);
        }

        [Test]
        public async Task CheckIfTrainingProgramExists_Succeed()
        {
            TrainingProgramAPIRepresentation trainingProgramAPIRepresentation = TrainingProgramMockData.GetTrainingProgramAPIRepresentation();
            var fakeHttpClient = FakeHttpClient.GetFakeClientSimple(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(trainingProgramAPIRepresentation)
            );
            _httpFactory.Setup(httpFactory => httpFactory.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);


            _service = new TrainingProgramService(_httpFactory.Object, _logger.Object);

            bool exists = await _service.CheckIfTrainingProgramExists(It.IsAny<string>());
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task CheckIfTrainingProgramExists_Exception()
        {
            TrainingProgramAPIRepresentation trainingProgramAPIRepresentation = TrainingProgramMockData.GetTrainingProgramAPIRepresentation();
            var fakeHttpClient = FakeHttpClient.GetFakeClientSimple(
                HttpStatusCode.NotFound,
                ""
            );
            _httpFactory.Setup(httpFactory => httpFactory.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);


            _service = new TrainingProgramService(_httpFactory.Object, _logger.Object);

            bool exists = await _service.CheckIfTrainingProgramExists(It.IsAny<string>());
            Assert.IsFalse(exists);
        }

    }
}

