using Newtonsoft.Json;
using RagnarockWebsocket.Converter;
using RagnarockWebsocket.Enums;

namespace RagnarockWebsocket.Data
{
    internal struct AHOUData
    {
        [JsonConverter(typeof(RowersEnumConverter))]
        public Rowers rowersId;
    }
}
