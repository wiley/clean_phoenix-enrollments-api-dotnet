using Enrollments.Domain.Event;
using Enrollments.Domain.Pagination;
using Enrollments.Domain.Params;
using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollments.Services
{
    public class EventService: IEventService
    {
        private readonly IMongoRepository<Event> _EventRepository;
        private readonly IPaginationService<Event> _paginationService;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IMongoRepository<Event> EventRepository,
            ILogger<EventService> logger,
            IPaginationService<Event> paginationService)
        {
            this._EventRepository = EventRepository;
            this._logger = logger;
            this._paginationService = paginationService;
        }

        public int TotalFound => _paginationService.TotalRecords;

        public Task<List<Event>> GetAllEvents(
            PageRequest request,
            EventFilterParams filterParams
        )
        {
            IQueryable<Event> events = _EventRepository.AsQueryable();

            if (filterParams.OrganizerId != null)
                events = events.Where(theEvent => theEvent.OrganizerId == filterParams.OrganizerId);
            if (filterParams.CohortId != null)
                events = events.Where(theEvent => theEvent.CohortId == filterParams.CohortId);
            if (filterParams.TrainingProgramId != null)
                events = events.Where(theEvent => theEvent.TrainingProgramId == filterParams.TrainingProgramId);
            if (filterParams.StartAtEqual != null)
                events = events.Where(theEvent => theEvent.StartAt == filterParams.StartAtLowerThanEqual);
            if (filterParams.StartAtGreaterThan != null)
                events = events.Where(theEvent => theEvent.StartAt > filterParams.StartAtGreaterThan);
            if (filterParams.StartAtGreaterThanEqual != null)
                events = events.Where(theEvent => theEvent.StartAt >= filterParams.StartAtGreaterThanEqual);
            if (filterParams.StartAtLowerThan != null)
                events = events.Where(theEvent => theEvent.StartAt < filterParams.StartAtLowerThan);
            if (filterParams.StartAtLowerThanEqual != null)
                events = events.Where(theEvent => theEvent.StartAt <= filterParams.StartAtLowerThanEqual);
            if (filterParams.EndAtEqual != null)
                events = events.Where(theEvent => theEvent.EndAt == filterParams.EndAtLowerThanEqual);
            if (filterParams.EndAtGreaterThan != null)
                events = events.Where(theEvent => theEvent.EndAt > filterParams.EndAtGreaterThan);
            if (filterParams.EndAtGreaterThanEqual != null)
                events = events.Where(theEvent => theEvent.EndAt >= filterParams.EndAtGreaterThanEqual);
            if (filterParams.EndAtLowerThan != null)
                events = events.Where(theEvent => theEvent.EndAt < filterParams.EndAtLowerThan);
            if (filterParams.EndAtLowerThanEqual != null)
                events = events.Where(theEvent => theEvent.EndAt <= filterParams.EndAtLowerThanEqual);

            return _paginationService.ApplyPaginationAsync(events, request);
        }

        public Event GetEvent(Guid Id)
        {
            return _EventRepository.FindById(Id);
        }

        public async Task CreateEvent(Event theEvent)
        {
            theEvent.CreatedAt = DateTime.Now;
            theEvent.UpdatedAt = DateTime.Now;
            await _EventRepository.InsertOneAsync(theEvent);

            _logger.LogInformation($"Inserted Event - {theEvent}");
        }

        public async Task<Event> UpdateEvent(Guid eventId, Event newEventData)
        {
            var update = Builders<Event>.Update.Set(existingEvent => existingEvent.Id, eventId);

            if (newEventData.CohortId != null)
                update = update.Set(existingEvent => existingEvent.CohortId, newEventData.CohortId);

            if (newEventData.TrainingProgramId != null)
                update = update.Set(existingEvent => existingEvent.TrainingProgramId, newEventData.TrainingProgramId);

            if (newEventData.OrganizerId != null)
                update = update.Set(existingEvent => existingEvent.OrganizerId, newEventData.OrganizerId);

            if (!string.IsNullOrWhiteSpace(newEventData.Type))
                update = update.Set(existingEvent => existingEvent.Type, newEventData.Type);

            if (!string.IsNullOrWhiteSpace(newEventData.Title))
                update = update.Set(existingEvent => existingEvent.Title, newEventData.Title);

            if (!string.IsNullOrWhiteSpace(newEventData.Objectives))
                update = update.Set(existingEvent => existingEvent.Objectives, newEventData.Objectives);

            if (newEventData.StartAt != null)
                update = update.Set(existingEvent => existingEvent.StartAt, newEventData.StartAt);

            if (newEventData.EndAt != null)
                update = update.Set(existingEvent => existingEvent.EndAt, newEventData.EndAt);

            if (!string.IsNullOrWhiteSpace(newEventData.Timezone))
                update = update.Set(existingEvent => existingEvent.Timezone, newEventData.Timezone);

            if (newEventData.Location != null)
                update = update.Set(existingEvent => existingEvent.Location, newEventData.Location);

            newEventData.UpdatedAt = DateTime.UtcNow;
            update = update.Set(existingEvent => existingEvent.UpdatedAt, newEventData.UpdatedAt);

            await _EventRepository.UpdateOneAsync(eventId, update);

            var updatedEvent = GetEvent(eventId);
            _logger.LogInformation($"Updated Event Data - {updatedEvent}");

            return updatedEvent;
        }

        public Event DeleteEvent(Guid Id)
        {
            return _EventRepository.DeleteById(Id);
        }
    }
}
