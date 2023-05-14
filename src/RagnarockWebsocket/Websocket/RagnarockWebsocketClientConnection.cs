using Newtonsoft.Json.Linq;
using WatsonWebsocket;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using RagnarockWebsocket.Data;

namespace RagnarockWebsocket.Websocket
{
    internal class RagnarockWebsocketClientConnection : IRagnarockWebsocketConnection
    {
        private readonly Uri socketURI;
        private WatsonWsClient client;

        public RagnarockWebsocketClientConnection(Uri socketURI)
        {
            if (socketURI.Scheme != "ws" && socketURI.Scheme != "wss")
            {
                throw new ArgumentException($"Invalid URI for WebSocket client connection: {socketURI}");
            }
            this.socketURI = socketURI;
            client = ConnectToServer();
        }

        #region Connection
        public event Action Connected = delegate { };
        public event Action Disconnected = delegate { };

        public bool IsConnected()
        {
            return client.Connected;
        }

        public void RestartConnection()
        {
            if (IsConnected())
            {
                client.Stop();
            }
            client = ConnectToServer();
        }

        private WatsonWsClient ConnectToServer()
        {
            WatsonWsClient newClient = new WatsonWsClient(socketURI);
            newClient.ServerConnected += OnServerConnected;
            newClient.ServerDisconnected += OnServerDisconnected;
            newClient.MessageReceived += OnMessageReceived;
            newClient.StartAsync(); // Run this async to make sure listeners to Connected event can be registered before it actually starts.
            return newClient;
        }

        private void OnServerConnected(object? sender, EventArgs args)
        {
            // I'm gonna assume here that client will need to send the same greeting as server currently.
            SendEvent("welcome", "welcome message data");
            Connected?.Invoke();
        }

        private void OnServerDisconnected(object? sender, EventArgs args)
        {
            Disconnected?.Invoke();
        }
        #endregion

        #region In → Events From the Socket to the Game
        public Task SendEvent(string eventName, object data)
        {
            if (!IsConnected())
            {
                throw new InvalidOperationException("Websocket client is not connected to Ragnarock server!");
            }
            EventData eventData = new()
            {
                eventName = eventName,
                data = data
            };
            return client.SendAsync(JObject.FromObject(eventData).ToString());
        }
        #endregion

        #region Out → Events From the Game to the Socket
        public event Action<string, JToken> Message = delegate { };

        private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            switch (args.MessageType)
            {
                case WebSocketMessageType.Text:
                    JObject payload = JObject.Parse(Encoding.UTF8.GetString(args.Data.ToArray()));
                    string? eventName = (string?)payload["event"];
                    JToken? data = payload["data"];
                    if (eventName == null || data == null)
                    {
                        throw new JsonException($"Invalid payload received from the socket: {payload}");
                    }
                    Message?.Invoke(eventName, data);
                    break;
                case WebSocketMessageType.Binary:
                    // Wanadev, what are you doing?
                    throw new ArgumentException($"Received unexpected binary message from the socket: {args.Data}");
                case WebSocketMessageType.Close:
                default:
                    // Do nothing.
                    break;
            }
        }
        #endregion

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
