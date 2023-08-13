using Newtonsoft.Json.Linq;
using RagnarockWebsocketCore.Data;
using RagnarockWebsocketCore.Enums;

namespace RagnarockWebsocketCore.Message
{
    public class RagnarockMessageHandler
    {
        #region Out → Events From the Game to the Socket
        /// <summary>
        /// Happens when player hits a drum.
        /// </summary>
        public delegate void DrumHit(DrumHitData data);
        public DrumHit? OnDrumHit;
        /// <summary>
        /// Happens when player hits a note (beat).
        /// </summary>
        public delegate void BeatHit(BeatHitData data);
        public BeatHit? OnBeatHit;
        /// <summary>
        /// Happens when player misses a note (beat).
        /// </summary>
        public delegate void BeatMiss(BeatMissData data);
        public BeatMiss? OnBeatMiss;
        /// <summary>
        /// Happens when player successfully triggers a combo with a shield.
        /// </summary>
        public delegate void ComboTriggered(ComboTriggeredData data);
        public ComboTriggered? OnComboTriggered;
        /// <summary>
        /// Happens when player loses a charged combo due to a miss.<br/>
        /// Misses that break the streak when player haven't charged BLUE combo yet won't trigger this event.
        /// </summary>
        public delegate void ComboLost(ComboLostData data);
        public ComboLost? OnComboLost;
        /// <summary>
        /// Happens when player starts the song.
        /// </summary>
        public delegate void StartSong(StartSongData data);
        public StartSong? OnStartSong;
        /// <summary>
        /// Happens when application requests current song info from the game with CurrentSong event.
        /// </summary>
        public delegate void SongInfos(SongInfosData data);
        public SongInfos? OnSongInfos;
        /// <summary>
        /// Happens when player completes a song. This is not triggered if player leaves to main menu before finishing the song.
        /// </summary>
        public delegate void EndSong(EndSongData data);
        public EndSong? OnEndSong;
        /// <summary>
        /// Happens when player uploads a score (should be right after EndSong).
        /// </summary>
        public delegate void Score(ScoreData data);
        public Score? OnScore;
        /// <summary>
        /// Triggered whenever any message is sent by the game.<br/>
        /// Note that events supported by this library will trigger both this and their corresponding event, in the given order.<br/>
        /// This is intended to be used as a temporary fallback in case the data for supported events is misaligned with the current version of the game, or there are new events that are not yet implemented here.<br/>
        /// First param is the event name, second param is the deserialized data from the event.
        /// </summary>
        public delegate void Message(string eventName, JToken data);
        public Message? OnMessage;
        #endregion

        #region Event listeners
        public void HandleMessage(string eventName, JToken data)
        {
            OnMessage?.Invoke(eventName, data);
            if (Enum.TryParse(eventName, out Event evt))
            {
                switch (evt)
                {
                    case Event.DrumHit: OnDrumHit?.Invoke(data.ToObject<DrumHitData>()); break;
                    case Event.BeatHit: OnBeatHit?.Invoke(data.ToObject<BeatHitData>()); break;
                    case Event.BeatMiss: OnBeatMiss?.Invoke(data.ToObject<BeatMissData>()); break;
                    case Event.ComboTriggered: OnComboTriggered?.Invoke(data.ToObject<ComboTriggeredData>()); break;
                    case Event.ComboLost: OnComboLost?.Invoke(data.ToObject<ComboLostData>()); break;
                    case Event.StartSong: OnStartSong?.Invoke(data.ToObject<StartSongData>()); break;
                    case Event.SongInfos: OnSongInfos?.Invoke(data.ToObject<SongInfosData>()); break;
                    case Event.EndSong: OnEndSong?.Invoke(data.ToObject<EndSongData>()); break;
                    case Event.Score: OnScore?.Invoke(data.ToObject<ScoreData>()); break;
                }
            }
        }

        #endregion

    }
}