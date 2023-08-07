namespace KyoS.Common.Enums
{
    public enum ThemeType
    {
        PSR,
        Group,
        All
    }

    public class ThemeUtils
    {
        public static ThemeType GetThemeByIndex(int index)
        {
            return (index == 0) ? ThemeType.PSR :
                   (index == 1) ? ThemeType.Group :
                   (index == 2) ? ThemeType.All : ThemeType.PSR;
        }
    }

}
