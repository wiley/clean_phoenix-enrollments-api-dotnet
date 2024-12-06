using System.Collections.Generic;

namespace Enrollments.Domain.Enumerations
{
    public class EventLocationTypeEnum
    {
        public static readonly string MS_TEAMS = "MS_TEAMS";
        public static readonly string SLACK = "SLACK";
        public static readonly string GOOGLE_MEET = "GOOGLE_MEET";
        public static readonly string ZOOM = "ZOOM";
        public static readonly string IN_LOCATION = "IN_LOCATION";

        public static List<string> GetTypes()
        {
            List<string> types = new()
            {
                MS_TEAMS,
                SLACK,
                GOOGLE_MEET,
                ZOOM,
                IN_LOCATION
            };

            return types;
        }
    }
}
