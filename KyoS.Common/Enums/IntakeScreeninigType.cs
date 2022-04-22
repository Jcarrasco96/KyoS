using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum IntakeClientIsStatus
    {
        Clean,
        PoorlyDressed,
        Flamboyant,
        PoorADLs,
        Disheveled,
        NeatlyDressed
    }
    public enum IntakeBehaviorIsStatus
    {
        Normal,
        Hyperactive,
        WithDrawn,
        ResistantOrAggressive
    }
    public enum IntakeSpeechIsStatus
    {
        Normal,
        Rapid,
        Slow,
        SlurredOrIncoherent
    }
    public class IntakeScreeninigType
    {
        public static IntakeClientIsStatus GetClientIsByIndex(int index)
        {
            return (index == 1) ? IntakeClientIsStatus.Clean :
                   (index == 2) ? IntakeClientIsStatus.PoorlyDressed :
                   (index == 3) ? IntakeClientIsStatus.Flamboyant :
                   (index == 4) ? IntakeClientIsStatus.PoorADLs :
                   (index == 5) ? IntakeClientIsStatus.Disheveled :
                   (index == 6) ? IntakeClientIsStatus.NeatlyDressed: IntakeClientIsStatus.Clean;
        }

        public static IntakeBehaviorIsStatus GetBehaviorIsByIndex(int index)
        {
            return (index == 1) ? IntakeBehaviorIsStatus.Normal :
                   (index == 2) ? IntakeBehaviorIsStatus.Hyperactive :
                   (index == 3) ? IntakeBehaviorIsStatus.WithDrawn :
                   (index == 4) ? IntakeBehaviorIsStatus.ResistantOrAggressive : IntakeBehaviorIsStatus.Normal;
        }

        public static IntakeSpeechIsStatus GetSpeechIsByIndex(int index)
        {
            return (index == 1) ? IntakeSpeechIsStatus.Normal :
                   (index == 2) ? IntakeSpeechIsStatus.Rapid :
                   (index == 3) ? IntakeSpeechIsStatus.Slow :
                   (index == 4) ? IntakeSpeechIsStatus.SlurredOrIncoherent : IntakeSpeechIsStatus.Normal;
        }
    }
}
