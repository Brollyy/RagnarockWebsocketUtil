using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RagnarockWebsocket.Enums;

namespace RagnarockWebsocket.Data
{
    /// <summary>
    /// Data received from the DrumHit event.
    /// </summary>
    public struct DrumHitData
    {
        /// <summary>
        /// Only treat HammerHand.LEFT and HammerHand.RIGHT as valid values here.<br/>
        /// If both hammers are hit at roughly the same time,
        /// two separate DrumHit events are issued, NOT a single one with HammerHand.BOTH.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public HammerHand hand;

        /// <summary>
        /// Intensity of the hit - seems to be capped at 0.75.
        /// </summary>
        public double intensity;
    }
}
