using Newtonsoft.Json;
using RagnarockWebsocket.Enums;

namespace RagnarockWebsocket.Converter
{
    public sealed class RowersEnumConverter : JsonConverter
    {
        public override bool CanRead { get { return false; } }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Rowers);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // TODO: implement this? We don't need it right now, but might be good to have in the future.
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                Rowers rowers = (Rowers)value;
                writer.WriteRawValue($"[{string.Join(',', rowers.GetRagnarockIds())}]");
            }
        }
    }
}
