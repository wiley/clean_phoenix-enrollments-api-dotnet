using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enrollments.Domain.Utils
{
    public class LinkJsonConverter : JsonConverter<String>
    {
        /// <summary>
        /// Used to replace "ADDHOST/" on links by something like https://localhost:/ to be provided during startup of the application. Once set <cref>SchemeHostIsNullOrEmpty</cref> is modified to false and set into internal variable, to reduce computational needs.
        /// </summary>
        public static string SchemeHost
        {
            get => schemeHost; set
            {
                SchemeHostIsNullOrEmpty = string.IsNullOrEmpty(value);
                schemeHost = value;
            }
        }

        /// <summary>
        /// This is statically defined and set to a variable to reduce computational costs, as it's conditional on main middleware pipeline and is meant to run only once.
        /// </summary>
        public static bool SchemeHostIsNullOrEmpty { get => schemeHostIsNullOrEmpty; private set => schemeHostIsNullOrEmpty = value; }

        private static readonly System.Text.RegularExpressions.Regex _regx = new(@"^ADDHOST", System.Text.RegularExpressions.RegexOptions.Compiled);
        private static string schemeHost;
        private static bool schemeHostIsNullOrEmpty = true;

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (SchemeHost != null)
                writer.WriteStringValue(_regx.Replace(value, SchemeHost ?? string.Empty));
            else
                writer.WriteStringValue(value);
        }
    }
}