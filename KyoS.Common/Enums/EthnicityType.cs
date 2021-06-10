using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum EthnicityType
    {
        Unknown,
        HispanicLatino,
        NonHispanicLatino
    }

    public class EthnicityUtils
    {
        public static EthnicityType GetEthnicityByIndex(int index)
        {
            return (index == 0) ? EthnicityType.Unknown :
                   (index == 1) ? EthnicityType.HispanicLatino :
                   (index == 2) ? EthnicityType.NonHispanicLatino : EthnicityType.Unknown;
        }
    }
}
