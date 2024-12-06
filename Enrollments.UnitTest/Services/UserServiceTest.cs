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
using Microsoft.Extensions.Logging;
using System;

namespace Enrollments.UnitTest.Services
{
    public class UserServiceTest
    {
        private IUserService _service;
        private readonly Mock<IHttpClientFactory> _httpFactory;
        private readonly Mock<ILogger<UserService>> _logger;

        public UserServiceTest()
        {
            _httpFactory = new Mock<IHttpClientFactory>();
            _logger = new Mock<ILogger<UserService>>();
        }

        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("USERS_API_BASE_URL", "http://user-mock.com");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("USERS_API_BASE_URL", null);
        }

        [Test]
        public async Task CheckIfUserExists_Succeed()
        {
            UserAPIRepresentation userAPIRepresentation = UserMockData.GetUserAPIRepresentation();
            var fakeHttpClient = FakeHttpClient.GetFakeClientSimple(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(userAPIRepresentation)
            );
            _httpFactory.Setup(httpFactory => httpFactory.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);


            _service = new UserService(_httpFactory.Object, _logger.Object);

            bool exists = await _service.CheckIfUserExists(It.IsAny<int>());
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task CheckIfUserExists_Exception()
        {
            UserAPIRepresentation userAPIRepresentation = UserMockData.GetUserAPIRepresentation();
            var fakeHttpClient = FakeHttpClient.GetFakeClientSimple(
                HttpStatusCode.NotFound,
                ""
            );
            _httpFactory.Setup(httpFactory => httpFactory.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);


            _service = new UserService(_httpFactory.Object, _logger.Object);

            bool exists = await _service.CheckIfUserExists(It.IsAny<int>());
            Assert.IsFalse(exists);
        }
    }
}

