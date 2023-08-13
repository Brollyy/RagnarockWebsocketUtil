namespace RagnarockWebsocketCore.Enums
{
    /// <summary>
    /// Represents supported event names.
    /// </summary>
    public enum Event
    {
        #region Out → Events From the Game to the Socket
        /// <summary>
        /// Happens when player hits a drum.
        /// </summary>
        DrumHit,
        /// <summary>
        /// Happens when player hits a note (beat).
        /// </summary>
        BeatHit,
        /// <summary>
        /// Happens when player misses a note (beat).
        /// </summary>
        BeatMiss,
        /// <summary>
        /// Happens when player successfully triggers a combo with a shield.
        /// </summary>
        ComboTriggered,
        /// <summary>
        /// Happens when player loses a charged combo due to a miss.<br/>
        /// Misses that break the streak when player haven't charged BLUE combo yet won't trigger this event.
        /// </summary>
        ComboLost,
        /// <summary>
        /// Happens when player starts the song.
        /// </summary>
        StartSong,
        /// <summary>
        /// Happens when application requests current song info from the game with CurrentSong event.
        /// </summary>
        SongInfos,
        /// <summary>
        /// Happens when player completes a song. This is not triggered if player leaves to main menu before finishing the song.
        /// </summary>
        EndSong,
        /// <summary>
        /// Happens when player uploads a score (should be right after EndSong).
        /// </summary>
        Score,
        #endregion

        #region In → Events From the Socket to the Game
        /// <summary>
        /// Displays a popup window within the game.
        /// </summary>
        dialog,
        /// <summary>
        /// Changes hammer for the current run in the hand(s) to the provided one.
        /// If player haven't unlocked the indicated hammer, the default one is set instead.
        /// </summary>
        hammer,
        /// <summary>
        /// Triggers arm animation for given rowers.
        /// </summary>
        ahou,
        /// <summary>
        /// Notifies the game to trigger SongInfos event.
        /// </summary>
        current_song,
        #endregion
    }
}
