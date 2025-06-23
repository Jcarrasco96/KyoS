namespace KyoS.Common.Enums
{
    public enum InsuranceType
    {
        Medicaid,
        Medicare,
        Comercial,
        Self_Pay
    }
    public class InsuranceUtils
    {
        public static InsuranceType GetInsuranceTypeByIndex(int index)
        {
            return (index == 0) ? InsuranceType.Medicaid :
                   (index == 1) ? InsuranceType.Medicare :
                   (index == 2) ? InsuranceType.Comercial:
                   (index == 3 ) ? InsuranceType.Self_Pay : InsuranceType.Medicaid;
        }
    }
  
}
