﻿using System.Numerics;
using Newtonsoft.Json.Linq;
using RagnarockWebsocketCore.Data;
using RagnarockWebsocketCore.Enums;
using RagnarockWebsocketCore.Message;
using RagnarockWebsocketCore.Websocket;
using RagnarockWebsocket.Enums;
using RagnarockWebsocket.Websocket;

namespace System.Net.Http { } // Fix for .NET Framework 4.8 issue (https://github.com/dotnet/sdk/issues/24146)

namespace RagnarockWebsocket
{
    public class RagnaWS : IDisposable
    {
        private readonly IRagnarockWebsocketConnection ragnarockWebsocketConnection;
        private readonly RagnarockMessageSender ragnarockMessageSender;
        private readonly RagnarockMessageHandler ragnarockMessageHandler;

        /// <summary>
        /// Sets up the Websocket for communication in given mode (client or server).
        /// In case of server, socketUri defines the URI of the new socket server that Ragnarock will need to connect into.
        /// In case of client, socketUri should point to the URI of the socket server managed by Ragnarock.
        /// </summary>
        /// <param name="socketUri">URI of the Websocket, e.g. new Uri("ws://localhost:8033/").</param>
        /// <param name="connectionMode">Type of Websocket connection to setup.</param>
        public RagnaWS(string socketUri = "http://localhost:8033/", ConnectionMode connectionMode = ConnectionMode.Server)
        {
            var socketURI = new Uri(socketUri);
            switch(connectionMode)
            {
                case ConnectionMode.Server:
                    ragnarockWebsocketConnection = new RagnarockWebsocketServerConnection(socketURI);
                    break;
                case ConnectionMode.Client:
                    ragnarockWebsocketConnection = new RagnarockWebsocketClientConnection(socketURI);
                    break;
                default:
                    throw new NotImplementedException("Provided connection mode is not supported");
            }
            ragnarockWebsocketConnection.Connected += HandleConnected;
            ragnarockWebsocketConnection.Disconnected += HandleDisconnected;
            ragnarockWebsocketConnection.Message += HandleMessage;
            ragnarockMessageSender = new(ragnarockWebsocketConnection);
            ragnarockMessageHandler = new()
            {
                OnDrumHit = delegate (DrumHitData data) { DrumHit?.Invoke(data); },
                OnBeatHit = delegate (BeatHitData data) { BeatHit?.Invoke(data); },
                OnBeatMiss = delegate (BeatMissData data) { BeatMiss?.Invoke(data); },
                OnComboTriggered = delegate (ComboTriggeredData data) { ComboTriggered?.Invoke(data); },
                OnComboLost = delegate (ComboLostData data) { ComboLost?.Invoke(data); },
                OnStartSong = delegate (StartSongData data) { StartSong?.Invoke(data); },
                OnSongInfos = delegate (SongInfosData data) { SongInfos?.Invoke(data); },
                OnEndSong = delegate (EndSongData data) {  EndSong?.Invoke(data); },
                OnScore = delegate (ScoreData data) { Score?.Invoke(data); },
                OnMessage = delegate (string eventName, JToken data) { Message?.Invoke(eventName, data); },
            };
        }

        #region Connection
        /// <summary>
        /// Happens when connection with Ragnarock is established.<br/>
        /// For ConnectionMode.Server mode, this should happen when song is started in Ragnarock.<br/>
        /// For ConnectionMode.Client mode, this will happen soon after RagnaWS constructor finishes, as long as Ragnarock server is running - it's unclear right now whether Wanadev intends on keeping the server up the whole time game is running, or only when players enter the boat.
        /// </summary>
        public event Action Connected = delegate { };
        /// <summary>
        /// Happens when connection with Ragnarock is lost.<br/>
        /// For ConnectionMode.Server mode, this should happen after player is finished with the song in game and leaves to main menu.<br/>
        /// For ConnectionMode.Client mode, this will probably happen after game is closed or after RagnaWS is disposed.
        /// </summary>
        public event Action Disconnected = delegate { };

        /// <summary>
        /// Restarts the socket connection.
        /// </summary>
        public void RestartConnection()
        {
            ragnarockWebsocketConnection.RestartConnection();
        }
        #endregion

