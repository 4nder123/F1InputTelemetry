namespace F1UDP.Enums
{
    public enum SessionType : byte
    {
        Unknown,
        Practice1,
        Practice2,
        Practice3,
        ShortPractice,
        Qualifying1,
        Qualifying2,
        Qualifying3,
        ShortQualifying,
        OneShotQualifying,
        SprintShootout1,
        SprintShootout2,
        SprintShootout3,
        ShortSprintShootout,
        OneShotSprintShootout,
        Race,
        Race2,
        Race3,
        TimeTrial
    }
    public static class SessionTypeDecoder
    {
        public static SessionType Decode(byte raw, int PacketFormat)
        {
            if (PacketFormat >= 2024)
            {
                return raw switch
                {
                    1 => SessionType.Practice1,
                    2 => SessionType.Practice2,
                    3 => SessionType.Practice3,
                    4 => SessionType.ShortPractice,

                    5 => SessionType.Qualifying1,
                    6 => SessionType.Qualifying2,
                    7 => SessionType.Qualifying3,
                    8 => SessionType.ShortQualifying,
                    9 => SessionType.OneShotQualifying,

                    10 => SessionType.SprintShootout1,
                    11 => SessionType.SprintShootout2,
                    12 => SessionType.SprintShootout3,
                    13 => SessionType.ShortSprintShootout,
                    14 => SessionType.OneShotSprintShootout,

                    15 => SessionType.Race,
                    16 => SessionType.Race2,
                    17 => SessionType.Race3,
                    18 => SessionType.TimeTrial,

                    _ => SessionType.Unknown
                };
            }
            if (PacketFormat >= 2021)
            {
                return raw switch
                {
                    1 => SessionType.Practice1,
                    2 => SessionType.Practice2,
                    3 => SessionType.Practice3,
                    4 => SessionType.ShortPractice,

                    5 => SessionType.Qualifying1,
                    6 => SessionType.Qualifying2,
                    7 => SessionType.Qualifying3,
                    8 => SessionType.ShortQualifying,
                    9 => SessionType.OneShotQualifying,

                    10 => SessionType.Race,
                    11 => SessionType.Race2,
                    12 => SessionType.Race3,
                    13 => SessionType.TimeTrial,

                    _ => SessionType.Unknown
                };
            }
            return raw switch
            {
                1 => SessionType.Practice1,
                2 => SessionType.Practice2,
                3 => SessionType.Practice3,
                4 => SessionType.ShortPractice,

                5 => SessionType.Qualifying1,
                6 => SessionType.Qualifying2,
                7 => SessionType.Qualifying3,
                8 => SessionType.ShortQualifying,
                9 => SessionType.OneShotQualifying,

                10 => SessionType.Race,
                11 => SessionType.Race2,
                12 => SessionType.TimeTrial,

                _ => SessionType.Unknown
            };
        }
    }

}
