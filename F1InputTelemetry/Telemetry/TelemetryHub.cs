using F1UDP.Structs;

namespace F1InputTelemetry.Telemetry
{
    public interface ITelemetryHub
    {
        event Action<PacketCarTelemetryData>? CarTelemetryReceived;
        event Action<PacketEventData>? EventReceived;
        event Action<PacketSessionData>? SessionReceived;
    }

    public sealed class TelemetryHub : ITelemetryHub
    {
        public event Action<PacketCarTelemetryData>? CarTelemetryReceived;
        public event Action<PacketEventData>? EventReceived;
        public event Action<PacketSessionData>? SessionReceived;

        internal void Publish(PacketCarTelemetryData data) => CarTelemetryReceived?.Invoke(data);
        internal void Publish(PacketEventData data) => EventReceived?.Invoke(data);
        internal void Publish(PacketSessionData data) => SessionReceived?.Invoke(data);
    }
}
