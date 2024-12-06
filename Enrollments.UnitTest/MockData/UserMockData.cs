using Enrollments.Domain;
using System;

namespace Enrollments.UnitTest.MockData
{
    public static class UserMockData
    {
        public static UserAPIRepresentation GetUserAPIRepresentation()
        {
            Random random = new Random();
            return GenerateUserAPIRepresentation(random.Next());
        }

        private static UserAPIRepresentation GenerateUserAPIRepresentation(int id)
        {
            return new UserAPIRepresentation
            {
                Id = id,
                UserID = id,
                UserName = "Auto Generated Name",
                FirstName = "Auto Generated First Name",
                LastName = "Auto Generated Last Name",
                Email = "user@wiley.com"
            };
        }
    }
}
