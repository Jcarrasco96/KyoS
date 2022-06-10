namespace KyoS.Common.Enums
{
    public enum DocumentDescription
    {
        Psychiatrist_evaluation,
        Intake,
        Bio,
        Fars,
        MTP,
        Addendum,
        MTP_review,
        Consent,
        Identification,
        MedicaidCard,
        MedicareCard,
        Social,
        Referrals,
        MentalStateExamination,
        AdicionalClinicDocumentation,
        FollowUps,
        YearlyPhysical,
        MedicalReports,
        Others
    }
    public class DocumentUtils
    {
        public static DocumentDescription GetDocumentByIndex(int index)
        {
            return (index == 0) ? DocumentDescription.Psychiatrist_evaluation :
                   (index == 1) ? DocumentDescription.Intake :
                   (index == 2) ? DocumentDescription.Bio :
                   (index == 3) ? DocumentDescription.Fars :
                   (index == 4) ? DocumentDescription.MTP :
                   (index == 5) ? DocumentDescription.Addendum :
                   (index == 6) ? DocumentDescription.MTP_review :
                   (index == 7) ? DocumentDescription.Consent :
                   (index == 8) ? DocumentDescription.Identification :
                   (index == 9) ? DocumentDescription.MedicaidCard :
                   (index == 10) ? DocumentDescription.MedicareCard :
                   (index == 11) ? DocumentDescription.Social :
                   (index == 12) ? DocumentDescription.Referrals :
                   (index == 13) ? DocumentDescription.MentalStateExamination :
                   (index == 14) ? DocumentDescription.AdicionalClinicDocumentation :
                   (index == 15) ? DocumentDescription.FollowUps :
                   (index == 16) ? DocumentDescription.YearlyPhysical :
                   (index == 17) ? DocumentDescription.MedicalReports :
                   (index == 18) ? DocumentDescription.Others : DocumentDescription.Others;
        }
    }
}
