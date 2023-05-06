using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Text;

namespace RagnarockWebsocketTest
{
    // TODO: These unit tests tend to hang randomly - not sure if this is connected to some asynchronous logic from the websockets, or just MSBuild being weird.
    [TestClass]
    public class RagnaWSTest
    {
        #region Connection
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ServerUriValidationTest()
        {
            // given
            var invalidServerUri = "ws://localhost:65535";

            // when
            using (new RagnaWS(invalidServerUri, ConnectionMode.Server)) { }

            // then ArgumentException is thrown
        }

        [TestMethod]
        public void ServerConnectedTest()
        {
            // given
            var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server);
            var are = new AutoResetEvent(false);
            var connected = false;

            // when
            using (socket)
            {
                socket.Connected += () =>
                {
                    are.Set();
                };
                using var client = new WatsonWsClient(new Uri("ws://localhost:65535"));
                client.StartAsync(); // Simulate starting a song in Ragnarock.
                connected = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(connected);
        }

        [TestMethod]
        public void ServerDisconnectedTest()
        {
            // given
            var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server);
            var are = new AutoResetEvent(false);
            var disconnected = false;

            // when
            using (socket)
            {
                socket.Disconnected += () =>
                {
                    are.Set();
                };
                using var client = new WatsonWsClient(new Uri("ws://localhost:65535"));
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));; // First, simulate starting a song to connect.
                client.Stop(); // Then, end the connection.
                disconnected = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(disconnected);
        }

        [TestMethod]
        public void ServerRestartConnectionTest()
        {
            // given
            var connectAre = new AutoResetEvent(false);
            var disconnectAre = new AutoResetEvent(false);
            var connected = false;
            var disconnected = false;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    connectAre.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;
                connected = connectAre.WaitOne(timeout: TimeSpan.FromSeconds(1));

                // then
                Assert.IsTrue(connected); // Connected at first.

                // when
                socket.Disconnected += () =>
                {
                    disconnectAre.Set();
                };
                socket.RestartConnection();
                disconnected = disconnectAre.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(disconnected);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ClientUriValidationTest()
        {
            // given
            var invalidClientUri = "http://localhost:65535";

            // when
           using (new RagnaWS(invalidClientUri, ConnectionMode.Client)) { }

            // then ArgumentException is thrown
        }

        [TestMethod]
        public void ClientConnectedTest()
        {
            // given
            var are = new AutoResetEvent(false);
            var connected = false;

            // when
            using (var server = new WatsonWsServer(new Uri("http://localhost:65535")))
            using (var socket = new RagnaWS("ws://localhost:65535", ConnectionMode.Client))
            {
                socket.Connected += () =>
                {
                    are.Set();
                };
                server.Start();
                connected = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(connected);
        }

        [TestMethod]
        public void ClientDisconnectedFromServerTest()
        {
            // given
            var server = new WatsonWsServer(new Uri("http://localhost:65535"));
            var are = new AutoResetEvent(false);
            var disconnected = false;

            // when
            using (server)
            {
                server.Start();
                using var socket = new RagnaWS("ws://localhost:65535", ConnectionMode.Client);
                socket.Connected += server.Stop;
                socket.Disconnected += () =>
                {
                    are.Set();
                };
                disconnected = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(disconnected);
        }

        [TestMethod]
        public void ClientRestartConnectionTest()
        {
            // given
            var connectAre = new AutoResetEvent(false);
            var disconnectAre = new AutoResetEvent(false);
            var connected = false;
            var disconnected = false;

            using (var server = new WatsonWsServer(new Uri("http://localhost:65535")))
            using (var socket = new RagnaWS("ws://localhost:65535", ConnectionMode.Client))
            {
                // when
                server.Start();
                socket.Connected += () =>
                {
                    connectAre.Set();
                };
                connected = connectAre.WaitOne(timeout: TimeSpan.FromSeconds(1));

                // then
                Assert.IsTrue(connected); // Connected at first.

                // when
                connectAre.Reset();
                socket.Disconnected += () =>
                {
                    disconnectAre.Set();
                };
                socket.RestartConnection();
                disconnected = disconnectAre.WaitOne(timeout: TimeSpan.FromSeconds(1));
                connected = connectAre.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(disconnected);
            Assert.IsTrue(connected);
        }
        #endregion

        #region In → Events From the Socket to the Game
        [TestMethod]
        public void SendCustomEventTest()
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var expectedMessage = JObject.Parse(
                @"{
                    ""event"": ""custom_event"", 
                    ""data"": { 
                        ""custom_data"": ""custom_value""
                    }
                }"
            );
            JObject? receivedMessage = null;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.MessageReceived += (sender, args) =>
                    {
                        receivedMessage = JObject.Parse(Encoding.UTF8.GetString(args.Data));
                        if ((string?)receivedMessage["event"] == "custom_event")
                        {
                            are.Set();
                        }
                    };
                    socket.SendCustomEvent("custom_event", new { custom_data = "custom_value" });
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedMessage.Should().BeEquivalentTo(expectedMessage);
        }

        [TestMethod]
        public void DisplayDialogPopupTest()
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var expectedMessage = JObject.Parse(
                @"{
                    ""event"": ""dialog"", 
                    ""data"": {
                        ""id"": ""samplePopup"",
                        ""title"": ""Sample popup"",
                        ""locationX"": 200,
                        ""locationY"": 50,
                        ""locationZ"": 20,
                        ""message"": ""Hello world"",
                        ""duration"": 5
                    }
                }"
            );
            JObject? receivedMessage = null;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.MessageReceived += (sender, args) =>
                    {
                        receivedMessage = JObject.Parse(Encoding.UTF8.GetString(args.Data));
                        if ((string?)receivedMessage["event"] == "dialog")
                        {
                            are.Set();
                        }
                    };
                    socket.DisplayDialogPopup("samplePopup", "Sample popup", new Vector3(200, 50, 20), "Hello world", 5);
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedMessage.Should().BeEquivalentTo(expectedMessage);
        }

        [TestMethod]
        [DataRow(HammerHand.Left, Hammer.DrumGod)]
        [DataRow(HammerHand.Right, Hammer.OriginalModel)]
        [DataRow(HammerHand.Both, Hammer.Surtr)]
        public void ChangeHammerTest(HammerHand hand, Hammer hammer)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var expectedMessage = JObject.Parse(
                $@"{{
                    ""event"": ""hammer"", 
                    ""data"": {{
                        ""hand"": ""{hand.ToString().ToLowerInvariant()}"",
                        ""hammer"": {(int) hammer}
                    }}
                }}"
            );
            JObject? receivedMessage = null;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.MessageReceived += (sender, args) =>
                    {
                        receivedMessage = JObject.Parse(Encoding.UTF8.GetString(args.Data));
                        if ((string?) receivedMessage["event"] == "hammer")
                        {
                            are.Set();
                        }
                    };
                    socket.ChangeHammer(hand, hammer);
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedMessage.Should().BeEquivalentTo(expectedMessage);
        }

        [TestMethod]
        [DataRow(Rowers.FirstRowLeft, new[] {4})]
        [DataRow(Rowers.FirstRowLeft | Rowers.ThirdRowRight, new[] {2, 4})]
        [DataRow(RowersUtil.SecondRow, new[] {5, 1})]
        [DataRow(RowersUtil.LeftSide, new[] { 4, 5, 6, 7 })]
        [DataRow(RowersUtil.All, new[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        public void AHOUTest(Rowers rowers, int[] expectedRowersId)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var expectedMessage = JObject.Parse(
                $@"{{
                    ""event"": ""ahou"", 
                    ""data"": {{
                        ""rowersId"": [{string.Join(',', expectedRowersId)}]
                    }}
                }}"
            );
            JObject? receivedMessage = null;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.MessageReceived += (sender, args) =>
                    {
                        receivedMessage = JObject.Parse(Encoding.UTF8.GetString(args.Data));
                        if ((string?)receivedMessage["event"] == "ahou")
                        {
                            are.Set();
                        }
                    };
                    socket.AHOU(rowers);
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedMessage.Should().BeEquivalentTo(expectedMessage);
        }

        [TestMethod]
        public void CurrentSongTest()
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var expectedMessage = JObject.Parse(
                @"{
                    ""event"": ""current_song"", 
                    ""data"": {}
                }"
            );
            JObject? receivedMessage = null;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.MessageReceived += (sender, args) =>
                    {
                        receivedMessage = JObject.Parse(Encoding.UTF8.GetString(args.Data));
                        if ((string?)receivedMessage["event"] == "current_song")
                        {
                            are.Set();
                        }
                    };
                    socket.CurrentSong();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedMessage.Should().BeEquivalentTo(expectedMessage);
        }
        #endregion

        #region Out → Events From the Game to the Socket

        [TestMethod]
        [DataRow("ragnarockInitConnection", "'connected'")]
        [DataRow("DrumHit", "{\"hand\": \"Left\", \"intensity\": 0.75}")]
        public void MessageTest(string eventName, string data)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            JToken expectedData = JToken.Parse(data);
            EventData messageReceived = new()
            {
                eventName = eventName,
                data = expectedData
            };
            string? receivedEventName = null;
            JToken? receivedData = null;

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(JObject.FromObject(messageReceived).ToString());
                };
                socket.Message += (eventName, data) =>
                {
                    receivedEventName = eventName;
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            Assert.AreEqual(eventName, receivedEventName);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        [DataRow(HammerHand.Left, 0.0123)]
        [DataRow(HammerHand.Right, 0.75)]
        public void DrumHitTest(HammerHand hand, double intensity)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""DrumHit"", 
                    ""data"": {{
                        ""hand"": ""{hand}"",
                        ""intensity"": {intensity}
                    }}
                }}"
            );
            DrumHitData? receivedData = null;
            DrumHitData expectedData = new()
            {
                hand = hand,
                intensity = intensity
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.DrumHit += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        [DataRow(0.004024, 120.0)]
        [DataRow(-0.000241, 120.0499667)]
        public void BeatHitTest(double delta, double time)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""BeatHit"", 
                    ""data"": {{
                        ""delta"": ""{delta}"",
                        ""time"": ""{time}""
                    }}
                }}"
            );
            BeatHitData? receivedData = null;
            BeatHitData expectedData = new()
            {
                delta = delta,
                time = time
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.BeatHit += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        [DataRow(120.0)]
        [DataRow(120.0499667)]
        public void BeatMissTest(double time)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""BeatMiss"", 
                    ""data"": {{
                        ""delta"": ""0.0"",
                        ""time"": ""{time}""
                    }}
                }}"
            );
            BeatMissData? receivedData = null;
            BeatMissData expectedData = new()
            {
                delta = 0.0,
                time = time
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.BeatMiss += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        [DataRow(ComboLevel.Yellow)]
        [DataRow(ComboLevel.Blue)]
        public void ComboTriggeredTest(ComboLevel level)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""ComboTriggered"", 
                    ""data"": {{
                        ""level"": {(int) level}
                    }}
                }}"
            );
            ComboTriggeredData? receivedData = null;
            ComboTriggeredData expectedData = new()
            {
                level = level
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.ComboTriggered += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        [DataRow(0.650041, ComboLevel.Blue)]
        [DataRow(1.012343, ComboLevel.Yellow)]
        [DataRow(2.501231, ComboLevel.Yellow)]
        public void ComboLostTest(double lostAt, ComboLevel expectedLostAtLevel)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""ComboLost"", 
                    ""data"": {{
                        ""lostAt"": ""{lostAt}""
                    }}
                }}"
            );
            ComboLostData? receivedData = null;
            ComboLostData expectedData = new()
            {
                lostAt = lostAt
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.ComboLost += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
            Assert.AreEqual(expectedLostAtLevel, receivedData?.GetLostAtLevel());
        }

        [TestMethod]
        [DataRow("Dewey", "Celkilt")]
        [DataRow("Kammthar", "Ultra Vomit")]
        public void StartSongTest(string songTitle, string songArtist)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""StartSong"", 
                    ""data"": {{
                        ""SongName"": ""{songTitle}"",
                        ""SongBand"": ""{songArtist}""
                    }}
                }}"
            );
            StartSongData? receivedData = null;
            StartSongData expectedData = new()
            {
                songTitle = songTitle,
                songArtist = songArtist
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.StartSong += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        [DataRow("Dewey", "Celkilt")]
        [DataRow("Kammthar", "Ultra Vomit")]
        public void SongInfosTest(string songTitle, string songArtist)
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""SongInfos"", 
                    ""data"": {{
                        ""SongName"": ""{songTitle}"",
                        ""SongBand"": ""{songArtist}""
                    }}
                }}"
            );
            SongInfosData? receivedData = null;
            SongInfosData expectedData = new()
            {
                songTitle = songTitle,
                songArtist = songArtist
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.SongInfos += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        public void EndSongTest()
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;
            var messageReceived =
                @"{
                    ""event"": ""EndSong"", 
                    ""data"": { }
                }";
            EndSongData? receivedData = null;
            EndSongData expectedData = new();

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.EndSong += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }

        [TestMethod]
        public void ScoreTest()
        {
            // given
            var are = new AutoResetEvent(false);
            var received = false;

            var percentageOfPerfects = 39;
            var comboBlue = 3;
            var comboYellow = 0;
            var missed = 5;
            var hit = 211;
            var hitPercentage = 98;
            var hitDeltaAverage = 11;
            var distance = 1555.403809;
            
            var messageReceived = FormattableString.Invariant(
                $@"{{
                    ""event"": ""Score"", 
                    ""data"": {{
                        ""stats"": {{
                            ""PercentageOfPerfects"": {percentageOfPerfects},
                            ""ComboBlue"": {comboBlue},
                            ""ComboYellow"": {comboYellow},
                            ""Missed"": {missed},
                            ""Hit"": {hit},
                            ""HitPercentage"": {hitPercentage},
                            ""HitDeltaAverage"": {hitDeltaAverage}
                        }},
                        ""extra"": {{ }},
                        ""distance"": ""{distance}""
                    }}
                }}"
            );
            ScoreData? receivedData = null;
            ScoreData expectedData = new()
            {
                stats = new()
                {
                    percentageOfPerfects = percentageOfPerfects,
                    comboBlue = comboBlue,
                    comboYellow = comboYellow,
                    missed = missed,
                    hit = hit,
                    hitPercentage = hitPercentage,
                    hitDeltaAverage = hitDeltaAverage
                },
                extra = new(),
                distance = distance
            };

            using (var socket = new RagnaWS("http://localhost:65535", ConnectionMode.Server))
            using (var client = new WatsonWsClient(new Uri("ws://localhost:65535")))
            {
                // when
                socket.Connected += () =>
                {
                    client.SendAsync(messageReceived);
                };
                socket.Score += (data) =>
                {
                    receivedData = data;
                    are.Set();
                };
                client.StartAsync().Wait(TimeSpan.FromSeconds(1));;

                received = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            }

            // then
            Assert.IsTrue(received);
            receivedData.Should().BeEquivalentTo(expectedData);
        }
        #endregion
    }
}