using Newtonsoft.Json;
using RagnarockWebsocketCore.Converter;
using RagnarockWebsocketCore.Enums;

namespace RagnarockWebsocketCore.Data
{
    internal struct AHOUData
    {
        [JsonConverter(typeof(RowersEnumConverter))]
        public Rowers rowersId;
    }
}
