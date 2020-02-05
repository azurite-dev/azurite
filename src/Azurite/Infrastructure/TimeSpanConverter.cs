using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azurite.Infrastructure
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        // public override bool CanConvert(Type typeToConvert)
        // {
        //     return typeToConvert == typeof(TimeSpan);
        // }

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.ParseExact(reader.GetString(), "c", System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("c", System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}