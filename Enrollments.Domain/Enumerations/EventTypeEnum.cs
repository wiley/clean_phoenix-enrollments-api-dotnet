using System.Collections.Generic;

namespace Enrollments.Domain.Enumerations
{
    public class EventTypeEnum
    {
        public static readonly string FACILITATED = "FACILITATED";
        public static readonly string WEBINAR = "WEBINAR";
        public static readonly string WEBCAST = "WEBCAST";

        public static List<string> GetTypes()
        {
            List<string> types = new()
            {
                FACILITATED,
                WEBINAR,
                WEBCAST
            };

            return types;
        }
    }
}
