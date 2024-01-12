namespace KyoS.Common.Enums
{
    public enum StatusTCMPaystub
    {
        Pending,
        Paid
    }
    public class StatusTCMPaystubUtils
    {
        public static StatusTCMPaystub GetStatusBillByIndex(int index)
        {
            return (index == 0) ? StatusTCMPaystub.Pending :
                   (index == 1) ? StatusTCMPaystub.Paid : StatusTCMPaystub.Pending;
        }
    }
  
}
