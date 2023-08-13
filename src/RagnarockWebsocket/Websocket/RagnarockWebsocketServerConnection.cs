using Newtonsoft.Json.Linq;
using WatsonWebsocket;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using RagnarockWebsocketCore.Data;
using RagnarockWebsocketCore.Websocket;

namespace RagnarockWebsocket.Websocket
{
    internal class RagnarockWebsocketServerConnection : IRagnarockWebsocketConnection
    {
        private static readonly TimeSpan MAX_TIMEOUT = TimeSpan.FromSeconds(5);

        private readonly Uri socketURI;
        private WatsonWsServer server;

        public RagnarockWebsocketServerConnection(Uri socketURI)
        {
            if (socketURI.Scheme != Uri.UriSchemeHttp && socketURI.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException($"Invalid URI for WebSocket server to start: {socketURI}");
            }
            this.socketURI = socketURI;
            server = StartServer();
        }

        #region Connection
        public event Action Connected = delegate { };
        public event Action Disconnected = delegate { };

        public bool IsConnected()
        {
            return server.IsListening && server.ListClients().Any();
        }

        public void RestartConnection()
        {
            if (IsConnected())
            {
                server.Stop();
            }
            server = StartServer();
        }

        private WatsonWsServer StartServer()
        {
            WatsonWsServer newServer = new WatsonWsServer(socketURI);
            newServer.ClientConnected += OnClientConnected;
            newServer.ClientDisconnected += OnClientDisconnected;
            newServer.MessageReceived += OnMessageReceived;
            newServer.StartAsync().Wait(MAX_TIMEOUT);
            return newServer;
        }

        private void OnClientConnected(object? sender, ConnectionEventArgs args)
        {
            // Greeting should really be sent to the specific client that was just connected,
            // but we're assuming only one client in the first place anyway.
            SendEvent("welcome", "welcome message data");
            Connected?.Invoke();
        }

        private void OnClientDisconnected(object? sender, EventArgs args)
        {
            // This should only be send after last client disconnects,
            // but we're assuming only one client in the first place anyway.
            Disconnected?.Invoke();
        }
        #endregion

        #region In → Events From the Socket to the Game
        public Task SendEvent(string eventName, object data)
        {
            if (!IsConnected())
            {
                throw new InvalidOperationException("Ragnarock client is not connected to Websocket server!");
            }
            EventData eventData = new()
            {
                eventName = eventName,
                data = data
            };
            // I'm assuming only one client at a time - in this context it's reasonable, as we open this server specifically only for Ragnarock to connect.
            return server.SendAsync(
                server.ListClients().First().Guid,
                JObject.FromObject(eventData).ToString()
            );
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
            server.Dispose();
        }
    }
}
