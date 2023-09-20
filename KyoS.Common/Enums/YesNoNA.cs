using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum YesNoNA
    {
        Yes,
        No,
        NA
    }
   
    public class YesNoNAType
    {
      
        public static YesNoNA GetYesNoNaByIndex(int index)
        {
            return (index == 0) ? YesNoNA.Yes :
                   (index == 1) ? YesNoNA.No :
                   (index == 2) ? YesNoNA.NA : YesNoNA.Yes;
        }
    }
}
