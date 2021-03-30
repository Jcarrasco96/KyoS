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
        Task<byte[]>  DailyAssistanceAsyncReport(List<Workday_Client> workdayClientList);
        Stream LarkinAbsenceNoteReport(Workday_Client workdayClient);
        Stream SolAndVidaAbsenceNoteReport(Workday_Client workdayClient);
        Stream DavilaAbsenceNoteReport(Workday_Client workdayClient);
    }
}
