using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum EffectivenessType
    {
        Effective,
        Highly_effective,
        Somewhat_effective,
        slightly_effective,
        Not_at_all_effective,
        unable_to_evaluate,
        In_progress,
        Successful
    }

    public class EffectivenessUtils
    {
        public static EffectivenessType GetEffectivenessByIndex(int index)
        {
            return (index == 0) ? EffectivenessType.Effective :
                   (index == 1) ? EffectivenessType.Highly_effective :
                   (index == 2) ? EffectivenessType.Somewhat_effective :
                   (index == 3) ? EffectivenessType.slightly_effective :
                   (index == 4) ? EffectivenessType.Not_at_all_effective :
                   (index == 5) ? EffectivenessType.unable_to_evaluate :
                   (index == 6) ? EffectivenessType.In_progress :
                   (index == 7) ? EffectivenessType.Successful : EffectivenessType.Effective;
        }
    }
}
