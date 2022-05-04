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
        #endregion

        #region Approved PSR Notes reports
        Stream FloridaSocialHSNoteReportSchema3(Workday_Client workdayClient);
        Stream DavilaNoteReportSchema4(Workday_Client workdayClient);
        Stream DreamsMentalHealthNoteReportSchema3(Workday_Client workdayClient);
        #endregion

        #region Approved Individual Notes reports
        Stream DavilaIndNoteReportSchema1(Workday_Client workdayClient);
        Stream FloridaSocialHSIndNoteReportSchema1(Workday_Client workdayClient);
        #endregion

        #region Approved Group Notes reports
        Stream DavilaGroupNoteReportSchema1(Workday_Client workdayClient);
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
        #endregion

        #region PSR general reports
        Stream DailyAssistanceReport(List<Workday_Client> workdayClientList);
        Stream PrintIndividualSign(List<Workday_Client> workdayClientList);
        #endregion

        #region Intake reports
        Stream FloridaSocialHSIntakeReport(IntakeScreeningEntity intake);
        #endregion

        #region Fars reports
        Stream FloridaSocialHSFarsReport(FarsFormEntity intake);
        #endregion

        #region Discharge reports
        Stream FloridaSocialHSDischargeReport(DischargeEntity intake);
        #endregion

        #region Utils functions
        byte[] ConvertStreamToByteArray(Stream stream);        
        #endregion
    }
}
