using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;

namespace BliveHelper.Utils.Structs
{
    public class UnixTimestampConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(long);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.WriteLine(reader.Value);
            return DateTimeOffset.FromUnixTimeSeconds((long)reader.Value).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timestamp = DateTimeOffset.ParseExact((string)value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToUnixTimeSeconds();
            writer.WriteValue(timestamp);
        }
    }
}
