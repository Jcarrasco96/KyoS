using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class WorkdayEntity
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = false)]
        public DateTime Date { get; set; }
        public WeekEntity Week { get; set; }
        public ICollection<Workday_Client> Workdays_Clients { get; set; }
        public string Day 
        {
            get
            {
                return Date.DayOfWeek.ToString();
            } 
        }

    }
}
