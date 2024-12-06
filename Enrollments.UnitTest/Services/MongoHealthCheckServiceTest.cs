using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Moq;
using NUnit.Framework;
using System;

namespace Enrollments.UnitTest.Services
{
    public class MongoHealthCheckServiceTest
    {
        private readonly IMongoHealthCheckService _service;
        private readonly Mock<IMongoTestConnection> _mongoTestConnection;

        public MongoHealthCheckServiceTest()
        {
            _mongoTestConnection = new Mock<IMongoTestConnection>();
            _service = new MongoHealthCheckService(_mongoTestConnection.Object);
        }

        [SetUp]
        public void Setup()
        {
            _mongoTestConnection.Setup(s => s.Test()).Returns(true);
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
            _mongoTestConnection.Setup(s => s.Test()).Returns(false);
            var isAlive = _service.IsAlive();
            Assert.IsFalse(isAlive);
        }

        [Test]
        public void TestConnection_Exception()
        {
            _mongoTestConnection.Setup(s => s.Test()).Throws(new Exception());
            var isAlive = _service.IsAlive();
            Assert.IsFalse(isAlive);
        }
    }
}