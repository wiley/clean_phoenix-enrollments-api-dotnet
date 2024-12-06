using Enrollments.Domain.Event;
using Enrollments.Domain.Pagination;
using Enrollments.Domain.Params;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrollments.Services.Interfaces
{
    public interface IEventService
    {
        int TotalFound { get; }

        Task<List<Event>> GetAllEvents(
            PageRequest request,
            EventFilterParams filterParams
        );

        Event GetEvent(Guid id);

        Task CreateEvent(Event theEvent);

        Task<Event> UpdateEvent(Guid Id, Event theEvent);

        Event DeleteEvent(Guid Id);
    }
}
