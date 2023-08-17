namespace KyoS.Common.Enums
{
    public enum RiskType
    {
        Low,
        Moderate,
        High
    }
    public class RiskUtils
    {
        public static RiskType GetRiskByIndex(int index)
        {
            return (index == 1) ? RiskType.Low :
                   (index == 2) ? RiskType.Moderate:
                   (index == 3) ? RiskType.High : RiskType.Low;
        }
    }
  
}
