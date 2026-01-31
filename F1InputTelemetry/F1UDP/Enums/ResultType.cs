namespace F1UDP.Enums
{
    public enum ResultType : byte
    {
        Unknown,
        Invalid,
        Inactive,
        Active,
        Finished,
        DidNotFinish,
        Disqualified,
        NotClassified,
        Retired
    }
    public static class ResultTypeDecoder
    {
        public static ResultType Decode(byte raw, int packetFormat)
        {
            if (packetFormat >= 2020)
            {
                return raw switch
                {
                    0 => ResultType.Invalid,
                    1 => ResultType.Inactive,
                    2 => ResultType.Active,
                    3 => ResultType.Finished,
                    4 => ResultType.DidNotFinish,
                    5 => ResultType.Disqualified,
                    6 => ResultType.NotClassified,
                    7 => ResultType.Retired,
                    _ => ResultType.Unknown
                };
            }

            return raw switch
            {
                0 => ResultType.Invalid,
                1 => ResultType.Inactive,
                2 => ResultType.Active,
                3 => ResultType.Finished,
                4 => ResultType.Disqualified,
                5 => ResultType.NotClassified,
                6 => ResultType.Retired,
                _ => ResultType.Unknown
            };
        }
    }
}
