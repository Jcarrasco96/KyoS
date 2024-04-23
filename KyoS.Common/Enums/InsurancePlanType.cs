namespace KyoS.Common.Enums
{
    public enum InsurancePlanType
    {
        Full_Medicaid,
        Medicare_Part_AB,
        Medicare_Part_B

    }
    public class InsurancePlanUtils
    {
        public static InsurancePlanType GetInsurancePlanTypeByIndex(int index)
        {
            return (index == 0) ? InsurancePlanType.Full_Medicaid :
                   (index == 1) ? InsurancePlanType.Medicare_Part_AB :
                   (index == 2) ? InsurancePlanType.Medicare_Part_B : InsurancePlanType.Full_Medicaid;
        }
    }
  
}
