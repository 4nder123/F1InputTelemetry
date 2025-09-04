using System.IO;
using F1UDP.Enums;

namespace F1UDP.Structs
{
    public struct PacketHeader
    {
        public ushort PacketFormat;
        public byte GameYear;
        public byte GameMajorVersion;
        public byte GameMinorVersion;
        public byte PacketVersion;
        public PacketType PacketType;
        public ulong SessionUID;
        public float SessionTime;
        public uint FrameIdentifier;
        public uint OverallFrameIdentifier; 
        public byte PlayerCarIndex;
        public byte SecondaryPlayerCarIndex;
        public int PacketSize;

        public static PacketHeader FromBytes(byte[] bytes)
        {
            PacketHeader packetHeader = new PacketHeader();

            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                long startPos = ms.Position;
                ushort PacketFormat = reader.ReadUInt16();

                packetHeader.PacketFormat = PacketFormat;
                packetHeader.GameYear = PacketFormat <= 2022 ? (byte)(PacketFormat % 100) : reader.ReadByte();
                packetHeader.GameMajorVersion = PacketFormat == 2018 ? (byte) 0 : reader.ReadByte();
                packetHeader.GameMinorVersion = PacketFormat == 2018 ? (byte) 0 : reader.ReadByte();
                packetHeader.PacketVersion = reader.ReadByte();
                packetHeader.PacketType = (PacketType)reader.ReadByte();
                packetHeader.SessionUID = reader.ReadUInt64();
                packetHeader.SessionTime = reader.ReadSingle();
                packetHeader.FrameIdentifier = reader.ReadUInt32();
                packetHeader.OverallFrameIdentifier = PacketFormat <= 2022 ? packetHeader.FrameIdentifier: reader.ReadUInt32();
                packetHeader.PlayerCarIndex = reader.ReadByte();
                packetHeader.SecondaryPlayerCarIndex = PacketFormat <= 2019 ? (byte) 255 : reader.ReadByte();
                packetHeader.PacketSize = (int)(ms.Position - startPos);
            }

            return packetHeader;
        }
    }
}
