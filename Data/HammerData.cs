using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RagnarockWebsocket.Enums;

namespace RagnarockWebsocket.Data
{
    internal struct HammerData
    {
        public Hammer hammer;
        [JsonConverter(typeof(StringEnumConverter))]
        public HammerHand hand;
    }
}
