namespace KyoS.Common.Enums
{
    public enum GenderType
    {
        Female,
        Male
    }
    public class GenderUtils
    {
        public static GenderType GetGenderByIndex(int index)
        {
            return (index == 1) ? GenderType.Female :
                   (index == 2) ? GenderType.Male : GenderType.Female;
        }
    }
}
