using KyoS.Web.Data.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IReportHelper
    {
        #region Group functions
        Task<byte[]> GroupAsyncReport(int id);
        #endregion

        #region PSR Absense Notes reports
        Stream LarkinAbsenceNoteReport(Workday_Client workdayClient);
        Stream DreamsMentalHealthAbsenceNoteReport(Workday_Client workdayClient);
        Stream SolAndVidaAbsenceNoteReport(Workday_Client workdayClient);
        Stream DavilaAbsenceNoteReport(Workday_Client workdayClient);
        Stream AdvancedGroupMCAbsenceNoteReport(Workday_Client workdayClient);
        Stream AtlanticGroupMCAbsenceNoteReport(Workday_Client workdayClient);
        Stream FloridaSocialHSAbsenceNoteReport(Workday_Client workdayClient);
        Stream DemoClinic1AbsenceNoteReport(Workday_Client workdayClient);
        Stream DemoClinic2AbsenceNoteReport(Workday_Client workdayClient);
        Stream CommunityHTCAbsenceNoteReport(Workday_Client workdayClient);
        Stream PrincipleCCIAbsenceNoteReport(Workday_Client workdayClient);
        Stream SapphireMHCAbsenceNoteReport(Workday_Client workdayClient);
        #endregion

        #region Approved PSR Notes reports
        Stream FloridaSocialHSNoteReportSchema3(Workday_Client workdayClient);
        Stream FloridaSocialHSNoteReportSchema3SS(Workday_Client workdayClient);
        Stream DavilaNoteReportSchema4(Workday_Client workdayClient);
        Stream DreamsMentalHealthNoteReportSchema3(Workday_Client workdayClient);
        Stream DreamsMentalHealthNoteReportSchema3SS(Workday_Client workdayClient);
        Stream CommunityHTCNoteReportSchema3(Workday_Client workdayClient);
        Stream CommunityHTCNoteReportSchema3SS(Workday_Client workdayClient);
        Stream PrincipleCCINoteReportSchema3(Workday_Client workdayClient);
        Stream PrincipleCCINoteReportSchema3SS(Workday_Client workdayClient);
        Stream SapphireMHCNoteReportSchema3(Workday_Client workdayClient);
        Stream SapphireMHCNoteReportSchema3SS(Workday_Client workdayClient);
        #endregion

        #region Approved Individual Notes reports
        Stream DavilaIndNoteReportSchema1(Workday_Client workdayClient);
        Stream FloridaSocialHSIndNoteReportSchema1(Workday_Client workdayClient);
        Stream DreamsMentalHealthIndNoteReportSchema1(Workday_Client workdayClient);
        Stream CommunityHTCIndNoteReportSchema1(Workday_Client workdayClient);
        Stream PrincipleCCIIndNoteReportSchema1(Workday_Client workdayClient);
        Stream SapphireMHCIndNoteReportSchema1(Workday_Client workdayClient);
        #endregion

        #region Approved Group Notes reports
        Stream DavilaGroupNoteReportSchema1(Workday_Client workdayClient);
        Stream FloridaSocialHSGroupNoteReportSchema1(Workday_Client workdayClient);
        Stream DreamsMentalHealthGroupNoteReportSchema1(Workday_Client workdayClient);
        Stream FloridaSocialHSGroupNoteReportSchema2(Workday_Client workdayClient);
        Stream DreamsMentalHealthGroupNoteReportSchema2(Workday_Client workdayClient);
        Stream CommunityHTCGroupNoteReportSchema2(Workday_Client workdayClient);
        Stream PrincipleCCIGroupNoteReportSchema2(Workday_Client workdayClient);
        Stream SapphireMHCGroupNoteReportSchema2(Workday_Client workdayClient);
        Stream FloridaSocialHSGroupNoteReportSchema3(Workday_Client workdayClient);
        Stream DreamsMentalHealthGroupNoteReportSchema3(Workday_Client workdayClient);
        Stream CommunityHTCGroupNoteReportSchema3(Workday_Client workdayClient);
        Stream PrincipleCCIGroupNoteReportSchema3(Workday_Client workdayClient);
        Stream SapphireMHCGroupNoteReportSchema3(Workday_Client workdayClient);
        #endregion

        #region MTP reports
        Stream LarkinMTPReport(MTPEntity mtp);
        Stream SolAndVidaMTPReport(MTPEntity mtp);
        Stream DreamsMentalHealthMTPReport(MTPEntity mtp);
        Stream AdvancedGroupMCMTPReport(MTPEntity mtp);
        Stream AtlanticGroupMCMTPReport(MTPEntity mtp);
        Stream FloridaSocialHSMTPReport(MTPEntity mtp);
        Stream DavilaMTPReport(MTPEntity mtp);
        Stream DemoClinic1MTPReport(MTPEntity mtp);
        Stream DemoClinic2MTPReport(MTPEntity mtp);
        Stream CommunityHTCMTPReport(MTPEntity mtp);
        Stream PrincipleCCIMTPReport(MTPEntity mtp);
        Stream SapphireMHCMTPReport(MTPEntity mtp);
        #endregion

        #region PSR general reports
        Stream DailyAssistanceReport(List<Workday_Client> workdayClientList);
        Stream PrintIndividualSign(List<Workday_Client> workdayClientList);
        Stream DailyAssistanceReportGroup(List<Workday_Client> workdayClientList);
        Stream PrintIndividualSignGroup(List<Workday_Client> workdayClientList);
        #endregion

        #region Intake reports
        Stream FloridaSocialHSIntakeReport(IntakeScreeningEntity intake);
        Stream DreamsMentalHealthIntakeReport(IntakeScreeningEntity intake);
        Stream CommunityHTCIntakeReport(IntakeScreeningEntity intake);
        Stream PrincipleCCIIntakeReport(IntakeScreeningEntity intake);
        Stream SapphireMHCIntakeReport(IntakeScreeningEntity intake);
        #endregion

        #region Fars reports
        Stream FloridaSocialHSFarsReport(FarsFormEntity intake);
        Stream DreamsMentalHealthFarsReport(FarsFormEntity intake);
        Stream CommunityHTCFarsReport(FarsFormEntity intake);
        Stream PrincipleCCIFarsReport(FarsFormEntity intake);
        Stream SapphireMHCFarsReport(FarsFormEntity intake);
        #endregion

        #region Discharge reports
        Stream FloridaSocialHSDischargeReport(DischargeEntity intake);
        Stream DreamsMentalHealthDischargeReport(DischargeEntity intake);
        Stream CommunityHTCDischargeReport(DischargeEntity intake);
        Stream PrincipleCCIDischargeReport(DischargeEntity intake);
        Stream SapphireMHCDischargeReport(DischargeEntity intake);
        #endregion

        #region Bio reports
        Stream FloridaSocialHSBioReport(BioEntity bio);
        Stream DreamsMentalHealthBioReport(BioEntity bio);
        Stream CommunityHTCBioReport(BioEntity bio);
        Stream PrincipleCCIBioReport(BioEntity bio);
        Stream SapphireMHCBioReport(BioEntity bio);
        #endregion

        #region Addendum reports
        Stream FloridaSocialHSAddendumReport(AdendumEntity addendum);
        Stream DreamsMentalHealthAddendumReport(AdendumEntity addendum);
        Stream CommunityHTCAddendumReport(AdendumEntity addendum);
        Stream PrincipleCCIAddendumReport(AdendumEntity addendum);
        Stream SapphireMHCAddendumReport(AdendumEntity addendum);
        #endregion

        #region MTP Review reports
        Stream FloridaSocialHSMTPReviewReport(MTPReviewEntity review);
        Stream DreamsMentalHealthMTPReviewReport(MTPReviewEntity review);
        Stream CommunityHTCMTPReviewReport(MTPReviewEntity review);
        Stream PrincipleCCIMTPReviewReport(MTPReviewEntity review);
        Stream SapphireMHCMTPReviewReport(MTPReviewEntity review);
        #endregion

        #region Medical History
        Stream FloridaSocialHSMedicalHistoryReport(IntakeMedicalHistoryEntity medicalHistory);
        Stream DreamsMentalHealthMedicalHistoryReport(IntakeMedicalHistoryEntity medicalHistory);
        Stream CommunityHTCMedicalHistoryReport(IntakeMedicalHistoryEntity medicalHistory);
        Stream PrincipleCCIMedicalHistoryReport(IntakeMedicalHistoryEntity medicalHistory);
        Stream SapphireMHCMedicalHistoryReport(IntakeMedicalHistoryEntity medicalHistory);
        #endregion

        #region Brief reports
        Stream FloridaSocialHSBriefReport(BriefEntity brief);
        Stream DreamsMentalHealthBriefReport(BriefEntity brief);
        Stream CommunityHTCBriefReport(BriefEntity brief);
        Stream PrincipleCCIBriefReport(BriefEntity brief);
        Stream SapphireMHCBriefReport(BriefEntity brief);
        #endregion

        #region Utils functions
        byte[] ConvertStreamToByteArray(Stream stream);
        #endregion

        #region Approved TCM Notes reports
        Stream FloridaSocialHSTCMNoteReportSchema1(TCMNoteEntity note);
        Stream DreamsMentalHealthTCMNoteReportSchema1(TCMNoteEntity note);
        #endregion

        #region TCM Service Plan
        Stream FloridaSocialHSTCMServicePlan(TCMServicePlanEntity servicePlan);
        Stream DreamsMentalHealthTCMServicePlan(TCMServicePlanEntity servicePlan);
        #endregion

        #region TCM Fars Form
        Stream TCMFloridaSocialHSFarsReport(TCMFarsFormEntity intake);
        Stream TCMDreamsMentalHealthFarsReport(TCMFarsFormEntity intake);
        #endregion

        #region TCM Binder Section #1
        Stream TCMIntakeFormReport(TCMIntakeFormEntity intakeForm);
        Stream TCMIntakeConsentForTreatmentReport(TCMIntakeConsentForTreatmentEntity intakeConsentForTreatment);
        Stream TCMIntakeConsentForRelease(TCMIntakeConsentForReleaseEntity intakeConsentForRelease);
        Stream TCMIntakeAdvancedDirective(TCMIntakeAdvancedDirectiveEntity intakeAdvanced);
        Stream TCMIntakeConsumerRights(TCMIntakeConsumerRightsEntity intakeConsumerRights);
        Stream TCMIntakeOrientationCheckList(TCMIntakeOrientationChecklistEntity intakeOrientation);
        Stream TCMIntakeAcknowledgementHippa(TCMIntakeAcknowledgementHippaEntity intakeAcknowledgement);
        Stream TCMIntakeForeignLanguage(TCMIntakeForeignLanguageEntity intakeForeignLanguage);
        Stream TCMIntakeWelcome(TCMIntakeWelcomeEntity intakeWelcome);
        #endregion

        #region TCM Binder Section #4
        Stream TCMIntakeAppendixJ(TCMIntakeAppendixJEntity intakeAppendixJ);
        Stream TCMDischarge(TCMDischargeEntity intakeDischarge);
        #endregion
    }
}
