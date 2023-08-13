using RagnarockWebsocketCore.Data;
using RagnarockWebsocketCore.Enums;
using RagnarockWebsocketCore.Websocket;
using System.Numerics;

namespace RagnarockWebsocketCore.Message
{
    public class RagnarockMessageSender
    {
        private readonly IRagnarockWebsocketConnection ragnarockWebsocketConnection;

        /// <summary>
        /// Constructs an instance of a sender to a given Ragnarock Websocket Connection.
        /// </summary>
        /// <param name="ragnarockWebsocketConnection">Websocket connection through which messages will be sent.</param>
        public RagnarockMessageSender(IRagnarockWebsocketConnection ragnarockWebsocketConnection)
        {
            this.ragnarockWebsocketConnection = ragnarockWebsocketConnection;
        }

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
            return ragnarockWebsocketConnection.SendEvent(eventName, data);
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
            // TODO: check what the dialogIdentifier can be used for - can we adjust the message after the fact, change location or duration?
            DialogData data = new()
            {
                id = dialogIdentifier,
                title = title,
                locationX = location.X,
                locationY = location.Y,
                locationZ = location.Z,
                message = message,
                duration = duration
            };
            return ragnarockWebsocketConnection.SendEvent(Event.dialog.ToString(), data);
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
            HammerData data = new()
            {
                hand = hand,
                hammer = hammer
            };
            return ragnarockWebsocketConnection.SendEvent(Event.hammer.ToString(), data);
        }

        /// <summary>
        /// Triggers arm animation for given rowers.
        /// </summary>
        /// <param name="rowers">rowers, as indicated by flag enum</param>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task AHOU(Rowers rowers)
        {
            AHOUData data = new()
            {
                rowersId = rowers
            };
            return ragnarockWebsocketConnection.SendEvent(Event.ahou.ToString(), data);
        }

        /// <summary>
        /// Notifies the game to trigger SongInfos event.
        /// </summary>
        /// <returns>Task associated with the async operation of sending the event.</returns>
        public Task CurrentSong()
        {
            return ragnarockWebsocketConnection.SendEvent(Event.current_song.ToString(), new CurrentSongData());
        }
        #endregion

    }
}