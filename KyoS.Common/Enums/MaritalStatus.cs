using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum MaritalStatus
    {
        Unknown,
        Single,
        Married,
        Cohabiting,
        Divorced,
        Separated,
        Widowed
    }

    public class MaritalUtils
    {
        public static MaritalStatus GetMaritalByIndex(int index)
        {
            return (index == 0) ? MaritalStatus.Unknown :
                   (index == 1) ? MaritalStatus.Single :
                   (index == 2) ? MaritalStatus.Married :
                   (index == 3) ? MaritalStatus.Cohabiting :
                   (index == 4) ? MaritalStatus.Divorced :
                   (index == 5) ? MaritalStatus.Separated :
                   (index == 6) ? MaritalStatus.Widowed : MaritalStatus.Unknown;
        }
    }
}
