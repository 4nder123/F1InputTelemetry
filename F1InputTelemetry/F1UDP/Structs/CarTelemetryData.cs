using System.IO;
using F1UDP.Interfaces;

namespace F1UDP.Structs
{
    public struct CarTelemetryData
    {
        public ushort Speed;
        public float Throttle;
        public float Steer;
        public float Brake;
        public byte Clutch;
        public sbyte Gear;
        public ushort EngineRPM;
        public byte Drs;
        public byte RevLightsPercent;
        public ushort RevLightsBitValue;
        public ushort[] BrakesTemperature;
        public byte[] TyresSurfaceTemperature;
        public byte[] TyresInnerTemperature;
        public ushort EngineTemperature;
        public float[] TyresPressure;
        public byte[] SurfaceType;

        public static CarTelemetryData FromBytes(BinaryReader reader, ushort PacketFormat)
        {
            CarTelemetryData car = new CarTelemetryData();

            car.Speed = reader.ReadUInt16();
            
            car.Throttle = (PacketFormat == 2018)? reader.ReadByte() / 100f : reader.ReadSingle();
            car.Steer = (PacketFormat == 2018) ? reader.ReadSByte() / 100f : reader.ReadSingle();
            car.Brake = (PacketFormat == 2018) ? reader.ReadByte() / 100f : reader.ReadSingle();

            car.Clutch = reader.ReadByte();
            car.Gear = reader.ReadSByte();
            car.EngineRPM = reader.ReadUInt16();
            car.Drs = reader.ReadByte();
            car.RevLightsPercent = reader.ReadByte();

            car.RevLightsBitValue = (PacketFormat <= 2020) ? car.RevLightsBitValue = 0 : reader.ReadUInt16();
            
            car.BrakesTemperature = new ushort[4];
            for (int i = 0; i < 4; i++)
                car.BrakesTemperature[i] = reader.ReadUInt16();

            car.TyresSurfaceTemperature = new byte[4];
            for (int i = 0; i < 4; i++)
                car.TyresSurfaceTemperature[i] = (PacketFormat == 2018) 
                    ? (byte)Math.Min((int)reader.ReadUInt16(), 255)
                    : reader.ReadByte();

            car.TyresInnerTemperature = new byte[4];
            for (int i = 0; i < 4; i++)
                car.TyresInnerTemperature[i] = (PacketFormat == 2018)
                    ? (byte)Math.Min((int)reader.ReadUInt16(), 255)
                    : reader.ReadByte(); ;

            car.EngineTemperature = reader.ReadUInt16();

            car.TyresPressure = new float[4];
            for (int i = 0; i < 4; i++)
                car.TyresPressure[i] = reader.ReadSingle();

            car.SurfaceType = new byte[4];
            if (PacketFormat == 2018) return car;
            for (int i = 0; i < 4; i++)
                car.SurfaceType[i] = reader.ReadByte();

            return car;
        }
    }
    public struct PacketCarTelemetryData : F1Packet
    {
        public PacketHeader Header;
        public CarTelemetryData[] Cars;

        public static PacketCarTelemetryData FromBytes(byte[] bytes, PacketHeader header)
        {
            PacketCarTelemetryData packetCarTelemetryData = new PacketCarTelemetryData();
            int TotalCars = header.PacketFormat == 2018 ? 20 : 22;
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                packetCarTelemetryData.Header = header;
                ms.Position = packetCarTelemetryData.Header.PacketSize;

                packetCarTelemetryData.Cars = new CarTelemetryData[22];
                for (int i = 0; i < TotalCars; i++)
                {
                    packetCarTelemetryData.Cars[i] =
                        CarTelemetryData.FromBytes(reader, packetCarTelemetryData.Header.PacketFormat);
                }
            }

            return packetCarTelemetryData;

        }
    }
}
