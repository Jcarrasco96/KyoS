﻿using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class WorkdayEntity : AuditableEntity
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public WeekEntity Week { get; set; }
        public ICollection<Workday_Client> Workdays_Clients { get; set; }
        public ICollection<Workday_Activity_Facilitator> Workdays_Activities_Facilitators { get; set; }
        public string Day => Date.DayOfWeek.ToString();
        public ServiceType Service { get; set; }
    }
}