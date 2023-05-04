using Newtonsoft.Json;
using RagnarockWebsocket.Enums;

namespace RagnarockWebsocket.Data
{
    /// <summary>
    /// Data received from the ComboTriggered event.
    /// </summary>
    public struct ComboTriggeredData
    {
        /// <summary>
        /// Level of the combo triggered.
        /// </summary>
        public ComboLevel level;
    }
}
