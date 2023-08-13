using Newtonsoft.Json;
using RagnarockWebsocketCore.Converter;

namespace RagnarockWebsocketCore.Data
{
    /// <summary>
    /// Data received from the BeatMiss event.
    /// </summary>
    public struct BeatMissData
    {
        /// <summary>
        /// Delta from the ideal timing. This is always 0 for BeatMiss.
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
