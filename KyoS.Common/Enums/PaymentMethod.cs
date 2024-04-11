namespace KyoS.Common.Enums
{
    public enum PaymentMethod
    {
        Check,
        Direct_Deposit,
        Zelle
    }
    public class PaymentMethodUtils
    {
        public static PaymentMethod GetPaymentMethodByIndex(int index)
        {
            return (index == 1) ? PaymentMethod.Check :
                   (index == 2) ? PaymentMethod.Direct_Deposit :
                   (index == 3) ? PaymentMethod.Zelle : PaymentMethod.Check;
        }
    }
  
}
