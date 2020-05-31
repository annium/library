using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XRest.Clients.TypeScript.Infrastructure.JsonConverters
{
    internal class HttpMethodJsonConverter : JsonConverter<HttpMethod>
    {
        public override HttpMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, HttpMethod value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}