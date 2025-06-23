namespace KyoS.Common.Enums
{
    public enum InsuranceCoverageType
    {
        Full_Medicaid,
        MMA_Capitated,
        Dual_Special_Needs_Plan,
        Medicare_Special_Needs,
        Medicare_Advantage_Plan,
        HMO,
        PPO,
        EPO,
        POS
    }
    public class InsuranceCoverageUtils
    {
        public static InsuranceCoverageType GetInsuranceCoverageTypeByIndex(int index)
        {
            return (index == 0) ? InsuranceCoverageType.Full_Medicaid :
                   (index == 1) ? InsuranceCoverageType.MMA_Capitated :
                   (index == 2) ? InsuranceCoverageType.Dual_Special_Needs_Plan :
                   (index == 3 ) ? InsuranceCoverageType.Medicare_Special_Needs :
                   (index == 4) ? InsuranceCoverageType.Medicare_Advantage_Plan :
                   (index == 5) ? InsuranceCoverageType.HMO :
                   (index == 6) ? InsuranceCoverageType.PPO :
                   (index == 7) ? InsuranceCoverageType.EPO :
                   (index == 8) ? InsuranceCoverageType.POS : InsuranceCoverageType.Full_Medicaid;
        }
    }
  
}
