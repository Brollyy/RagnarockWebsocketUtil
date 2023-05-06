using Newtonsoft.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RagnarockWebsocketTest")]

namespace RagnarockWebsocket.Data
{
    internal struct EventData
    {
        [JsonProperty("event")]
        public string eventName;
        public object data;
    }
}
