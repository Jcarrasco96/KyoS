namespace KyoS.Common.Enums
{
    public enum ThemeType
    {
        PSR,
        Group
    }

    public class ThemeUtils
    {
        public static ThemeType GetThemeByIndex(int index)
        {
            return (index == 0) ? ThemeType.PSR :
                   (index == 1) ? ThemeType.Group : ThemeType.PSR;
        }
    }

}
