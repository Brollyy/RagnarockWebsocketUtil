using Newtonsoft.Json;

namespace RagnarockWebsocketCore.Data
{
    public struct EventData
    {
        [JsonProperty("event")]
        public string eventName;
        public object data;
    }
}
