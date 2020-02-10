using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetStateService.Azure.Storage.Blob.Converters
{

    public class TimeSpanConverter : JsonConverter<TimeSpan?>
    {

        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            return s != null ? (TimeSpan?)TimeSpan.Parse(reader.GetString()) : null;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            if (value != null)
                writer.WriteStringValue(value.ToString());
            else
                writer.WriteNullValue();
        }

    }

}
