using RagnarockWebsocket;
using RagnarockWebsocket.Enums;
using Newtonsoft.Json.Linq;
using System.Numerics;

void RagnarockWebsocket_Connected()
{
    Console.WriteLine("Connected!");
}

void RagnarockWebsocket_Disconnected()
{
    Console.WriteLine("Disconnected");
}

void RagnarockWebsocket_Message(string eventName, JToken data)
{
    Console.WriteLine($"Received event {eventName} with data {data}");
}

RagnaWS socket = new RagnaWS(connectionMode: ConnectionMode.Server);
socket.Connected += RagnarockWebsocket_Connected;
socket.Disconnected += RagnarockWebsocket_Disconnected;
socket.Message += RagnarockWebsocket_Message;

while (true)
{
    string command = Console.ReadLine();
    var commWithParams = command.Split(" ");
    switch(commWithParams[0])
    {
        case "quit": return 0;
        case "ahou": socket.SendCustomEvent("ahou", new {rowersId = new[] { int.Parse(commWithParams[1]) } }).Wait(); break;
        case "song": socket.CurrentSong().Wait(); break;
        case "hammer": socket.ChangeHammer(HammerHand.Both, (Hammer) int.Parse(commWithParams[1])).Wait(); break;
        case "dialog":
            socket.DisplayDialogPopup(commWithParams[1], commWithParams[2], new Vector3(float.Parse(commWithParams[3]), float.Parse(commWithParams[4]), float.Parse(commWithParams[5])), commWithParams[6], 10); break;
        default: socket.SendCustomEvent("hammer", new { hand = 2, hammer = int.Parse(commWithParams[1]) }); break;
    }
}