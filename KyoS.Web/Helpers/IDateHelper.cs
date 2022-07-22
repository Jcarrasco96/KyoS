using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IDateHelper
    {
        int GetWeekOfYear(DateTime date);
        DateTime FirstDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci);
        void InitDateFinalDateOfWeekFromDate(DateTime date, ref DateTime initDate, ref DateTime finalDate);
    }        
}
