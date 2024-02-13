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
            return (index == 0) ? RiskType.Low :
                   (index == 1) ? RiskType.Moderate:
                   (index == 2) ? RiskType.High : RiskType.Low;
        }
    }
  
}