        #region In → Events From the Socket to the Game
        /// <summary>
        /// Send a custom event to the game.<br/>
        /// This is intended to be used for undocumented/new events that are not yet implemented by this library.
        /// </summary>
        /// <param name="eventName">name of the custom event</param>
        /// <param name="data">data payload</param>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task SendCustomEvent(string eventName, object data)
        {
            return ragnarockMessageSender.SendCustomEvent(eventName, data);
        }

        /// <summary>
        /// Displays a popup window within the game.
        /// </summary>
        /// <param name="dialogIdentifier">Unique identifier of the popup window.</param>
        /// <param name="title">Title displayed in the popup window.</param>
        /// <param name="location">3D position vector of where the popup window needs to be placed.</param>
        /// <param name="message">Message displayed in the popup window.</param>
        /// <param name="duration">How long the popup window will stay up (in seconds).</param>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task DisplayDialogPopup(string dialogIdentifier, string title, Vector3 location, string message, double duration)
        {
            return ragnarockMessageSender.DisplayDialogPopup(dialogIdentifier, title, location, message, duration);
        }

        /// <summary>
        /// Changes hammer for the current run in the hand(s) to the provided one.
        /// If player haven't unlocked the indicated hammer, the default one is set instead.
        /// </summary>
        /// <param name="hand">Hand(s) to change the hammer for.</param>
        /// <param name="hammer">Hammer to change to.</param>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task ChangeHammer(HammerHand hand, Hammer hammer)
        {
            return ragnarockMessageSender.ChangeHammer(hand, hammer);
        }

        /// <summary>
        /// Triggers arm animation for given rowers.
        /// </summary>
        /// <param name="rowers">rowers, as indicated by flag enum</param>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task AHOU(Rowers rowers)
        {
            return ragnarockMessageSender.AHOU(rowers);
        }

        /// <summary>
        /// Notifies the game to trigger SongInfos event.
        /// </summary>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task CurrentSong()
        {
            return ragnarockMessageSender.CurrentSong();
        }
        #endregion

        #region Out → Events From the Game to the Socket
        /// <summary>
        /// Happens when player hits a drum.
        /// </summary>
        public event Action<DrumHitData> DrumHit = delegate { };
        /// <summary>
        /// Happens when player hits a note (beat).
        /// </summary>
        public event Action<BeatHitData> BeatHit = delegate { };
        /// <summary>
        /// Happens when player misses a note (beat).
        /// </summary>
        public event Action<BeatMissData> BeatMiss = delegate { };
        /// <summary>
        /// Happens when player successfully triggers a combo with a shield.
        /// </summary>
        public event Action<ComboTriggeredData> ComboTriggered = delegate { };
        /// <summary>
        /// Happens when player loses a charged combo due to a miss.<br/>
        /// Misses that break the streak when player haven't charged BLUE combo yet won't trigger this event.
        /// </summary>
        public event Action<ComboLostData> ComboLost = delegate { };
        /// <summary>
        /// Happens when player starts the song.
        /// </summary>
        public event Action<StartSongData> StartSong = delegate { };
        /// <summary>
        /// Happens when application requests current song info from the game with CurrentSong event.
        /// </summary>
        public event Action<SongInfosData> SongInfos = delegate { };
        /// <summary>
        /// Happens when player completes a song. This is not triggered if player leaves to main menu before finishing the song.
        /// </summary>
        public event Action<EndSongData> EndSong = delegate { };
        /// <summary>
        /// Happens when player uploads a score (should be right after EndSong).
        /// </summary>
        public event Action<ScoreData> Score = delegate { };
        /// <summary>
        /// Triggered whenever any message is sent by the game.<br/>
        /// Note that events supported by this library will trigger both this and their corresponding event at the same time.<br/>
        /// This is intended to be used as a temporary fallback in case the data for supported events is misaligned with the current version of the game, or there are new events that are not yet implemented here.<br/>
        /// First param is the event name, second param is the deserialized data from the event.
        /// </summary>
        public event Action<string, JToken> Message = delegate { };
        #endregion

        #region Event listeners
        private void HandleMessage(string eventName, JToken data)
        {
            ragnarockMessageHandler.HandleMessage(eventName, data);
        }

        private void HandleConnected()
        {
            Connected?.Invoke();
        }

        private void HandleDisconnected()
        {
            Disconnected?.Invoke();
        }
        #endregion

        public void Dispose()
        {
            ragnarockWebsocketConnection.Dispose();
        }
    }
}