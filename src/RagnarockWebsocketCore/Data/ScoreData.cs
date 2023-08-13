using Newtonsoft.Json;
using RagnarockWebsocketCore.Converter;

namespace RagnarockWebsocketCore.Data
{
    /// <summary>
    /// Data received from the Score event.
    /// </summary>
    public struct ScoreData
    {
        /// <summary>
        /// Statistics about the run - hits/misses, etc.
        /// </summary>
        public ScoreStats stats;

        /// <summary>
        /// Extra info about the run.
        /// </summary>
        public ScoreExtras extra;

        /// <summary>
        /// Distance traveled, with accuracy up to 6 decimal places.
        /// </summary>
        [JsonConverter(typeof(StringDoubleConverter))]
        public double distance;
    }

    /// <summary>
    /// Inner structure for Score event data stats.
    /// </summary>
    public struct ScoreStats
    {
        /// <summary>
        /// Percentage of perfect hits, rounded to the nearest integer.
        /// </summary>
        [JsonProperty("PercentageOfPerfects")]
        public int percentageOfPerfects;

        /// <summary>
        /// Number of blue combos (level 1) hit.
        /// </summary>
        [JsonProperty("ComboBlue")]
        public int comboBlue;

        /// <summary>
        /// Number of yellow combos (level 2) hit.
        /// </summary>
        [JsonProperty("ComboYellow")]
        public int comboYellow;

        /// <summary>
        /// Number of hit notes.
        /// </summary>
        [JsonProperty("Missed")]
        public int missed;

        /// <summary>
        /// Number of hit notes.
        /// </summary>
        [JsonProperty("Hit")]
        public int hit;

        /// <summary>
        /// Percentage of hits, rounded to the nearest integer.
        /// </summary>
        [JsonProperty("HitPercentage")]
        public int hitPercentage;

        /// <summary>
        /// Average delta of all hits in milliseconds, rounded to the nearest integer.<br/>
        /// Positive - too slow, negative - too fast.
        /// </summary>
        [JsonProperty("HitDeltaAverage")]
        public int hitDeltaAverage;
    }

    /// <summary>
    /// Inner structure for Score event data extra.
    /// </summary>
    public struct ScoreExtras
    {
        // Doesn't contain anything currently, even for custom songs or when you get a High Score.
    }
}
