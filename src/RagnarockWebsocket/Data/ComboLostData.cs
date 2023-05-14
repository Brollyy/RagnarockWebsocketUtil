using Newtonsoft.Json;
using RagnarockWebsocket.Converter;
using RagnarockWebsocket.Enums;
using RagnarockWebsocket.Extensions;

namespace RagnarockWebsocket.Data
{
    /// <summary>
    /// Data received from the ComboLost event.
    /// </summary>
    public struct ComboLostData
    {
        /// <summary>
        /// Represents what percentage the combo was at when it was lost.<br/>
        /// This value is a proportion of combo points required to do a YELLOW combo and is not capped at 1 - it also tracks the notes hit when holding on to charged yellow combo.
        /// </summary>
        [JsonConverter(typeof(StringDoubleConverter))]
        public double lostAt;

        /// <summary>
        /// Gets the level of the lost combo.
        /// </summary>
        /// <returns>level of the lost combo</returns>
        public readonly ComboLevel GetLostAtLevel()
        {
            return (ComboLevel) (int) (2 * lostAt).Clamp(1, 2);
        }
    }
}
