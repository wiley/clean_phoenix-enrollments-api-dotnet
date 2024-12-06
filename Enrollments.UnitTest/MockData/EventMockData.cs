using Enrollments.Domain.Event;
using System.Collections.Generic;
using System;

namespace Enrollments.UnitTest.MockData
{
    public static class EventMockData
    {
        public static List<Event> GetEventListData()
        {
            List<Event> events = new List<Event>();
            events.Add(GenerateEvent(Guid.Parse("dfe028b7-8a43-4582-9c4a-eca4db29df53")));
            events.Add(GenerateEvent(Guid.Parse("5b9c2b82-7250-45cb-a01e-26cd253f24dc")));

            return events;
        }

        public static Event GetEvent(Guid id) {
            return GenerateEvent(id);
        }

        private static Event GenerateEvent(Guid id) {
        return new Event
            {
                Id = id,
                CohortId = new Guid("5e2d9393-e22a-4b90-9a02-69d6be694b49"),
                TrainingProgramId = new Guid("8537e528-0e43-4833-baf3-2279439033da"),
                OrganizerId = 0,
                Title = "Mocked Event Title",
                Objectives = "Some random objectives for the mocked event.",
                StartAt = DateTime.Parse("2023-05-22T13:39:21-03:00"),
                EndAt = DateTime.Parse("2023-05-26T11:22:33-03:00"),
                Timezone = "America/Sao_Paulo",
                Type = "WEBCAST",
                Location = new EventLocation {
                    Description = "A MS Teams location example.",
                    Type = "MS_TEAMS",
                    Link = "https://teams.microsoft.com/l/meetup-join/19%3ameeting_Mzk2ZTk0ODMtZGE4ZC00ZDU3LWIwZGMtNjA1ZGQxNjRiZjNh%40thread.v2/0?context=%7b%22Tid%22%3a%2224fe244f-890e-46ef-be2f-a5202976b7a5%22%2c%22Oid%22%3a%220ac75e96-d666-4bac-9693-454209945a72%22%7d",
                },
                CreatedAt = DateTime.Parse("2023-02-22T21:40:18.243Z"),
                CreatedBy = 0,
                UpdatedAt = DateTime.Parse("2023-02-22T21:40:18.243Z"),
                UpdatedBy = 0,
            };
        }
    }
}
