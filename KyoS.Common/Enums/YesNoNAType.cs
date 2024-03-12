namespace KyoS.Common.Enums
{
    public enum YesNoNAType
    {
        Yes,
        No,
        NA
    }
   
    public class YesNoNAUtils
    {
      
        public static YesNoNAType GetYesNoNaByIndex(int index)
        {
            return (index == 0) ? YesNoNAType.Yes :
                   (index == 1) ? YesNoNAType.No :
                   (index == 2) ? YesNoNAType.NA : YesNoNAType.Yes;
        }
    }
}
