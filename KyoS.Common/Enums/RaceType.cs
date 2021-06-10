using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum RaceType
    {
        Unknown,
        White,
        Black,
        NativeAmerican,
        AfricanAmerican,
        Asian,
        Other
    }
    public class RaceUtils
    {
        public static RaceType GetRaceByIndex(int index)
        {
            return (index == 0) ? RaceType.Unknown :
                   (index == 1) ? RaceType.White :
                   (index == 2) ? RaceType.Black :
                   (index == 3) ? RaceType.NativeAmerican :
                   (index == 4) ? RaceType.AfricanAmerican :
                   (index == 5) ? RaceType.Asian :
                   (index == 6) ? RaceType.Other : RaceType.Unknown;
        }
    }
}
