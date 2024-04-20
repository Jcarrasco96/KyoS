namespace KyoS.Common.Enums
{
    public enum InsurancePlanType
    {
        Medicare,
        Medicaid
        
    }
    public class InsurancePlanUtils
    {
        public static InsurancePlanType GetInsurancePlanTypeByIndex(int index)
        {
            return (index == 0) ? InsurancePlanType.Medicare :
                   (index == 1) ? InsurancePlanType.Medicaid : InsurancePlanType.Medicare;
        }
    }
  
}
