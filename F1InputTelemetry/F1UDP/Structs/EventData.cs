using System.IO;
using F1UDP.Enums;
using F1UDP.Interfaces;

namespace F1UDP.Structs
{
    public struct PacketEventData : F1Packet
    {
        public PacketHeader Header;
        public EventType EventType;

        public static PacketEventData FromBytes(byte[] bytes, PacketHeader header) 
        {
            PacketEventData packetEventData = new PacketEventData();
            packetEventData.Header = header;

            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                ms.Position = header.PacketSize;
                packetEventData.EventType = (EventType)reader.ReadUInt32();
            }

            return packetEventData;
        }
    }
}