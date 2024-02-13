namespace KyoS.Common.Enums
{
    public enum PaidOrigi
    {
        Income,
        Difference
    }
    public class PaidOrigiUtils
    {
        public static PaidOrigi GetPaidOrigiByIndex(int index)
        {
            return (index == 1) ? PaidOrigi.Income :
                   (index == 2) ? PaidOrigi.Difference : PaidOrigi.Income;
        }
    }
  
}
