using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class Workday_ClientViewModel : Workday_Client
    {
        public int Origin { get; set; }
        public string DateInterval { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Schedule")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a schedule.")]
        public int IdSchedule { get; set; }
        public IEnumerable<SelectListItem> Schedules { get; set; }
    }
}
