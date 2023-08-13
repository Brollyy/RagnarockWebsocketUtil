using Newtonsoft.Json;

namespace RagnarockWebsocketCore.Data
{
    /// <summary>
    /// Data received from the SongInfos event.
    /// </summary>
    public struct SongInfosData
    {
        /// <summary>
        /// Title of the song.
        /// </summary>
        [JsonProperty("SongName")]
        public string songTitle;

        /// <summary>
        /// Author of the song.
        /// </summary>
        [JsonProperty("SongBand")]
        public string songArtist;
    }
}
