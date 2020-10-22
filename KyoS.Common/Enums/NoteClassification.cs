namespace KyoS.Common.Enums
{
    public enum NoteClassification
    {
        Depressed,
        Negativistic,
        Sadness,
        Anxious,
        SleepProblems,
        Insomnia,
        Socialization,
        Isolation,
        Community,
        Motivation,
        Irritable,
        SelfEsteem,
        Concentration,
        Memory,
        Independent,
        MedicalManagenent,
        SelfCare,
        PositiveSelfTalk,
        NegativeSelfTalk
    }
    public class NoteClassificationUtils
    {
        public static NoteClassification GetClassificationByIndex(int index)
        {
            return (index == 1) ? NoteClassification.Depressed :
                   (index == 2) ? NoteClassification.Negativistic :
                   (index == 3) ? NoteClassification.Sadness :
                   (index == 4) ? NoteClassification.Anxious :
                   (index == 5) ? NoteClassification.SleepProblems :
                   (index == 6) ? NoteClassification.Insomnia :
                   (index == 7) ? NoteClassification.Socialization :
                   (index == 8) ? NoteClassification.Isolation :
                   (index == 9) ? NoteClassification.Community :
                   (index == 10) ? NoteClassification.Motivation :
                   (index == 11) ? NoteClassification.Irritable :
                   (index == 12) ? NoteClassification.SelfEsteem :
                   (index == 13) ? NoteClassification.Concentration :
                   (index == 14) ? NoteClassification.Memory :
                   (index == 15) ? NoteClassification.Independent :
                   (index == 16) ? NoteClassification.MedicalManagenent :
                   (index == 17) ? NoteClassification.SelfCare :
                   (index == 18) ? NoteClassification.PositiveSelfTalk :
                   (index == 19) ? NoteClassification.NegativeSelfTalk : NoteClassification.Depressed;
        }
    }
}
