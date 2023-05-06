# RagnarockWebsocket

NuGet package for handling communication with Ragnarock VR and Vikings on Tour through a dedicated Websocket.

Documentation provided by WanaDev - [WebSocket (BETA)](https://wanadev.notion.site/WebSocket-BETA-30cdc789baa44b899d161bcbd128227d). 
It describes how the connection setup should be done from the game's side, which is a requirement to use this library in practice.

## Server vs. Client

Current version of Ragnarock (1.8) requires a Websocket Server for the game to connect into when a new play-session is established (player goes from main menu into the boat), 
but this only allows for one application using the Websocket to be running at the same time.

This will most likely change in the future to allow for multiple independent applications, 
so Websocket Client mode is also supported by this library.

## Usage

### Initialization & Connection

```csharp
using RagnarockWebsocket;
using RagnarockWebsocket.Enums;

// Server connection.
RagnaWS serverSocket = new RagnaWS("http://localhost:8083/", ConnectionMode.Server);  // Equivalent to new RagnaWS(), using the default values from Wanadev documentation.

// Client connection
RagnaWS clientSocket = new RagnaWS("ws://localhost:8083/", ConnectionMode.Client);
```

With server connection, after initializing, the listener connection is kept until the socket object is disposed of, so restarting the song in Ragnarock will re-establish the connection.

With client connection, if there are connectivity issues, socket needs to be restarted manually:

```csharp
socket.RestartConnection();
```

### Listening to events

```RagnaWS``` exposes a number of events you can listen to:

<table>
	<thead>
		<th>Event</th>
		<th style="min-width: 200px;">Description</th>
		<th>Payload</th>
		<th>Example</th>
	</thead>
	<tbody>
		<tr>
			<td><code>Connected</code></td>
			<td>Happens when connection with Ragnarock is established.</td>
			<td>None</td>
			<td>

```csharp
socket.Connected += () => {
    Console.WriteLine("Connected to Ragnarock!");
}
```

</td>
		</tr>
		<tr>
			<td><code>Disconnected</code></td>
			<td>Happens when connection with Ragnarock is lost.</td>
			<td>None</td>
			<td>

```csharp
socket.Disconnected += () => {
    Console.WriteLine("Connection to Ragnarock lost!");
}
```

</td>
		</tr>
		<tr>
			<td><code>Message</code></td>
			<td>Triggered whenever any message is sent by the game.</td>
			<td><code>string eventName, Newtonsoft.Json.Linq.JToken data</code></td>
			<td>

```csharp
socket.Message += (eventName, data) => {
    Console.WriteLine($"Received event {eventName} with data {data}.");
}

// Received event ragnarockInitConnection with data connected.
// Received event DrumHit with data {hand: "Left", intensity: 0.75}.
```

</td>
		</tr>
		<tr>
			<td><code>DrumHit</code></td>
			<td>Happens when player hits a drum.</td>
			<td><code>RagnarockWebsocket.Data.DrumHitData&nbsp;data</code></td>
			<td>

```csharp
socket.DrumHit += (data) => {
    Console.WriteLine($"Drum hit with {data.hand} hand at {data.intensity} intensity.");
}

// Drum hit with Left hand at 0.0123 intensity.
// Drum hit with Right hand at 0.75 intensity.
```

</td>
		</tr>
		<tr>
			<td><code>BeatHit</code></td>
			<td>Happens when player hits a note (beat).</td>
			<td><code>RagnarockWebsocket.Data.BeatHitData&nbsp;data</code></td>
			<td>

```csharp
socket.BeatHit += (data) => {
    Console.WriteLine($"Note hit at {data.time} beat with {1000 * data.delta}ms latency.");
}

// Note hit at 120.0 beat with 4.024ms latency.
// Note hit at 120.0499667 beat with -0.241ms latency.
```

</td>
		</tr>
		<tr>
			<td><code>BeatMiss</code></td>
			<td>Happens when player misses a note (beat).</td>
			<td><code>RagnarockWebsocket.Data.BeatMissData&nbsp;data</code></td>
			<td>

```csharp
socket.BeatMiss += (data) => {
    Console.WriteLine($"Note missed at {data.time} beat.");
}

// Note missed at 120.0 beat.
// Note missed at 120.0499667 beat.
```

</td>
		</tr>
		<tr>
			<td><code>ComboTriggered</code></td>
			<td>Happens when player successfully triggers a combo with a shield.</td>
			<td><code>RagnarockWebsocket.Data.ComboTriggeredData&nbsp;data</code></td>
			<td>

```csharp
socket.ComboTriggered += (data) => {
    Console.WriteLine($"{data.level} combo triggered.");
}

// Yellow combo triggered.
// Blue combo triggered.
```

</td>
		</tr>
		<tr>
			<td><code>ComboLost</code></td>
			<td>Happens when player loses a charged combo due to a miss.<br/><br/>Misses that break the streak when player haven't charged BLUE combo yet won't trigger this event.</td>
			<td><code>RagnarockWebsocket.Data.ComboLostData&nbsp;data</code></td>
			<td>

```csharp
socket.ComboLost += (data) => {
    Console.WriteLine($"{data.GetLostAtLevel()} combo lost at {data.lostAt}.");
}

// Blue combo lost at 0.650041.
// Yellow combo lost at 1.012343.
// Yellow combo lost at 2.501231.
```

</td>
		</tr>
		<tr>
			<td><code>StartSong</code></td>
			<td>Happens when player starts the song.</td>
			<td><code>RagnarockWebsocket.Data.StartSongData&nbsp;data</code></td>
			<td>

```csharp
socket.StartSong += (data) => {
    Console.WriteLine($"Started playing {data.songTitle} by {data.songArtist}.");
}

// Started playing Dewey by Celkilt.
// Started playing Kammthar by Ultra Vomit.
```

</td>
		</tr>
		<tr>
			<td><code>SongInfos</code></td>
			<td>Happens when application requests current song info from the game with CurrentSong event.</td>
			<td><code>RagnarockWebsocket.Data.SongInfosData&nbsp;data</code></td>
			<td>

```csharp
socket.SongInfos += (data) => {
    Console.WriteLine($"Playing {data.songTitle} by {data.songAuthor}.");
}
socket.CurrentSong().Wait();

// Playing Dewey by Celkilt.
// Playing Kammthar by Ultra Vomit.
```

</td>
		</tr>
		<tr>
			<td><code>EndSong</code></td>
			<td>Happens when player completes a song. This is not triggered if player leaves to main menu before finishing the song.</td>
			<td><code>RagnarockWebsocket.Data.EndSongData&nbsp;data</code></td>
			<td>

```csharp
socket.Endsong += (data) => {
    Console.WriteLine("Finished playing a song.");
}
```

</td>
		</tr>
		<tr>
			<td><code>Score</code></td>
			<td>Happens when player uploads a score (should be right after EndSong).</td>
			<td><code>RagnarockWebsocket.Data.ScoreEventData&nbsp;data</code></td>
			<td>

```csharp
socket.Endsong += (data) => {
    Console.WriteLine($"Traveled {data.distance}m and only missed {data.stats.missed} notes.");
}

// Traveled 1555.403809m and only missed 5 notes.
```

</td>
		</tr>
	</tbody>
</table>

### Sending events to the game

`RagnaWS` allows to communicate with the game by using one of the predefined methods:

<table>
	<thead>
		<th>Event</th>
		<th style="min-width: 200px;">Description</th>
		<th>Parameters</th>
		<th>Example</th>
	</thead>
	<tbody>
		<tr>
			<td><code>SendCustomEvent</code></td>
			<td>Send a custom event to the game.<br/><br/>This is intended to be used for undocumented/new events that are not yet implemented by this library.</td>
			<td><code>string eventName, object data</code></td>
			<td>

```csharp
socket.SendCustomEvent("hammer", new { hammer = 1, hand = "left" }).Wait();
```

</td>
		</tr>
		<tr>
			<td><code>DisplayDialogPopup</code></td>
			<td>Displays a popup window within the game.</td>
			<td><code>string&nbsp;dialogIdentifier, string&nbsp;title, System.Numerics.Vector3&nbsp;location, string&nbsp;message, double&nbsp;duration</code></td>
			<td>

```csharp
using System.Numerics;

socket.DisplayDialogPopup("samplePopup", "Sample popup", new Vector3(200, 50, 20), "Hello world", 5).Wait();
```

</td>
		</tr>
		<tr>
			<td><code>ChangeHammer</code></td>
			<td>Changes hammer for the current run in the hand(s) to the provided one.</td>
			<td><code>RagnarockWebsocket.Enums.HammerHand&nbsp;hand, RagnarockWebsocket.Enums.Hammer&nbsp;hammer</code></td>
			<td>

```csharp
using RagnarockWebsocket.Enums;

socket.ChangeHammer(HammerHand.Left, Hammer.DrumGod).Wait();
socket.ChangeHammer(HammerHand.Right, Hammer.OriginalModel).Wait();
socket.ChangeHammer(HammerHand.Both, Hammer.Surtr).Wait();
```

</td>
		</tr>
		<tr>
			<td><code>AHOU</code></td>
			<td>Triggers arm animation for given rowers.</td>
			<td><code>RagnarockWebsocket.Enums.Rowers&nbsp;rowers</code></td>
			<td>

```csharp
using RagnarockWebsocket.Enums;

socket.AHOU(Rowers.FirstRowLeft).Wait();
socket.AHOU(Rowers.FirstRowLeft | Rowers.ThirdRowRight).Wait();
socket.AHOU(RowersUtil.SecondRow).Wait();
socket.AHOU(RowersUtil.LeftSide).Wait();
socket.AHOU(RowersUtil.All).Wait();
```

</td>
		</tr>
		<tr>
			<td><code>CurrentSong</code></td>
			<td>Notifies the game to trigger SongInfos event.</td>
			<td>None</td>
			<td>

```csharp
socket.SongInfos += (data) => {
    Console.WriteLine($"Playing {data.songTitle} by {data.songAuthor}.");
}
socket.CurrentSong().Wait();
```

</td>
		</tr>
	</tbody>
</table>