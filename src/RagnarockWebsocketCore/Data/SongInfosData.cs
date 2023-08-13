using Newtonsoft.Json;

namespace RagnarockWebsocketCore.Data
{
    /// <summary>
    /// Data received from the StartSong event.
    /// </summary>
    public struct StartSongData
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
