using F1UDP.Interfaces;
using System.IO;


namespace F1UDP.Structs
{
    public struct CarMotionData
    {
        public float WorldPositionX;
        public float WorldPositionY;
        public float WorldPositionZ;
        public float WorldVelocityX;
        public float WorldVelocityY;
        public float WorldVelocityZ;
        public short WorldForwardDirX;
        public short WorldForwardDirY;
        public short WorldForwardDirZ;
        public short WorldRightDirX;
        public short WorldRightDirY;
        public short WorldRightDirZ;
        public float GForceLateral;
        public float GForceLongitudinal;
        public float GForceVertical;
        public float Yaw;
        public float Pitch;
        public float Roll;
        public static CarMotionData FromBytes(BinaryReader reader, ushort PacketFormat) 
        { 
            CarMotionData car = new CarMotionData();

            car.WorldPositionX = reader.ReadSingle();
            car.WorldPositionY = reader.ReadSingle();
            car.WorldPositionZ = reader.ReadSingle();
            car.WorldVelocityX = reader.ReadSingle();
            car.WorldVelocityY = reader.ReadSingle();
            car.WorldVelocityZ = reader.ReadSingle();
            car.WorldForwardDirX = reader.ReadInt16();
            car.WorldForwardDirY = reader.ReadInt16();
            car.WorldForwardDirZ = reader.ReadInt16();
            car.WorldRightDirX = reader.ReadInt16();
            car.WorldRightDirY = reader.ReadInt16();
            car.WorldRightDirZ = reader.ReadInt16();
            car.GForceLateral = reader.ReadSingle();
            car.GForceLongitudinal = reader.ReadSingle();
            car.GForceVertical = reader.ReadSingle();
            car.Yaw = reader.ReadSingle();
            car.Pitch = reader.ReadSingle();
            car.Roll = reader.ReadSingle();


            return car;
        }
    }

    public struct PacketMotionData : F1Packet
    {
        public PacketHeader Header;
        public CarMotionData[] Cars;
        public static PacketMotionData FromBytes(byte[] bytes, PacketHeader header)
        {
            PacketMotionData packetMotionData = new PacketMotionData();
            int TotalCars = header.PacketFormat <= 2019 ? 20 : 22;
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                packetMotionData.Header = header;
                ms.Position = packetMotionData.Header.PacketSize;

                packetMotionData.Cars = new CarMotionData[TotalCars];
                for (int i = 0; i < TotalCars; i++)
                {
                    packetMotionData.Cars[i] =
                       CarMotionData.FromBytes(reader, packetMotionData.Header.PacketFormat);
                }
            }

            return packetMotionData;

        }
    }
}
