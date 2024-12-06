using System.Collections.Generic;

namespace Enrollments.Domain.Enumerations
{
    public class EnrollmentTypeEnum
    {
        public static readonly string MANDATORY = "MANDATORY";
        public static readonly string RECOMMENDED = "RECOMMENDED";
        public static readonly string SELF_ENROLLED = "SELF-ENROLLED";

        public static List<string> GetTypes()
        {
            List<string> types = new()
            {
                MANDATORY,
                RECOMMENDED,
                SELF_ENROLLED
            };

            return types;
        }
    }
}
