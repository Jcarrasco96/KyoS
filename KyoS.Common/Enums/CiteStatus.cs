namespace KyoS.Common.Enums
{
    public enum CiteStatus
    {
        S,
        C,
        R,
        NS,
        AR,
        CO,
        A,
        X
    }
    public class CiteUtils
    {
        public static CiteStatus GetCiteByIndex(int index)
        {
            return (index == 1) ? CiteStatus.S :
                   (index == 2) ? CiteStatus.C:
                   (index == 3) ? CiteStatus.R :
                   (index == 4) ? CiteStatus.NS :
                   (index == 5) ? CiteStatus.AR :
                   (index == 6) ? CiteStatus.CO :
                   (index == 7) ? CiteStatus.A :
                   (index == 8) ? CiteStatus.X : CiteStatus.S;
        }
    }
  
}
