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
    public class OrganizationServiceTest
    {
        private IOrganizationService _service;
        private readonly Mock<IHttpClientFactory> _httpFactory;
        private readonly Mock<ILogger<OrganizationService>> _logger;

        public OrganizationServiceTest()
        {
            _httpFactory = new Mock<IHttpClientFactory>();
            _logger = new Mock<ILogger<OrganizationService>>();
        }

        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("COMPANY_API_BASE_URL", "http://company-mock.com");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("COMPANY_API_BASE_URL", null);
        }

        [Test]
        public async Task CheckIfOrganizationExists_Succeed()
        {
            OrganizationAPIRepresentation organizationAPIRepresentation = OrganizationMockData.GetOrganizationAPIRepresentation();
            var fakeHttpClient = FakeHttpClient.GetFakeClientSimple(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(organizationAPIRepresentation)
            );
            _httpFactory.Setup(httpFactory => httpFactory.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);


            _service = new OrganizationService(_httpFactory.Object, _logger.Object);

            bool exists = await _service.CheckIfOrganizationExists(It.IsAny<int>());
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task CheckIfOrganizationExists_Exception()
        {
            OrganizationAPIRepresentation organizationAPIRepresentation = OrganizationMockData.GetOrganizationAPIRepresentation();
            var fakeHttpClient = FakeHttpClient.GetFakeClientSimple(
                HttpStatusCode.NotFound,
                ""
            );
            _httpFactory.Setup(httpFactory => httpFactory.CreateClient(It.IsAny<string>())).Returns(fakeHttpClient);


            _service = new OrganizationService(_httpFactory.Object, _logger.Object);

            bool exists = await _service.CheckIfOrganizationExists(It.IsAny<int>());
            Assert.IsFalse(exists);
        }

    }
}

