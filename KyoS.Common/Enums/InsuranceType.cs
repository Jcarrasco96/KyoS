namespace KyoS.Common.Enums
{
    public enum InsuranceType
    {
        Medicaid,
        Medicare
    }
    public class InsuranceUtils
    {
        public static InsuranceType GetInsuranceTypeByIndex(int index)
        {
            return (index == 0) ? InsuranceType.Medicaid :
                   (index == 1) ? InsuranceType.Medicare : InsuranceType.Medicaid;
        }
    }
  
}
