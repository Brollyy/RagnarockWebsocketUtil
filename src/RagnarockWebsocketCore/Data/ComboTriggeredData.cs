using Newtonsoft.Json;
using RagnarockWebsocketCore.Enums;

namespace RagnarockWebsocketCore.Data
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
