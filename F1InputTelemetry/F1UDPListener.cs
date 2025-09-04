using F1UDP;
using F1UDP.Enums;
using F1UDP.Structs;
using System.Net;
using System.Net.Sockets;

namespace TelemetryUI
{
    class F1UDPListener
    {
        public static byte spectatorCarIndex = 255;
        public static void HandleTelmetryData(MainWindow main, PacketCarTelemetryData telemetry)
        {
            var playerIndex = telemetry.Header.PlayerCarIndex;
            if (spectatorCarIndex != 255) playerIndex = spectatorCarIndex;
            if (playerIndex == 255) return;

            var playerData = telemetry.Cars[playerIndex];
            main.Dispatcher.Invoke(() => {
                main.Update(playerData.Throttle, playerData.Brake, playerData.Clutch, playerData.Steer);
            });
        }
        public static void HandleEventData(MainWindow main, PacketEventData eventData)
        {
            EventType eventType = eventData.EventType;
            switch (eventType)
            {
                case EventType.SessionStarted:
                    main.Dispatcher.Invoke(() => { main.ShowWindow(); });
                    break;

                case EventType.SessionEnded:
                    main.Dispatcher.Invoke(() => { main.HideWindow(); });
                    break;
            }
        }
        public static async Task StartListener(MainWindow main, Settings settings, CancellationToken token)
        {
            using var listener = new UdpClient(settings.Port);
            var groupEP = new IPEndPoint(IPAddress.Parse(settings.IPAddress), settings.Port);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var receiveTask = listener.ReceiveAsync();
                    var completedTask = await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, token));
                    if (completedTask != receiveTask) break; 

                    var result = await receiveTask;
                    var packet = result.Buffer.ToPacket();

                    switch (packet)
                    {
                        case PacketCarTelemetryData telemetry:
                            HandleTelmetryData(main, telemetry);
                            break;

                        case PacketEventData eventData when settings.AutoHide:
                            HandleEventData(main, eventData);
                            break;

                        case PacketSessionData sessionData:
                            spectatorCarIndex = sessionData.SpectatorCarIndex;
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

    }
}
