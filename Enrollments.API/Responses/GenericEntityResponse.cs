using System;
using Newtonsoft.Json;

namespace Enrollments.API.Responses
{
    public abstract class GenericEntityResponse
    {
        [JsonProperty(Order = -2)]
        public Guid Id { get; set; }

        [JsonProperty(Order = 100)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(Order = 101)]
        public UInt32 CreatedBy { get; set; }

        [JsonProperty(Order = 102)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(Order = 103)]
        public UInt32 UpdatedBy { get; set; }

        [JsonProperty(Order = 103)]
        public LinkSelfResponse _links { get; set; }

        public GenericEntityResponse() {
            _links ??= new LinkSelfResponse();
            CreatedBy = 0;
            UpdatedBy = 0;
        }
    }
}
