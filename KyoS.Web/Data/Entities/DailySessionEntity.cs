using KyoS.Common.Enums;
using System;

namespace KyoS.Web.Data.Entities
{
    public class DailySessionEntity
    {
        public int Id { get; set; }

        public DayOfWeekType Day { get; set; }

        public DateTime Date { get; set; }

        public GroupEntity Group { get; set; }
    }
}
