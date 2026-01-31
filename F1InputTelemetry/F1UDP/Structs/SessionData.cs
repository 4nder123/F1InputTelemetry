using System.IO;
using F1UDP.Enums;
using F1UDP.Interfaces;

namespace F1UDP.Structs
{
    public struct PacketSessionData : F1Packet
    {
        public PacketHeader Header;
        public byte Weather;
        public sbyte TrackTemperature;
        public sbyte AirTemperature;
        public byte TotalLaps;
        public ushort TrackLength;    
        public SessionType SessionType;                  
        public sbyte TrackId;
        public FormulaType Formula;
        public ushort SessionTimeLeft;
        public ushort SessionDuration;
        public byte PitSpeedLimit;
        public byte GamePaused;
        public byte IsSpectating;
        public byte SpectatorCarIndex;

        public static PacketSessionData FromBytes(byte[] bytes, PacketHeader header) 
        {
            PacketSessionData packetSessionData = new PacketSessionData();
            packetSessionData.Header = header;

            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                ms.Position = header.PacketSize;

                packetSessionData.Weather = reader.ReadByte();
                packetSessionData.TrackTemperature = reader.ReadSByte();
                packetSessionData.AirTemperature = reader.ReadSByte();
                packetSessionData.TotalLaps = reader.ReadByte();
                packetSessionData.TrackLength = reader.ReadUInt16();
                packetSessionData.SessionType = SessionTypeDecoder.Decode(reader.ReadByte(), header.PacketFormat);
                packetSessionData.TrackId = reader.ReadSByte();
                packetSessionData.Formula = (FormulaType)reader.ReadByte();
                packetSessionData.SessionTimeLeft = reader.ReadUInt16();
                packetSessionData.SessionDuration = reader.ReadUInt16();
                packetSessionData.PitSpeedLimit = reader.ReadByte();
                packetSessionData.GamePaused = reader.ReadByte();
                packetSessionData.IsSpectating = reader.ReadByte();
                packetSessionData.SpectatorCarIndex = reader.ReadByte();
            }

            return packetSessionData;
        }
    }
}
