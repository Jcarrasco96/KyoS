using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum FARSType
    {
        Initial,
        MtpReview,
        Discharge_PSR,
        Discharge_Ind,
        Discharge_Group,
        Other
    }
    public class FARSUtils
    {
        public static FARSType GetypeByIndex(int index)
        {
            return (index == 0) ? FARSType.Initial :
                   (index == 1) ? FARSType.MtpReview :
                   (index == 2) ? FARSType.Discharge_PSR :
                   (index == 3) ? FARSType.Discharge_Ind :
                   (index == 4) ? FARSType.Discharge_Group :
                   (index == 5) ? FARSType.Other : FARSType.Initial;
        }
    }
}
