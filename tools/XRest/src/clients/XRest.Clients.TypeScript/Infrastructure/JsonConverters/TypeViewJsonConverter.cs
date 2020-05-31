using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Infrastructure.JsonConverters
{
    internal class TypeViewJsonConverter : JsonConverter<TypeView>
    {
        public override TypeView Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TypeView value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());

            // full view (with properties)
            // writer.WriteStartObject();
            //
            // writer.WriteString("Name", value.ToString());
            //
            // writer.WritePropertyName("Properties");
            // writer.WriteStartObject();
            // foreach (var property in value.Properties)
            //     writer.WriteString(property.Name, property.Type.ToString());
            // writer.WriteEndObject();
            //
            // writer.WriteEndObject();
        }
    }
}