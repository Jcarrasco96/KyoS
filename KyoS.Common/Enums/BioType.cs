using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum Bio_Appetite
    {
        Diminished,
        Increased,
        WNL,
        Anorexia
    }
    public enum Bio_Hydration
    {
        Diminished,
        IncreaseFluids,
        RestrictFluids,
        WNL,
        Inadequate
    }
    public enum Bio_RecentWeightChange
    {
        Intended,
        Unintended,
        Gained,
        Lost,
        N_A
    }
    public enum Bio_IfSexuallyActive
    {
        YES,
        NO,
        N_A
    }
    public enum Bio_Type
    {
        BIO,
        BRIEF
    }
    public class BioType
    {
        public static Bio_Appetite GetBioAppetiteByIndex(int index)
        {
            return (index == 1) ? Bio_Appetite.Diminished :
                   (index == 2) ? Bio_Appetite.Increased :
                   (index == 3) ? Bio_Appetite.WNL :
                   (index == 4) ? Bio_Appetite.Anorexia : Bio_Appetite.Diminished;
        }

        public static Bio_Hydration GetBioHydrationByIndex(int index)
        {
            return (index == 1) ? Bio_Hydration.Diminished :
                   (index == 2) ? Bio_Hydration.IncreaseFluids :
                   (index == 3) ? Bio_Hydration.RestrictFluids :
                   (index == 4) ? Bio_Hydration.WNL :
                   (index == 5) ? Bio_Hydration.Inadequate : Bio_Hydration.Diminished;
        }

        public static Bio_RecentWeightChange GetBioRecentWeightChangeByIndex(int index)
        {
            return (index == 1) ? Bio_RecentWeightChange.Intended :
                   (index == 2) ? Bio_RecentWeightChange.Unintended :
                   (index == 3) ? Bio_RecentWeightChange.Gained :
                   (index == 4) ? Bio_RecentWeightChange.Lost :
                   (index == 5) ? Bio_RecentWeightChange.N_A : Bio_RecentWeightChange.Intended;
        }
        public static Bio_IfSexuallyActive GetBioIfSexuallyActiveByIndex(int index)
        {
            return (index == 1) ? Bio_IfSexuallyActive.YES :
                   (index == 2) ? Bio_IfSexuallyActive.NO :
                   (index == 3) ? Bio_IfSexuallyActive.N_A : Bio_IfSexuallyActive.N_A;
        }
        public static Bio_Type GetBioTypeByIndex(int index)
        {
            return (index == 0) ? Bio_Type.BIO :
                   (index == 1) ? Bio_Type.BRIEF : Bio_Type.BIO;
        }
    }
}
