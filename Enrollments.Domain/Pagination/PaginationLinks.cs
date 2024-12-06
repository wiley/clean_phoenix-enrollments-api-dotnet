using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Enrollments.Domain.Pagination
{
    public class PaginationLinks
    {
        [JsonPropertyName("self")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LinkedResource Self { get; set; }

        [JsonPropertyName("previous")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LinkedResource Previous { get; set; }

        [JsonPropertyName("first")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LinkedResource First { get; set; }

        [JsonPropertyName("next")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LinkedResource Next { get; set; }

        [JsonPropertyName("last")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LinkedResource Last { get; set; }
    }
}