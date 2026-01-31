using F1UDP.Enums;
using F1UDP.Interfaces;
using System.IO;

namespace F1UDP.Structs
{
    public struct LapData
    {
        public uint LastLapTimeMS;
        public uint CurrentLapTimeMS;

        public ushort Sector1TimeMS;
        public ushort Sector2TimeMS;

        public ushort DeltaToCarInFrontMS;
        public ushort DeltaToRaceLeaderMS;

        public float LapDistance;
        public float TotalDistance;
        public float SafetyCarDelta;

        public byte CarPosition;
        public byte CurrentLapNum;
        public byte PitStatus;
        public byte NumPitStops;
        public byte Sector;
        public byte CurrentLapInvalid;
        public byte Penalties;
        public byte TotalWarnings;
        public byte CornerCuttingWarnings;
        public byte NumUnservedDriveThroughPens;
        public byte NumUnservedStopGoPens;
        public byte GridPosition;
        public byte DriverStatus;
        public ResultType ResultStatus;

        public byte PitLaneTimerActive;
        public ushort PitLaneTimeInLaneMS;
        public ushort PitStopTimerMS;
        public byte PitStopShouldServePen;

        public float SpeedTrapFastestSpeed;
        public byte SpeedTrapFastestLap;

        public static LapData FromBytes(BinaryReader reader, ushort packetFormat)
        {
            LapData lap = new LapData();

            if (packetFormat <= 2019)
            {
                lap.LastLapTimeMS = (uint)(reader.ReadSingle() * 1000f);
                lap.CurrentLapTimeMS = (uint)(reader.ReadSingle() * 1000f);
                reader.ReadSingle();
                lap.Sector1TimeMS = (ushort)(reader.ReadSingle() * 1000f);
                lap.Sector2TimeMS = (ushort)(reader.ReadSingle() * 1000f);

            }
            else if (packetFormat == 2020)
            {
                lap.LastLapTimeMS = (uint)(reader.ReadSingle() * 1000f);
                lap.CurrentLapTimeMS = (uint)(reader.ReadSingle() * 1000f);

                lap.Sector1TimeMS = reader.ReadUInt16();
                lap.Sector2TimeMS = reader.ReadUInt16();

                reader.BaseStream.Position += 20;
            }
            else
            {
                lap.LastLapTimeMS = reader.ReadUInt32();
                lap.CurrentLapTimeMS = reader.ReadUInt32();

                lap.Sector1TimeMS = reader.ReadUInt16();
                if (packetFormat >= 2023) reader.ReadByte();

                lap.Sector2TimeMS = reader.ReadUInt16();
                if (packetFormat >= 2023)
                {
                    reader.ReadByte();
                    lap.DeltaToCarInFrontMS = reader.ReadUInt16();
                    if (packetFormat >= 2024) reader.ReadByte();

                    lap.DeltaToRaceLeaderMS = reader.ReadUInt16();
                    if (packetFormat >= 2024) reader.ReadByte();
                }
            }


            lap.LapDistance = reader.ReadSingle();
            lap.TotalDistance = reader.ReadSingle();
            lap.SafetyCarDelta = reader.ReadSingle();

            lap.CarPosition = reader.ReadByte();
            lap.CurrentLapNum = reader.ReadByte();
            lap.PitStatus = reader.ReadByte();

            if (packetFormat >= 2021)
                lap.NumPitStops = reader.ReadByte();

            lap.Sector = reader.ReadByte();
            lap.CurrentLapInvalid = reader.ReadByte();
            lap.Penalties = reader.ReadByte();

            if (packetFormat >= 2021)
            {
                lap.TotalWarnings = reader.ReadByte();
                if (packetFormat >= 2023)
                    lap.CornerCuttingWarnings = reader.ReadByte();
                lap.NumUnservedDriveThroughPens = reader.ReadByte();
                lap.NumUnservedStopGoPens = reader.ReadByte();
            }

            lap.GridPosition = reader.ReadByte();
            lap.DriverStatus = reader.ReadByte();
            lap.ResultStatus = ResultTypeDecoder.Decode(reader.ReadByte(), packetFormat);

            if (packetFormat >= 2021)
            {
                lap.PitLaneTimerActive = reader.ReadByte();
                lap.PitLaneTimeInLaneMS = reader.ReadUInt16();
                lap.PitStopTimerMS = reader.ReadUInt16();
                lap.PitStopShouldServePen = reader.ReadByte();
            }

            if (packetFormat >= 2024)
            {
                lap.SpeedTrapFastestSpeed = reader.ReadSingle();
                lap.SpeedTrapFastestLap = reader.ReadByte();
            }

            return lap;
        }
    }

    public struct PacketLapData : F1Packet
    {
        public PacketHeader Header;
        public LapData[] Cars;

        public byte TimeTrialPBCarIdx;
        public byte TimeTrialRivalCarIdx;

        public static PacketLapData FromBytes(byte[] bytes, PacketHeader header)
        {
            PacketLapData packet = new PacketLapData();
            int totalCars = header.PacketFormat <= 2019 ? 20 : 22;

            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                packet.Header = header;
                ms.Position = packet.Header.PacketSize;

                packet.Cars = new LapData[totalCars];
                for (int i = 0; i < totalCars; i++)
                {
                    packet.Cars[i] = LapData.FromBytes(reader, header.PacketFormat);
                }

                if (header.PacketFormat >= 2022)
                {
                    packet.TimeTrialPBCarIdx = reader.ReadByte();
                    packet.TimeTrialRivalCarIdx = reader.ReadByte();
                }
            }

            return packet;
        }
    }
}
