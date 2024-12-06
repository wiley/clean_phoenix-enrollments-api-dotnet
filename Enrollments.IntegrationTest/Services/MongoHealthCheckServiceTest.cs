using Enrollments.Infrastructure.Mongo;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NUnit.Framework;

namespace Enrollments.IntegrationTest.Services
{
    public class MongoHealthCheckServiceTest
    {
        private readonly IMongoHealthCheckService _service;
        private readonly ServiceProvider _services;

        public MongoHealthCheckServiceTest()
        {
            _services = new DIServices().GenerateDependencyInjection();
            _service = _services.GetService<IMongoHealthCheckService>();
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConnection_Succeed()
        {
            var isAlive = _service.IsAlive();
            Assert.IsTrue(isAlive);
        }

        [Test]
        public void TestConnection_Failed()
        {
            var svc = new MongoHealthCheckService(new MongoTestConnection(new MongoDBSettings(), _services.GetService<IMongoClient>()));
            var isAlive = svc.IsAlive();
            Assert.IsFalse(isAlive);
        }
    }
}