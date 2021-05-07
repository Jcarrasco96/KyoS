using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IReportHelper
    {
        Task<byte[]>  GroupAsyncReport(int id);
        Stream DailyAssistanceReport(List<Workday_Client> workdayClientList);
        Stream PrintIndividualSign(List<Workday_Client> workdayClientList);
        Stream LarkinAbsenceNoteReport(Workday_Client workdayClient);
        Stream HealthAndBeautyAbsenceNoteReport(Workday_Client workdayClient);
        Stream SolAndVidaAbsenceNoteReport(Workday_Client workdayClient);
        Stream DavilaAbsenceNoteReport(Workday_Client workdayClient);
        Stream AdvancedGroupMCAbsenceNoteReport(Workday_Client workdayClient);
        Stream LarkinMTPReport(MTPEntity mtp);
        Stream SolAndVidaMTPReport(MTPEntity mtp);
        Stream HealthAndBeautyMTPReport(MTPEntity mtp);
        Stream AdvancedGroupMCMTPReport(MTPEntity mtp);
        Stream DavilaMTPReport(MTPEntity mtp);
    }
}
