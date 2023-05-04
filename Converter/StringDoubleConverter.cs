using Newtonsoft.Json;
using System.Globalization;

namespace RagnarockWebsocket.Converter
{
    public sealed class StringDoubleConverter : JsonConverter
    {

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            double.TryParse((string)reader.Value, CultureInfo.InvariantCulture, out double result);
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(double));
        }
    }
}
