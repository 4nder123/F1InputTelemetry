using F1UDP.Enums;
using F1UDP.Interfaces;
using F1UDP.Structs;

namespace F1UDP
{
    public static class ParsePacket
    {
        public static F1Packet? ToPacket(this byte[] data)
        {
            PacketHeader packetHeader = PacketHeader.FromBytes(data);
            PacketType packetType = packetHeader.PacketType;

            return packetType switch
            {
                PacketType.CarTelemetry => PacketCarTelemetryData.FromBytes(data, packetHeader),
                PacketType.Event => PacketEventData.FromBytes(data, packetHeader),
                PacketType.Session => PacketSessionData.FromBytes(data, packetHeader),
                _ => null
            };
        }
    }
}
