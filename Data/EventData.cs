using Newtonsoft.Json;

namespace RagnarockWebsocket.Data
{
    internal struct EventData
    {
        [JsonProperty("event")]
        public string eventName;
        public object data;
    }
}
