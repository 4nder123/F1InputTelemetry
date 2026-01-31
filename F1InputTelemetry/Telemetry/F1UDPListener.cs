using F1UDP;
using F1UDP.Structs;
using System.Net;
using System.Net.Sockets;
using F1InputTelemetry.Settings;

namespace F1InputTelemetry.Telemetry
{
    class F1UDPListener
    {
        public static async Task StartListener(TelemetryHub hub, AppSettings settings, CancellationToken token)
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
                            hub.Publish(telemetry);
                            break;

                        case PacketEventData eventData:
                            hub.Publish(eventData);
                            break;

                        case PacketSessionData sessionData:
                            hub.Publish(sessionData);
                            break;
                        case PacketMotionData motionData:
                            hub.Publish(motionData);
                            break;
                        case PacketLapData lapData:
                            hub.Publish(lapData);
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
