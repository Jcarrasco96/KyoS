namespace KyoS.Common.Enums
{
    public enum InsurancePlanType
    {
        Full_Medicaid,
        Madicare_Part_AB,
        Madicare_Part_B

    }
    public class InsurancePlanUtils
    {
        public static InsurancePlanType GetInsurancePlanTypeByIndex(int index)
        {
            return (index == 0) ? InsurancePlanType.Full_Medicaid :
                   (index == 1) ? InsurancePlanType.Madicare_Part_AB :
                   (index == 2) ? InsurancePlanType.Madicare_Part_B : InsurancePlanType.Full_Medicaid;
        }
    }
  
}
