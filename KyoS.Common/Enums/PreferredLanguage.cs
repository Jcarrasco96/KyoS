using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum PreferredLanguage
    {
        English,
        Spanish,
        French,
        Portuguese
    }

    public class PreferredLanguageUtils
    {
        public static PreferredLanguage GetPreferredLanguageByIndex(int index)
        {
            return (index == 0) ? PreferredLanguage.English :
                   (index == 1) ? PreferredLanguage.Spanish :
                   (index == 2) ? PreferredLanguage.French :
                   (index == 3) ? PreferredLanguage.Portuguese : PreferredLanguage.English;
        }
    }
}
