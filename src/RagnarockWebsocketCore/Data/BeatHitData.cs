using Newtonsoft.Json;
using RagnarockWebsocketCore.Converter;

namespace RagnarockWebsocketCore.Data
{
    /// <summary>
    /// Data received from the BeatHit event.
    /// </summary>
    public struct BeatHitData
    {
        /// <summary>
        /// Delta from the ideal timing, in seconds. Positive - too late, negative - too early.
        /// </summary>
        [JsonConverter(typeof(StringDoubleConverter))]
        public double delta;

        /// <summary>
        /// Timing of the beat in global beats (determined by global song BPM).
        /// </summary>
        [JsonConverter(typeof(StringDoubleConverter))]
        public double time;
    }
}
