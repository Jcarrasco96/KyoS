namespace KyoS.Common.Enums
{
    public enum StatusBill
    {
        Unbilled,
        Billed,
        Pending,
        Paid
    }
    public class StatusBillUtils
    {
        public static StatusBill GetStatusBillByIndex(int index)
        {
            return (index == 1) ? StatusBill.Unbilled :
                   (index == 2) ? StatusBill.Billed :
                   (index == 3) ? StatusBill.Pending :
                   (index == 4) ? StatusBill.Paid: StatusBill.Unbilled;
        }
    }
  
}
