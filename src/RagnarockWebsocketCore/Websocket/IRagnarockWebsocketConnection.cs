using Newtonsoft.Json.Linq;

namespace System.Net.Http { } // Fix for .NET Framework 4.8 issue (https://github.com/dotnet/sdk/issues/24146)

namespace RagnarockWebsocketCore.Websocket
{
    public interface IRagnarockWebsocketConnection : IDisposable
    {
        #region Connection
        /// <summary>
        /// Check if the Websocket connection is alive and well.
        /// </summary>
        /// <returns>Whether socket is still running.</returns>
        bool IsConnected();
        /// <summary>
        /// Happens when Websocket connection is established.
        /// </summary>
        event Action Connected;
        /// <summary>
        /// Happens when Websocket connection is terminated.
        /// </summary>
        event Action Disconnected;
        /// <summary>
        /// Restarts Websocket connection.
        /// </summary>
        void RestartConnection();
        #endregion

        #region In → Events From the Socket to the Game
        /// <summary>
        /// Sends an event with data to Ragnarock through the Websocket connection.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="data">Data payload to be serialized alongside the event.</param>
        /// <returns>Task associated with sending out the event.</returns>
        Task SendEvent(string eventName, object data);
        #endregion

        #region Out → Events From the Game to the Socket
        /// <summary>
        /// Happens whenever message is received from Ragnarock through the Websocket connection.
        /// </summary>
        event Action<string, JToken> Message;
        #endregion
    }
}
