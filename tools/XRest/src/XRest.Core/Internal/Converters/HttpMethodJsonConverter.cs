using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XRest.Core.Internal.Converters;

internal class HttpMethodJsonConverter : JsonConverter<HttpMethod>
{
    public override HttpMethod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.GetString();

        return token is not null ? new HttpMethod(token) : null;
    }

    public override void Write(Utf8JsonWriter writer, HttpMethod value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Method);
    }
}
