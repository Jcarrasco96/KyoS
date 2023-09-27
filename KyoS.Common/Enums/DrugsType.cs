using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum DrugsType
    {
        Alcohol,
        Amphetamine_Meth,
        Barbiturates,
        Benzodiazepines,
        Caffeine,
        Cocaina_Crack,
        Hallucinogens,
        Heroin,
        Inhalants_Solvents,
        LSD,
        Marijuana_Hashish,
        MDMA_Ecstasy,
        Nicotne,
        Opium,
        PrescriptionDrugs
    }

    public class DrugsUtils
    {
        public static DrugsType GetEffectivenessByIndex(int index)
        {
            return (index == 0) ? DrugsType.Alcohol :
                   (index == 1) ? DrugsType.Amphetamine_Meth:
                   (index == 2) ? DrugsType.Barbiturates :
                   (index == 3) ? DrugsType.Benzodiazepines :
                   (index == 4) ? DrugsType.Caffeine :
                   (index == 5) ? DrugsType.Cocaina_Crack :
                   (index == 6) ? DrugsType.Hallucinogens :
                   (index == 7) ? DrugsType.Heroin :
                   (index == 8) ? DrugsType.Inhalants_Solvents :
                   (index == 9) ? DrugsType.LSD :
                   (index == 10) ? DrugsType.Marijuana_Hashish :
                   (index == 11) ? DrugsType.MDMA_Ecstasy :
                   (index == 12) ? DrugsType.Nicotne :
                   (index == 13) ? DrugsType.Opium :
                   (index == 14) ? DrugsType.PrescriptionDrugs : DrugsType.Alcohol;
        }
    }
}
