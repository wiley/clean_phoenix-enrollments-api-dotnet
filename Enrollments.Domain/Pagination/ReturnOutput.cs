using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Enrollments.Domain.Pagination
{
    public class ReturnOutput<T>
    {
        public int Count { get; set; }
        public List<T> Items { get; set; }

        [JsonProperty("_links")]
        public PaginationLinks Links { get; set; }

        private static string makeUrl(string url, int size, int offset, List<KeyValuePair<string, string>> queryParams = null)
        {
            var builder = new StringBuilder();
            builder.Append(url);
            if (queryParams is object)
            {
                queryParams.ForEach(param =>
                {
                    builder.Append((builder.ToString().Contains("?") ? "&" : "?") + $"{param.Key}={param.Value}");
                });
            }
            builder.Append((builder.ToString().Contains("?") ? "&" : "?") + $"size={size}");
            builder.Append($"&offset={offset}");
            return builder.ToString();
        }

        public void MakePaginationLinks(HttpRequest request, int offset, int size, int count, List<KeyValuePair<string, string>> queryParams = null)
        {
            validateHttps(request);
            var url = $"{request.Scheme}://{request.Host.Value}{request.Path}";
            if (queryParams is object)
            {
                queryParams = queryParams.Where(q => q.Key.ToLower() is not nameof(size) and not nameof(offset)).ToList();
            }
            Links = new PaginationLinks
            {
                Self = new LinkedResource() { Href = makeUrl(url, size, offset, queryParams) }
            };
            if (offset >= size)
            {
                Links.Previous = new LinkedResource() { Href = makeUrl(url, size, (offset - size), queryParams) };
                Links.First = new LinkedResource() { Href = makeUrl(url, size, 0, queryParams) };
            }
            if (offset + size < count)
            {
                Links.Next = new LinkedResource() { Href = makeUrl(url, size, (offset + size), queryParams) };
            }
            if (offset + size <= count)
            {
                Links.Last = new LinkedResource() { Href = makeUrl(url, size, count - size, queryParams) };
            }
        }

        private static void validateHttps(HttpRequest request)
        {
            if (!request.IsHttps)
            {
                request.Scheme = request.Scheme.Replace("http", "https");
            }
        }
    }
}