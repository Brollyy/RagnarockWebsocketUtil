using Newtonsoft.Json.Linq;
namespace RagnarockWebsocket.Websocket
{
    internal interface IRagnarockWebsocketConnection : IDisposable
    {
        #region Connection
        bool IsConnected();
        event Action Connected;
        event Action Disconnected;
        void RestartConnection();
        #endregion


        #region In → Events From the Socket to the Game
        Task SendEvent(string eventName, object data);
        #endregion

        #region Out → Events From the Game to the Socket
        event Action<string, JToken> Message;
        #endregion
    }
}
