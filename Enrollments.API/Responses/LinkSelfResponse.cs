using Microsoft.AspNetCore.Mvc.Routing;

namespace Enrollments.API.Responses
{
    public class LinkSelfResponse
    {
        public Self Self { get; set; }

        public LinkSelfResponse() {
            Self ??= new Self(); 
        }
    }

    public class Self {
        public string Href { get; set; }
    }
}
