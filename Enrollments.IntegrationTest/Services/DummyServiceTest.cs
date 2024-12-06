using Enrollments.Domain;
using Enrollments.Domain.Pagination;
using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrollments.IntegrationTest.Services
{
    public class DummyServiceTest
    {
        private readonly IDummyService _service;
        private readonly ServiceProvider _services;
        private readonly IMongoRepository<Dummy> _repository;
        private readonly ILogger<DummyService> _logService;
        private readonly IPaginationService<Dummy> _pagination;

        public DummyServiceTest()
        {
            _services = new DIServices().GenerateDependencyInjection();
            _repository = _services.GetService<IMongoRepository<Dummy>>();
            _logService = _services.GetService<ILogger<DummyService>>();
            _pagination = _services.GetService<IPaginationService<Dummy>>();
            _service = new DummyService(_repository, _logService, _pagination);
        }

        [SetUp]
        public void Setup()
        {
            var dummy = _service.LoadDummyData().Result;
            if (dummy.Count() == 0)
                _service.InsertDummyData();
        }

        [Test]
        public async Task LoadDummyData_DataShouldExist()
        {
            var dummy = await _service.LoadDummyData();
            Assert.IsNotNull(dummy);
        }

        [Test]
        public async Task FindDummyDataPagination_ShouldReturnData()
        {
            var filters = new List<Filter>();
            var request = new PageRequest()
            {
                PageOffset = 1,
                PageSize = 15,
                Filters = filters
            };
            var dummy = await _service.FindDummyData(request);
            Assert.IsNotNull(dummy);
            Assert.IsTrue(dummy.Count() >= 1);
        }

        [Test]
        public async Task FindDummyDataPagination_ShouldReturnNoData()
        {
            var filters = new List<Filter>();
            filters.Add(new Filter
            {
                FieldName = "Name",
                Values = new string[] { "ShouldNotExist" }
            });
            var request = new PageRequest()
            {
                PageOffset = 1,
                PageSize = 15,
                Filters = filters
            };
            var dummy = await _service.FindDummyData(request);
            Assert.IsNotNull(dummy);
            Assert.IsTrue(dummy.Count() == 0);
        }
    }
}