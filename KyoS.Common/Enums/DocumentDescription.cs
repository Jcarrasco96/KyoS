namespace KyoS.Common.Enums
{
    public enum DocumentDescription
    {
        Psychiatrist_evaluation,
        Intake,
        Bio,
        MTP,
        Fars,
        MTP_review,
        Consent,
        Others
    }
    public class DocumentUtils
    {
        public static DocumentDescription GetDocumentByIndex(int index)
        {
            return (index == 0) ? DocumentDescription.Psychiatrist_evaluation :
                   (index == 1) ? DocumentDescription.Intake :
                   (index == 2) ? DocumentDescription.Bio :
                   (index == 3) ? DocumentDescription.MTP :
                   (index == 4) ? DocumentDescription.Fars :
                   (index == 5) ? DocumentDescription.MTP_review :
                   (index == 6) ? DocumentDescription.Consent :
                   (index == 7) ? DocumentDescription.Others : DocumentDescription.Others;
        }
    }
}
