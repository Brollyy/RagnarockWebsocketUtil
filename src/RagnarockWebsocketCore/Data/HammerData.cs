using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RagnarockWebsocketCore.Enums;

namespace RagnarockWebsocketCore.Data
{
    internal struct HammerData
    {
        public Hammer hammer;
        [JsonConverter(typeof(StringEnumConverter))]
        public HammerHand hand;
    }
}
