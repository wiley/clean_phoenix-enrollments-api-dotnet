using Enrollments.Domain.Event;
using Enrollments.Domain.Pagination;
using Enrollments.Domain.Params;
using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Enrollments.UnitTest.MockData;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollments.UnitTest.Services
{
    public class EventServiceTest
    {
        private IEventService _service;
        private readonly Mock<IMongoRepository<Event>> _repository;
        private readonly Mock<ILogger<EventService>> _logger;
        private readonly Mock<IPaginationService<Event>>_paginationService;

        public EventServiceTest()
        {
            _repository = new Mock<IMongoRepository<Event>>();
            _logger = new Mock<ILogger<EventService>>();
            _paginationService = new Mock<IPaginationService<Event>>();
        }

        [Test]
        public async Task GetAllEventsTest()
        {
            List<Event> mockEvents = EventMockData.GetEventListData();
            Mock<PageRequest> _pageRequest = new Mock<PageRequest>();

            _paginationService.Setup(r => r.ApplyPaginationAsync(It.IsAny<IQueryable<Event>>(), _pageRequest.Object)).Returns(Task.FromResult(mockEvents));
            _service = new EventService(_repository.Object, _logger.Object, _paginationService.Object);

            List<Event> events = await _service.GetAllEvents(_pageRequest.Object, new EventFilterParams{});
            CollectionAssert.AreEqual(events, mockEvents);
        }

        [Test]
        public void GetEvent()
        {
            Guid id = Guid.Parse("dfe028b7-8a43-4582-9c4a-eca4db29df53");
            Event mockedEvent = EventMockData.GetEvent(id);

            _repository.Setup(r => r.FindById(id)).Returns(mockedEvent);
            _service = new EventService(_repository.Object, _logger.Object, _paginationService.Object);

            Event theEvent = _service.GetEvent(id);
            Assert.AreEqual(theEvent, mockedEvent);
        }

        [Test]
        public async Task CreateEvent()
        {
            Guid id = Guid.Parse("dfe028b7-8a43-4582-9c4a-eca4db29df53");
            Event mockedEvent = EventMockData.GetEvent(id);

            _repository.Setup(r => r.InsertOneAsync(It.IsAny<Event>())).Returns(Task.FromResult(mockedEvent));
            _service = new EventService(_repository.Object, _logger.Object, _paginationService.Object);

            await _service.CreateEvent(mockedEvent);
            Assert.AreEqual(mockedEvent, mockedEvent);
        }

        [Test]
        public async Task UpdateEvent()
        {
            Guid id = Guid.Parse("dfe028b7-8a43-4582-9c4a-eca4db29df53");
            Event mockedEvent = EventMockData.GetEvent(id);

            mockedEvent.CohortId = new Guid("5426a433-3987-437c-bb7f-6cc70d597798");
            mockedEvent.TrainingProgramId = new Guid("554db656-a0c6-4aaf-86f4-4c2a858ff0bd");
            mockedEvent.OrganizerId = 100005;
            mockedEvent.Type = "FACILITATED";
            mockedEvent.Title = "Updated Event Title";
            mockedEvent.Objectives = "Updated Event Objectives";
            mockedEvent.Timezone = "America/Santarem";
            mockedEvent.StartAt = DateTime.UtcNow;
            mockedEvent.EndAt = DateTime.UtcNow;
            mockedEvent.Location = new EventLocation();
            mockedEvent.Location.Description = "Test Location Description";
            mockedEvent.Location.Link = "https://teams.microsoft.com/l/meetup-join/19%3ameeting_Mzk2ZTk0ODMtZGE4ZC00ZDU3LWIwZGMtNjA1ZGQxNjRiZjNh%40thread.v2/0?context=%7b%22Tid%22%3a%2224fe244f-890e-46ef-be2f-a5202976b7a5%22%2c%22Oid%22%3a%220ac75e96-d666-4bac-9693-454209945a72%22%7d";
            mockedEvent.Location.Type = "MS_TEAMS";

            Event updatedEvent = mockedEvent;

            _repository.Setup(r => r.UpdateOneAsync(id, It.IsAny<UpdateDefinition<Event>>())).Returns(Task.FromResult(updatedEvent));
            _service = new EventService(_repository.Object, _logger.Object, _paginationService.Object);

            await _service.UpdateEvent(id, updatedEvent);

            Assert.True(mockedEvent.CohortId == updatedEvent.CohortId);
            Assert.True(mockedEvent.TrainingProgramId == updatedEvent.TrainingProgramId);
            Assert.True(mockedEvent.OrganizerId == updatedEvent.OrganizerId);
            Assert.True(mockedEvent.Type == updatedEvent.Type);
            Assert.True(mockedEvent.Title == updatedEvent.Title);
            Assert.True(mockedEvent.Objectives == updatedEvent.Objectives);
            Assert.True(mockedEvent.Timezone == updatedEvent.Timezone);
            Assert.True(mockedEvent.StartAt == updatedEvent.StartAt);
            Assert.True(mockedEvent.EndAt == updatedEvent.EndAt);
            Assert.True(mockedEvent.Location.Description == updatedEvent.Location.Description);
            Assert.True(mockedEvent.Location.Link == updatedEvent.Location.Link);
            Assert.True(mockedEvent.Location.Type == updatedEvent.Location.Type);
            Assert.GreaterOrEqual(updatedEvent.UpdatedAt, mockedEvent.UpdatedAt);
        }

        [Test]
        public void DeleteEvent()
        {
            Guid id = Guid.Parse("dfe028b7-8a43-4582-9c4a-eca4db29df53");
            Event mockedEvent = EventMockData.GetEvent(id);

            _repository.Setup(r => r.DeleteById(It.IsAny<Guid>())).Returns(mockedEvent);
            _service = new EventService(_repository.Object, _logger.Object, _paginationService.Object);

            Event theEvent = _service.DeleteEvent(id);
            Assert.AreEqual(theEvent, mockedEvent);
        }
    }
}
