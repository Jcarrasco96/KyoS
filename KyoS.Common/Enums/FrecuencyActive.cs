using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum FrecuencyActive
    {
        Daily,
        Three_Time_per_week_or_more,
        Three_Time_per_week_or_less,
        Once_per_week,
        Rarely,
        Never
    }

    public class FrecuencyActiveUtils
    {
        public static FrecuencyActive GetFrecuencyActiveByIndex(int index)
        {
            return (index == 0) ? FrecuencyActive.Daily :
                   (index == 1) ? FrecuencyActive.Three_Time_per_week_or_more :
                   (index == 2) ? FrecuencyActive.Three_Time_per_week_or_less :
                   (index == 3) ? FrecuencyActive.Once_per_week :
                   (index == 4) ? FrecuencyActive.Rarely :
                   (index == 5) ? FrecuencyActive.Never : FrecuencyActive.Daily;
        }
    }
}
