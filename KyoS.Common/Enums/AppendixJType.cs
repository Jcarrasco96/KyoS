using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum AppendixJType
    {
        Initial,
        Review,
        Discontinuity,
    }
    public class AppendixJUtils
    {
        public static AppendixJType GetTypeByIndex(int index)
        {
            return (index == 0) ? AppendixJType.Initial :
                   (index == 1) ? AppendixJType.Review :
                   (index == 2) ? AppendixJType.Discontinuity : AppendixJType.Initial;
        }
    }
}
